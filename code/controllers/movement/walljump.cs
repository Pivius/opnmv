using Sandbox;
using System;

namespace OMMovement
{
	public class Walljump
	{
		// EMM Walljump
		public float WalljumpDistance = 30;
		public float WalljumpDelay = 0.2f;
		public float WalljumpSideVelocity = 260.0f;
		public float WalljumpUpVelocity = 200.0f;
		public float WalljumpAngle = 58.0f;
		public float WalljumpTime{get; set;} = 0;
		public bool CanWalljump = true;
		public bool CanWalljumpSky = false;
		//public ulong WALLJUMP_BUTTONS {get; private set;} = ulong.Parse((InputButton.Jump | InputButton.Forward | InputButton.Back | InputButton.Right | InputButton.Left).ToString());
		protected MovementController Controller;
		public Walljump(MovementController controller)
		{
			Controller = controller;
		}


	}
}
