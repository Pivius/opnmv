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
		public Vector3 LadderNormal;
		public bool IsTouchingLadder = false;

		public MovementController()
		{
			
			Properties = new BaseProperties();
			Duck = new Sandbox.Duck(this);
			AirAccelerate = new AirAccelerate();
			Accelerate = new Accelerate();
			Gravity = new Gravity();
			Friction = new Friction();
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

		public override void SetBBox(Vector3 mins, Vector3 maxs)
		{
			Properties.OBBMins = mins;
			Properties.OBBMaxs = maxs;
		}

		public override void UpdateBBox()
		{
			var mins = Properties.StandMins * Pawn.Scale;
			var maxs = Properties.StandMaxs * Pawn.Scale;
			
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

		public override void FrameSimulate()
		{
			EyeRot = Input.Rotation;
		}

		public override void Simulate()
		{
			EyePosLocal = Vector3.Up * (EyeHeight * Pawn.Scale);
			EyePosLocal += TraceOffset;
			EyeRot = Input.Rotation;

			UpdateBBox();
			RestoreGroundPos();

			if (Unstuck.TestAndFix())
				return;

			// RunLadderMode
			CheckLadder();
			Swimming = Pawn.WaterLevel.Fraction > 0.6f;

			//
			// Start Gravity
			//
			if (!Swimming && !IsTouchingLadder)
			{
				Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);
			}

			if (Properties.AutoJump ? Input.Down(InputButton.Jump) : Input.Pressed(InputButton.Jump))
			{
				CheckJumpButton();
			}

			// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor, 
			//  we don't slow when standing still, relative to the conveyor.
			bool bStartOnGround = GroundEntity != null;

			if (bStartOnGround)
			{
				Velocity = Velocity.WithZ(0);

				if (GroundEntity != null)
				{
					Velocity = Friction.ApplyFriction(Velocity, Properties.Friction, Properties.StopSpeed);
				}
			}

			WishVelocity = WishVel(Properties.MaxSpeed);

			if (!Swimming && !IsTouchingLadder)
			{
				WishVelocity = WishVelocity.WithZ(0);
			}

			Duck.PreTick();
			bool bStayOnGround = false;

			if (Swimming)
			{
				Velocity = Friction.ApplyFriction(Velocity, 1, Properties.StopSpeed);
				WaterMove();
			}
			else if (IsTouchingLadder)
			{
				LadderMove();
			}
			else if (GroundEntity != null)
			{
				bStayOnGround = true;
				WalkMove();
			}
			else
			{
				AirMove();
			}

			base.CategorizePosition(bStayOnGround);

			// FinishGravity
			if (!Swimming && !IsTouchingLadder)
			{
				Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);
			}


			if (GroundEntity != null)
			{
				Velocity = Velocity.WithZ(0);
			}

			SaveGroundPos();
			Properties.OldVelocity = Velocity;
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
			var velocity = Velocity;

			AirAccelerate.Move(ref velocity, Properties, WishVelocity);
			Velocity = velocity;
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
			var velocity = Velocity;

			WishVelocity = WishVelocity.WithZ(0);
			WishVelocity = WishVelocity.Normal * wishspeed;
			velocity = Velocity.WithZ(0);
			Accelerate.Move(ref velocity, Properties, WishVelocity);
			Velocity = velocity.WithZ(0);

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
