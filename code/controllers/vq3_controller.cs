using Sandbox;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OMMovement
{
	public partial class QuakeController : MovementController
	{
		public QuakeProperties Properties;
		public QuakeAirAccelerate AirAccelerate;
		
		public QuakeController()
		{
			
			Properties = new QuakeProperties();
			Duck = new Sandbox.Duck(this);
			AirAccelerate = new QuakeAirAccelerate();
			Accelerate = new Accelerate();
			Gravity = new Gravity();
			Friction = new Friction();
			Unstuck = new Sandbox.Unstuck(this);
		}
	}
}
