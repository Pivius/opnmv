using Sandbox;
using Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OMMovement
{
	public class Duck : NetworkComponent
	{
		protected MovementController Controller;
		public MovementPlayer Player;
		public float DuckTime{get; set;} = 0.0f;
		public bool IsDucking{get; set;} = false;
		public Duck(MovementController controller)
		{
			Controller = controller;
		}

		public void StartDuck()
		{
			bool should_duck = Input.Down(InputButton.Duck);
			DebugOverlay.ScreenText( 0, $"Input: {InputButton.Duck}" );
		}

		public virtual void PreTick() 
		{
			StartDuck();
		}
	}
}
