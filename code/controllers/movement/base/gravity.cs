using System.Threading;
using System.Numerics;
using Sandbox;

namespace OMMovement
{
    public class Gravity 
    {
		// Source Gravity
		
		public Vector3 AddGravity(float gravity, Vector3 velocity)
		{
			return velocity - new Vector3(0, 0, gravity * Time.Delta);
		}
    }
}
