using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Sandbox;
using Core;

namespace OMMovement
{
	public partial class MovementController : WalkController
	{
		public BaseProperties Properties;
		public AirAccelerate AirAccelerate;
		public Accelerate Accelerate;
		public Gravity Gravity;
		public Friction Friction;
		public Water Water;
		public Duck Duck;
		public Vector3 LadderNormal;
		public bool IsTouchingLadder = false;

		public MovementController()
		{
			
			Properties = new BaseProperties();
			Duck = new Duck(this);
			AirAccelerate = new AirAccelerate();
			Accelerate = new Accelerate();
			Gravity = new Gravity();
			Friction = new Friction();
			Water = new Water();
			Unstuck = new Sandbox.Unstuck(this);
		}

		public override TraceResult TraceBBox(Vector3 start, Vector3 end, float liftFeet = 0.0f)
		{
			return TraceBBox(start, end, Properties.OBBMins, Properties.OBBMaxs, liftFeet);
		}

		public override BBox GetHull()
		{
			return new BBox(Properties.OBBMins, Properties.OBBMaxs);
		}

		public virtual Vector3 GetPlayerMins(bool is_ducked)
		{
			return is_ducked ? Properties.DuckMins : Properties.StandMins;
		}

		public virtual Vector3 GetPlayerMaxs(bool is_ducked)
		{
			return is_ducked ? Properties.DuckMaxs : Properties.StandMaxs;
		}

		public virtual Vector3 GetPlayerMins()
		{
			return Duck.IsDucked ? Properties.DuckMins : Properties.StandMins;
		}

		public virtual Vector3 GetPlayerMaxs()
		{
			return Duck.IsDucked ? Properties.DuckMaxs : Properties.StandMaxs;
		}

		public virtual float GetPlayerViewOffset(bool is_ducked)
		{
			return is_ducked ? Properties.DuckViewOffset : Properties.StandViewOffset;
		}

		public virtual float GetViewOffset()
		{
			return Properties.ViewOffset;
		}

		public override void SetBBox(Vector3 mins, Vector3 maxs)
		{
			Properties.OBBMins = mins;
			Properties.OBBMaxs = maxs;
		}

		public override void UpdateBBox()
		{
			var mins = GetPlayerMins() * Pawn.Scale;
			var maxs = GetPlayerMaxs() * Pawn.Scale;

			if (Properties.OBBMins != mins || Properties.OBBMaxs != maxs)
			{
				SetBBox(mins, maxs);
			}
		}

		public bool OnGround()
		{
			return GroundEntity != null;
		}

		public float FallDamage()
		{
			return MathF.Max(Velocity.z - 580.0f, 0) * Properties.FallDamageMultiplier;
		}

		public Vector3 WishVel(float strafe_speed)
		{	
			Vector3 forward = Input.Rotation.Forward;
			float forward_speed = Input.Forward * strafe_speed;
			float side_speed = Input.Left * strafe_speed;
			Vector3 forward_wish = new Vector3(forward.x, forward.y, 0).Normal * forward_speed;
			Vector3 side_wish = new Vector3(-forward.y, forward.x, 0).Normal * side_speed;

			return forward_wish + side_wish;
		}

		public Vector3 ClipVelocity(Vector3 velocity, Vector3 normal)
		{	
			return velocity - (normal * velocity.Dot(normal));
		}

		public override void FrameSimulate()
		{
			EyeRot = Input.Rotation;
		}

		public virtual void AddSlopeSpeed()
		{
			TraceResult trace = TraceBBox(Position, Position.WithZ(Position.z - 2));
			Vector3 normal = trace.Normal;

			if (normal.z < 1 && Velocity.z <= 0 && GroundEntity != null && Properties.MoveState == STATE.INAIR)
			{
				float dot;

				Velocity -= (normal * Velocity.Dot(normal));
				dot = Velocity.Dot(normal);

				if (dot < 0)
				{
					Velocity -= (normal * dot);
					
				}
				
			}
		}

		public override void Simulate()
		{
			var is_ducked = false;
			EyePosLocal = Vector3.Up * GetViewOffset() * Pawn.Scale;
			EyePosLocal += TraceOffset;
			EyeRot = Input.Rotation;
	
			UpdateBBox();
			RestoreGroundPos();
			Duck.UpdateDuckJumpEyeOffset();
			Duck.TryDuck();
			if (Unstuck.TestAndFix())
				return;

			// RunLadderMode
			CheckLadder();
			Swimming = Water.CheckWater(Position, Properties.OBBMins, Properties.OBBMaxs, GetViewOffset(), Pawn);
			
			//
			// Start Gravity
			//
			if (!Swimming && !IsTouchingLadder)
			{
				Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);
			}

			if (Water.JumpTime > 0.0f)
			{
				Velocity = Water.WaterJump(Velocity);
				TryPlayerMove();
				return;
			}

