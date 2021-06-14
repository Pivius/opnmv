using System.Threading;
using System.Numerics;
using Sandbox;

namespace OMMovement
{
    public class Accelerate : AirAccelerate
    {
		// Source Movement Accelerate

		public override float GetVelDiff(Vector3 velocity, float length, Vector3 wish_dir)
		{
			return length - velocity.Dot(wish_dir);
		}

		public override Vector3 Move(Vector3 velocity, BaseProperties props, Vector3 strafe_vel = new Vector3(), Vector3 position = new Vector3())
		{
			return GetFinalVelocity(velocity, strafe_vel, props.MaxSpeed, props.Accelerate);
		}
    }
}
