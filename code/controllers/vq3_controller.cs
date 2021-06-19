using Sandbox;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OMMovement
{
	public class QuakeController : MovementController
	{
		public QuakeController()
		{
			Properties = new QuakeProperties();
			AirAccelerate = new QuakeAirAccelerate();
		}

		public override void Simulate()
		{
			
			if (StartMove()) 
				return;

			if (SetupMove()) 
				return;
			
			EndMove();
		}

	}
}
