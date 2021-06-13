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

		public virtual void Move(ref Vector3 Velocity, BaseProperties Properties, Vector3 strafe_vel)
		{
			Velocity = GetFinalVelocity(Velocity, strafe_vel, Properties.MaxSpeed, Properties.AirAccelerate);
		}
	}

	public class QuakeAirAccelerate : AirAccelerate
	{
		// # Vanilla Quake 3 Air Accel

		public override float GetVelDiff(Vector3 velocity, float length, Vector3 strafe_dir)
		{
			return length - velocity.Dot(strafe_dir);
		}

		public virtual float AdjustAccel(float dot_vel, float accel, float strafe_accel, float air_stop_accel)
		{
			if (dot_vel < 0.0f || Input.Forward != 0.0f)
				accel = air_stop_accel;

			if (Input.Left != 0)
				accel = strafe_accel;
			else if (Input.Forward != 0.0f)
				accel = air_stop_accel;

			return accel;
		}

		public virtual Vector3 GetFinalVelocity(Vector3 velocity, Vector3 strafe_vel, float strafe_vel_length, float accel, float strafe_accel, float air_stop_accel)
		{
			Vector3 strafe_dir = strafe_vel.Normal;
			float vel_diff = GetVelDiff(velocity, strafe_vel_length, strafe_dir);
			Vector3 accel_speed = GetAccelSpeed(strafe_dir, strafe_vel_length, vel_diff, AdjustAccel(velocity.Dot(strafe_dir), accel, strafe_accel, air_stop_accel));
			
			return accel_speed * strafe_dir;
		}

		public virtual Vector3 AirControl(Vector3 velocity, Vector3 strafe_dir, float air_control)
		{
			if (Input.Forward >= 1 && Input.Left == 0)
			{
				float vel_length;
				float dot_vel;
				float z_speed = velocity.z;
				float k = 32;
				
				velocity = velocity.WithZ(0);
				vel_length = velocity.Length;
				dot_vel = velocity.Dot(strafe_dir);
				k = k * (air_control * (dot_vel * dot_vel) * Time.Delta);

				if (dot_vel > 0)
					velocity = ((velocity * vel_length) + (strafe_dir * k)).Normal;

				velocity *= vel_length;
				velocity = velocity.WithZ(z_speed);
			}

			return velocity;
		}

		// Defaul Max Speed - 300
		public virtual void Move(ref Vector3 velocity, QuakeProperties props, Vector3 strafe_vel)
		{
			float strafe_vel_length = CapWishSpeed(strafe_vel.Length, props.MaxSpeed);
			
			if (Input.Left != 0 && Input.Forward == 0)
				strafe_vel_length = CapWishSpeed(strafe_vel.Length, props.SideStrafeMaxSpeed);

			velocity = GetFinalVelocity(velocity, strafe_vel, strafe_vel_length, props.AirAccelerate, props.StrafeAcceleration, props.AirStopAcceleration);

			if (props.AirControl > 0)
				velocity = AirControl(velocity, strafe_vel.Normal, props.AirControl);
		}
	}
}
