using Sandbox;
using System;
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
