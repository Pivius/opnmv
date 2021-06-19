using Sandbox;
using System;
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
