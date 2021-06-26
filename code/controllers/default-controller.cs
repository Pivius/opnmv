using Sandbox;
using System;
using Core;
namespace OMMovement
{
	public class DefaultController : MovementController
	{
		public DefaultController()
		{
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
