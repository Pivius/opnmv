using System.Diagnostics;
using Sandbox;
using System;
using Core;

namespace OMMovement
{
	public class Walljump
	{
		// EMM Walljump
		public float WalljumpDistance = 30;
		public float WalljumpDelay = 0.2f;
		public float WalljumpSideVelocity = 260.0f;
		public float WalljumpUpVelocity = 200.0f;
		public float WalljumpAngle = 58.0f;
		public float WalljumpTime{get; set;} = 0;
		public bool CanWalljump = true;
		public bool CanWalljumpSky = false;
		protected ulong WALLJUMP_BUTTONS = ((ulong) InputButton.Forward | (ulong) InputButton.Right | (ulong) InputButton.Left | (ulong) InputButton.Back | (ulong) InputButton.Jump);
		public Walljump()
		{}

		public virtual bool CooledDown()
		{
			return Time.Now > (WalljumpTime + WalljumpDelay);
		}


		public virtual bool PressedWalljumpButton(MovementPlayer player)
		{
			return player.KeyPressed(WALLJUMP_BUTTONS);
		}

		public virtual float GetAngle(Vector3 trace_direction, Vector3 wall_normal)
		{
			return Vector3.GetAngle(trace_direction, wall_normal.WithZ(0));
		}

		public virtual TraceResult Trace(Vector3 position, Vector3 mins, Vector3 maxs, Vector3 direction, Entity Pawn)
		{
			Vector3 perimeter_pos = position - (Vector3.One * direction * 23);
			float walljump_distance = WalljumpDistance - maxs.x; // Works under the presumption the hulls x and y are identical
			Vector3 obb_trace = Vector3.One * walljump_distance;
			TraceResult trace;

			obb_trace.z = 0;
			perimeter_pos.x = MathX.Clamp(perimeter_pos.x, position.x + mins.x, position.x + maxs.x);
			perimeter_pos.y = MathX.Clamp(perimeter_pos.y, position.y + mins.y, position.y + maxs.y);
			perimeter_pos.z = position.z;

			trace = TraceUtil.Hull(position, perimeter_pos, -obb_trace, obb_trace, Pawn);
			return trace;
		}

		public virtual Vector3 GetAddVelocity(Vector3 direction)
		{
			return (direction * WalljumpSideVelocity).WithZ(WalljumpUpVelocity);
		}

		public virtual bool TryWalljump(MovementController controller, Vector3 direction)
		{
			TraceResult trace = Trace(controller.Position, controller.GetPlayerMins(), controller.GetPlayerMaxs(), direction, controller.Pawn);
			bool did_walljump;

			if (trace.Fraction < 1.0f && (WalljumpAngle > GetAngle(direction, trace.Normal)))
			{
				did_walljump = true;
				controller.Velocity += GetAddVelocity(direction);
				WalljumpTime = Time.Now;
			}
			else
				did_walljump = false;

			return did_walljump;
		}
		
		public virtual void Move(MovementController controller, Vector3 strafe_vel = new Vector3())
		{
			if (CanWalljump && controller.GetPlayer().KeyDown(InputButton.Jump) && PressedWalljumpButton(controller.GetPlayer()) && CooledDown())
			{
				Vector3 forward = Input.Rotation.Forward.WithZ(0).Normal;
				Vector3 right = new Vector3(forward.y, -forward.x, 0);
				bool did_walljump = false;
				var player = controller.GetPlayer();

				if (player.KeyDown(InputButton.Right))
					did_walljump = TryWalljump(controller, right);

				if (player.KeyDown(InputButton.Left) && !did_walljump)
					did_walljump = TryWalljump(controller, -right);

				if (player.KeyDown(InputButton.Forward) && !did_walljump)
					did_walljump = TryWalljump(controller, forward);

				if (player.KeyDown(InputButton.Back) && !did_walljump)
					did_walljump = TryWalljump(controller, -forward);
			}
		}
	}
}
