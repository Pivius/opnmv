using System.Threading;
using System.Numerics;
using Sandbox;

namespace OMMovement
{
    public class Accelerate : AirAccelerate
    {
		public override float GetVelDiff(float length, Vector3 velocity, Vector3 wish_dir)
		{
			return length - velocity.Dot(wish_dir);
		}
    }
}
