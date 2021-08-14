using System;
using System.Collections.Generic;
using Sandbox;
using OMMovement;

namespace Core
{
	public enum MODE : int
	{
		SOURCE = 0,
		VQ3,
		CPMA,
		GRAPPLE,
		PARKOUR
	}

	public partial class MovementPlayer : Player
	{
		[ConVar.ClientData] public static int opnmv_mode {get; set;} = 0;
		[Net] public int MoveMode {get; private set;} = 0;

		public static List<PawnController> MoveControllers{get; private set;} = new List<PawnController>
		{
			new DefaultController(),
			new QuakeController(),
			new CPMAController(),
			new ParkourController()
		};

		public virtual PawnController GetActiveController(Client client)
		{
			if ( DevController != null ) return DevController;

			SetMoveMode(client);
			return Controller;
		}

		public void SetMoveMode(Client client)
		{
			if (IsServer)
			{
				int mode = 0;

				Int32.TryParse(client.GetUserString("opnmv_mode"), out mode);
		
				if (mode != MoveMode)
				{
					var controller_count = MoveControllers.Count - 1;

					mode = (int)MathX.Clamp(mode, 0, controller_count);
					client.SendCommandToClient("opnmv_mode " + mode);
					MoveMode = mode;
					Controller = MoveControllers[MoveMode];
					ClientMode(mode);
				}
			}
		}

		[ClientRpc] private void ClientMode(int mode)
		{
			MoveMode = mode;
			Controller = MoveControllers[MoveMode];
		}
	}
}
