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

		public override void FrameSimulate()
		{
			Duck.TryDuck();
			//BetterLog.Info(Duck.IsDucked);
			EyeRot = Input.Rotation;
			EyePosLocal = Vector3.Up * GetViewOffset() * Pawn.Scale;
			WishVelocity = WishVel(Properties.MaxMove);
			Walljump.Move(this, WishVelocity);
		}

		public override void Simulate()
		{
			if (StartMove()) 
				return;
			//Sandbox.BetterLog.Info(Properties.StandMaxs);
			Walljump.Move(this, WishVelocity);

			if (SetupMove()) 
				return;
			
			EndMove();
		}

	}
}
