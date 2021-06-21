using Sandbox;
using OMMovement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public partial class MovementPlayer : Player
	{
		TimeSince timeSinceDied;
		public MovementPlayer()
		{
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			Controller = new DefaultController();
			Animator = new PlayerAnimator();
			Camera = new PlayerCamera();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
		}

		public override void Simulate(Client client)
		{
			ProcessMoveButtons();
			if (base.LifeState == LifeState.Dead)
			{
				if (timeSinceDied > 3 && IsServer)
					Respawn();

				return;
			}
			
			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
		}

		public override void FrameSimulate(Client client)
		{
			ProcessMoveButtons();
			var controller = GetActiveController();
			controller?.FrameSimulate(client, this, GetActiveAnimator());
		}

		public override void OnKilled()
		{
			base.OnKilled();
			EnableDrawing = false;
		}
	}
}
