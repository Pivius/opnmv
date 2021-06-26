using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Sandbox;
using Core;

namespace OMMovement
{
	public abstract partial class MovementController : WalkController
	{
		// # Fields
		public AirAccelerate AirAccelerate;
		public Accelerate Accelerate;
		public Gravity Gravity;
		public Friction Friction;
		public Water Water;
		public Duck Duck;

		public float GetWalkSpeed()
		{
			float walk_speed = (GetPlayer().KeyDown(InputButton.Walk) && Properties.CanWalk) ? Properties.WalkSpeed : ((GetPlayer().KeyDown(InputButton.Run) && Properties.CanRun) ? Properties.RunSpeed : Properties.DefaultSpeed);
	
			return Duck.IsDucked ? walk_speed * (Properties.DuckedWalkSpeed/Properties.DefaultSpeed) : walk_speed;
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

		/// <summary>
		/// Consistent speed boosts when landing on a slope
		/// </summary>
		public virtual void AddSlopeSpeed()
		{
			TraceResult trace = TraceUtil.PlayerBBox(Position, Position.WithZ(Position.z - 2), this);
			Vector3 normal = trace.Normal;

			if (normal.z < 1 && Velocity.z <= 0 && OnGround() && Properties.MoveState == STATE.INAIR)
			{
				Velocity -= (normal * Velocity.Dot(normal));

				if (Velocity.Dot(normal) < 0)
					Velocity = ClipVelocity(Velocity, normal);
			}
		}

		/// <summary>
		/// Does sliding when on a ramp or surf
		/// </summary>
		public override void TryPlayerMove()
		{
			MoveHelper mover = new MoveHelper(Position, Velocity);
			mover.Trace = mover.Trace.Size(Properties.OBBMins, Properties.OBBMaxs).Ignore(Pawn);
			mover.MaxStandableAngle = Properties.StandableAngle;

			mover.TryMove(Time.Delta);

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public override void AirMove()
		{
			AirAccelerate.Move(this, WishVelocity);
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
			Accelerate.Move(this, WishVelocity);
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

				var pm = TraceUtil.PlayerBBox(Position, dest, this);

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
				// Now pull the base velocity back out.  Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			base.StayOnGround();
		}
	
		public virtual void CheckJumpButton()
			{

			if (Water.JumpTime > 0.0f)
			{
				Water.JumpTime -= Time.Delta;

				if (Water.JumpTime < 0.0f)
					Water.JumpTime = 0;

				return;
			}
			
			if (Water.WaterLevel >= WATERLEVEL.Waist)
			{
				ClearGroundEntity();
				Velocity = Velocity.WithZ(100);

				return;
			}
			
			if (!OnGround())
				return;

			ClearGroundEntity();
			Velocity = Velocity.WithZ( Velocity.z + Properties.JumpPower );
			Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);

			AddEvent( "jump" );
		}

		public override void CheckLadder()
		{
			if (IsTouchingLadder && GetPlayer().KeyPressed(InputButton.Jump))
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

		/// <summary>
		/// Runs first in the simulate method
		/// </summary>
		/// <returns>
		/// return true to not run anything past this event
		/// </returns>
		public virtual bool StartMove()
		{
			EyePosLocal = Vector3.Up * GetViewOffset() * Pawn.Scale;
			EyePosLocal += TraceOffset;
			EyeRot = Input.Rotation;
			WishVelocity = WishVel(Properties.MaxMove);
			UpdateBBox();
			RestoreGroundPos();
			Duck.TryDuck();
			
			if (Unstuck.TestAndFix())
				return true;
			
			// RunLadderMode
			CheckLadder();

			return false;
		}

		/// <summary>
		/// Runs in the middle of the simulate method
		/// </summary>
		/// <returns>
		/// return true to not run anything past this event
		/// </returns>
		public virtual bool SetupMove()
		{
			var player = GetPlayer();

			Swimming = Water.CheckWater(Position, Properties.OBBMins, Properties.OBBMaxs, GetViewOffset(), Pawn);

			//
			// Start Gravity
			//
			if (!Swimming && !IsTouchingLadder)
			{
				Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			if (Water.JumpTime > 0.0f)
			{
				Velocity = Water.WaterJump(Velocity);
				TryPlayerMove();
				return true;
			}

			if (Water.WaterLevel >= WATERLEVEL.Waist)
			{
				if (Water.WaterLevel == WATERLEVEL.Waist)
					Velocity = Water.CheckWaterJump(Velocity, Position, this);

				if (Velocity.z < 0.0f && Water.JumpTime > 0.0f)
					Water.JumpTime = 0.0f;

				if (Properties.AutoJump ? player.KeyDown(InputButton.Jump) : player.KeyPressed(InputButton.Jump))
					CheckJumpButton();
	
				Water.Move(this);
				base.CategorizePosition(OnGround());

				if (OnGround())
					Velocity = Velocity.WithZ(0);

				Properties.MoveState = STATE.WATER;
			}
			else
			{
		
				if (Properties.AutoJump ? player.KeyDown(InputButton.Jump) : player.KeyPressed(InputButton.Jump))
					CheckJumpButton();

				if (OnGround())
				{
					Velocity = Velocity.WithZ(0);
					Friction.Move(this);
				}

				if (!IsTouchingLadder)
					WishVelocity = WishVelocity.WithZ(0);

				bool bStayOnGround = false;
				
				if (IsTouchingLadder)
				{
					LadderMove();
					Properties.MoveState = STATE.LADDER;
				}
				else if (OnGround())
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

				// FinishGravity
				if (!IsTouchingLadder)
					Velocity = Gravity.AddGravity(Properties.Gravity * 0.5f, Velocity);

				if (OnGround())
				{
					AddSlopeSpeed();
					Velocity = Velocity.WithZ(0);
				}

				base.CategorizePosition(bStayOnGround);
			}
			return false;
		}

		/// <summary>
		/// Is the last to run in the simulate method
		/// </summary>
		public virtual void EndMove()
		{
			SaveGroundPos();
			Properties.OldVelocity = Velocity;
		}
	}
}
