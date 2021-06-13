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

		public override void Move(ref Vector3 Velocity, BaseProperties Properties, Vector3 strafe_vel)
		{
			Velocity = GetFinalVelocity(Velocity, strafe_vel, Properties.MaxSpeed, Properties.Accelerate);
		}
    }
}
