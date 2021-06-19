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

	public partial class MovementPlayer
	{
		[ConVar.ClientData] public static int opnmv_mode { get; set; } = 0;
		public int MoveMode{ get; set; } = 0;
		public List<PawnController> MoveControllers = new List<PawnController>
		{
			new DefaultController(),
			new QuakeController(),
			new CPMAController()
		};

		public override PawnController GetActiveController()
		{
			if ( DevController != null ) return DevController;

			SetMoveMode();

			return Controller;
		}

		public void SetMoveMode()
		{
			var mode = Int32.Parse(ConsoleSystem.GetValue("opnmv_mode"));
			
			if (mode != MoveMode)
			{
				var controller_count = MoveControllers.Count - 1;
				var clamped_mode = (int)MathX.Clamp(mode, 0, controller_count);

				ConsoleSystem.Run("opnmv_mode", clamped_mode);
				MoveMode = clamped_mode;
				Controller = MoveControllers[MoveMode];
			}
		}
	}
}
