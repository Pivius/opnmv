using Sandbox;
using System;
using System.Collections;
using Core;

namespace OMMovement
{
	public class ParkourController : MovementController
	{
		public Walljump Walljump;

		public ParkourController()
		{
			Properties = new ParkourProperties();
			Walljump = new Walljump();
		}

		public override void Simulate()
		{
			if (StartMove()) 
				return;

			Walljump.Move(this, WishVelocity);

			if (SetupMove()) 
				return;
			
			EndMove();
		}

	}
}
