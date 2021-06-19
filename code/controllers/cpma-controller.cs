using Sandbox;
using System;

namespace OMMovement
{
	public class CPMAController : MovementController
	{
		public CPMAController()
		{
			Properties = new CPMAProperties();
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
