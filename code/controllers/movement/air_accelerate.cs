using System.Threading;
using System.Numerics;
using Sandbox;

namespace OMMovement
{
	public class AirAccelerate
	{
		// # Source Movement Air Accel

		public virtual float CapWishSpeed(float wish_speed, float max_speed)
		{
			return MathX.Clamp(wish_speed, 0, max_speed);
		}

		public virtual float GetVelDiff(Vector3 velocity, float length, Vector3 strafe_dir)
		{
			return (length/10.0f) - velocity.Dot(strafe_dir);
		}

		public virtual Vector3 GetAccelSpeed(Vector3 strafe_dir, float length, float vel_diff, float accel)
		{
			return (strafe_dir * MathX.Clamp(length * accel * Time.Delta, 0, vel_diff));
		}

		public virtual Vector3 GetFinalVelocity(Vector3 velocity, Vector3 strafe_vel, float max_speed, float accel)
		{
			Vector3 strafe_dir = strafe_vel.Normal;
			float strafe_vel_length = CapWishSpeed(strafe_vel.Length, max_speed);
			float vel_diff = GetVelDiff(velocity, strafe_vel_length, strafe_dir);
			Vector3 accel_speed = GetAccelSpeed(strafe_dir, strafe_vel_length, vel_diff, accel);

			return velocity + accel_speed;
		}

		public virtual void Move(MovementController controller, Vector3 strafe_vel = new Vector3())
		{
			controller.Velocity = GetFinalVelocity(controller.Velocity, strafe_vel, controller.Properties.MaxSpeed, controller.Properties.AirAccelerate);
		}
	}
}
