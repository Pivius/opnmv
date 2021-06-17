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

		public override void Move(MovementController controller, Vector3 strafe_vel = new Vector3())
		{
			controller.Velocity = GetFinalVelocity(controller.Velocity, strafe_vel, controller.GetWalkSpeed(), controller.Properties.Accelerate);
		}
    }
}
