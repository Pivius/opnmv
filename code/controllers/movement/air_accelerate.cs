using System.Threading;
using System.Numerics;
using Sandbox;

namespace OMMovement
{
	public class AirAccelerate
	{
		public virtual float GetWishSpeed(float wish_speed, float max_speed)
		{
			return MathX.Clamp(wish_speed, 0, max_speed);
		}

		public virtual float GetVelDiff(float length, Vector3 velocity, Vector3 wish_dir)
		{
			return (length/10.0f) - velocity.Dot(wish_dir);
		}

		public virtual Vector3 GetAccelSpeed(Vector3 wish_dir, float length, float vel_diff, float accel)
		{
			return (wish_dir * MathX.Clamp(length * accel * Time.Delta, 0, vel_diff));
		}

		public virtual Vector3 GetFinalVelocity(Vector3 wish_speed, float max_speed, Vector3 velocity, float accel)
		{
			float strafe_vel_length = GetWishSpeed(wish_speed.Length, max_speed);
			float vel_diff = GetVelDiff(strafe_vel_length, velocity, wish_speed.Normal);
			Vector3 accel_speed = GetAccelSpeed(wish_speed.Normal, strafe_vel_length, vel_diff, accel);

			return velocity + accel_speed;
		}
	}
}
