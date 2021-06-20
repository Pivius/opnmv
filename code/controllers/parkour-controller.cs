using Sandbox;
using System;
using Core;

namespace OMMovement
{
	public class ParkourController : MovementController
	{
		public Walljump Walljump;

		public ParkourController()
		{
			Properties = new ParkourProperties();
			Walljump = new Walljump(this);
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
