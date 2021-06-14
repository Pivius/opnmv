using System;
using System.Threading;
using System.Numerics;
using Sandbox;

namespace OMMovement
{
    public class Friction : Accelerate
    {
		// # Source Movement Friction
		
		public virtual Vector3 ApplyFriction(Vector3 velocity, float friction, float stop_speed)
		{
			float speed = velocity.Length;
			float control = MathF.Max(speed, stop_speed);
			float drop = control * friction * Time.Delta;
			float new_speed = MathF.Max(speed - drop, 0);

			if (new_speed != speed)
			{
				new_speed /= speed;
				velocity *= new_speed;
			}

			return velocity;
		}

		public override Vector3 Move(Vector3 velocity, BaseProperties props, Vector3 strafe_vel = new Vector3(), Vector3 position = new Vector3())
		{
			return ApplyFriction(velocity, props.Friction, props.StopSpeed);
		}
    }
}