			if (Water.WaterLevel >= WATERLEVEL.Waist)
			{
				if (Water.WaterLevel == WATERLEVEL.Waist)
					Velocity = Water.CheckWaterJump(Velocity, Position, Properties, Pawn);

				if (Velocity.z < 0.0f && Water.JumpTime > 0.0f)
					Water.JumpTime = 0.0f;

				if (Properties.AutoJump ? Input.Down(InputButton.Jump) : Input.Pressed(InputButton.Jump))
					CheckJumpButton();
	
				Water.Move(this);
				base.CategorizePosition(OnGround());

				if (OnGround())
					Velocity = Velocity.WithZ(0);

				Properties.MoveState = STATE.WATER;
			}
			else
			{
				
				if (Properties.AutoJump ? Input.Down(InputButton.Jump) : Input.Pressed(InputButton.Jump))
					CheckJumpButton();

				bool bStartOnGround = GroundEntity != null;

				if (bStartOnGround)
				{
					Velocity = Velocity.WithZ(0);

					if (GroundEntity != null)
						Velocity = Friction.Move(Velocity, Properties);
				}

				WishVelocity = WishVel(Properties.MaxMove);

				if (!IsTouchingLadder)
					WishVelocity = WishVelocity.WithZ(0);

				bool bStayOnGround = false;
				
				//if (Swimming)
				//{
					//Velocity = Friction.ApplyFriction(Velocity, 1, Properties.StopSpeed);
					//WaterMove();
					//Properties.MoveState = STATE.WATER;
				//}
				if (IsTouchingLadder)
				{
					LadderMove();
					Properties.MoveState = STATE.LADDER;
				}
				else if (GroundEntity != null)
				{
					bStayOnGround = true;
					WalkMove();
					Properties.MoveState = STATE.GROUND;
				}
				else
				{
					AirMove();
					Properties.MoveState = STATE.INAIR;
				}

				base.CategorizePosition(bStayOnGround);

				// FinishGravity
				if (!IsTouchingLadder)
					Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);


				if (GroundEntity != null)
				{
					AddSlopeSpeed();
					Velocity = Velocity.WithZ(0);
				}
			}
			
			SaveGroundPos();
			Properties.OldVelocity = Velocity;
		}

	public virtual void CheckJumpButton()
			{

			if ( GroundEntity == null )
				return;

			ClearGroundEntity();

			float flMul = 268.3281572999747f * 1.2f;
			float startz = Velocity.z;

			Velocity = Velocity.WithZ( startz + flMul );
			Velocity -= new Vector3( 0, 0, Properties.Gravity * 0.5f ) * Time.Delta;
			Duck.JumpTime = 0.0f;
			Duck.InDuckJump = false;

			AddEvent( "jump" );

		}

		public override void CheckLadder()
		{
			if (IsTouchingLadder && Input.Pressed(InputButton.Jump))
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;
			}

			const float ladderDistance = 1.0f;
			var start = Position;
			Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal) * ladderDistance;

			var pm = Trace.Ray(start, end)
						.Size(Properties.OBBMins, Properties.OBBMaxs)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.LADDER, true)
						.Ignore(Pawn)
						.Run();

			IsTouchingLadder = false;

			if (pm.Hit)
			{
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}
		public override void TryPlayerMove()
		{
			MoveHelper mover = new MoveHelper(Position, Velocity);
			mover.Trace = mover.Trace.Size(Properties.OBBMins, Properties.OBBMaxs).Ignore(Pawn);
			mover.MaxStandableAngle = GroundAngle;

			mover.TryMove(Time.Delta);

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public override void AirMove()
		{
			Velocity = AirAccelerate.Move(Velocity, Properties, WishVelocity);
			Velocity += BaseVelocity;
			TryPlayerMove();
			Velocity -= BaseVelocity;
		}


		public override void WaterMove()
		{
			var wish_speed = WishVelocity;
			var wish_dir = wish_speed.Normal;

			Velocity = Accelerate.GetFinalVelocity(Velocity, WishVelocity * 0.8f, Properties.MaxSpeed, Properties.WaterAccelerate);
			Velocity += BaseVelocity;
			TryPlayerMove();
			Velocity -= BaseVelocity;
		}

		public override void WalkMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ(0);
			WishVelocity = WishVelocity.Normal * wishspeed;
			Velocity = Velocity.WithZ(0);
			Velocity = Accelerate.Move(Velocity, Properties, WishVelocity);
			Velocity = Velocity.WithZ(0);

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if (Velocity.Length < 1.0f)
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination	
				var dest = (Position + Velocity * Time.Delta).WithZ(Position.z);

				var pm = TraceBBox(Position, dest);

				if (pm.Fraction == 1)
				{
					Position = pm.EndPos;
					base.StayOnGround();
					return;
				}

				base.StepMove();
			}
			finally
			{

				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			base.StayOnGround();
		}

		void RestoreGroundPos()
		{
			if (GroundEntity == null || GroundEntity.IsWorld)
				return;
		}

		void SaveGroundPos()
		{
			if (GroundEntity == null || GroundEntity.IsWorld)
				return;
		}
	}
}
