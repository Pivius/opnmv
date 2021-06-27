using Sandbox;

namespace OMMovement
{
	public class QuakeAirAccelerate : AirAccelerate
	{
		// # Vanilla Quake 3 Air Accel

		public override float GetVelDiff(Vector3 velocity, float length, Vector3 strafe_dir)
		{
			return (length) - velocity.Dot(strafe_dir);
		}

		public virtual float AdjustAccel(float dot_vel, float accel, float strafe_accel, float air_stop_accel)
		{
			if (dot_vel < 0.0f || (Input.Forward != 0.0f && Input.Left == 0))
				accel = air_stop_accel;

			if (Input.Left != 0 && Input.Forward == 0)
				accel = strafe_accel;

			return accel;
		}

		public virtual Vector3 GetFinalVelocity(Vector3 velocity, Vector3 strafe_vel, float strafe_vel_length, float accel, float strafe_accel, float air_stop_accel)
		{
			Vector3 strafe_dir = strafe_vel.Normal;
			float vel_diff = GetVelDiff(velocity, strafe_vel_length, strafe_dir);
			Vector3 accel_speed = GetAccelSpeed(strafe_dir, strafe_vel_length, vel_diff, AdjustAccel(velocity.Dot(strafe_dir), accel, strafe_accel, air_stop_accel));

			return velocity + accel_speed;
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
				velocity = velocity.Normal;
				dot_vel = velocity.Dot(strafe_dir);
				k *= (air_control * dot_vel * dot_vel * Time.Delta);

				if (dot_vel > 0)
					velocity = ((velocity * vel_length) + (strafe_dir * k)).Normal;

				velocity *= vel_length;
				velocity = velocity.WithZ(z_speed);
			}

			return velocity;
		}

		public override void Move(MovementController controller, Vector3 strafe_vel = new Vector3())
		{
			var props = controller.Properties;
			var velocity = controller.Velocity;
			float strafe_vel_length = CapWishSpeed(strafe_vel.Length, props.MaxSpeed);

			if (Input.Left != 0 && Input.Forward == 0)
				strafe_vel_length = CapWishSpeed(strafe_vel.Length, props.SideStrafeMaxSpeed);

			controller.Velocity = GetFinalVelocity(velocity, strafe_vel, strafe_vel_length, props.AirAccelerate, props.StrafeAcceleration, props.AirStopAcceleration);

			if (props.AirControl > 0)
				controller.Velocity = AirControl(controller.Velocity, strafe_vel.Normal, props.AirControl);
		}
	}
}
