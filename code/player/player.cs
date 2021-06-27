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
		protected ulong SpawnButtons = ((ulong) InputButton.Forward | (ulong) InputButton.Right | (ulong) InputButton.Left | (ulong) InputButton.Back | (ulong) InputButton.Jump);
		public TimeSince timeSinceDied;

		public MovementPlayer()
		{
		}

		public override void Respawn()
		{
			SetModel("models/citizen/citizen.vmdl");
			Controller = new DefaultController();
			Animator = new PlayerAnimator();
			Camera = new PlayerCamera();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Event.Run("PlayerSpawn", this);
			base.Respawn();
			ClientRespawn(this);
		}

		[ClientRpc] public void ClientRespawn(MovementPlayer player) 
		{
			Event.Run("PlayerSpawn", player);
		}

		public override void Simulate(Client client)
		{
			ProcessMoveButtons();

			if (LifeState == LifeState.Dead)
			{
				using(Prediction.Off())
				{
					if (KeyPressed(SpawnButtons) && IsServer)
						Respawn();
					return;
				}
			}

			var controller = GetActiveController(client);
			controller?.Simulate( client, this, GetActiveAnimator() );

			if (IsServer)
				Event.Run("Simulate.Server", client);
		}

		public override void FrameSimulate(Client client)
		{
			foreach (var cl in Client.All)
			{
				Event.Run("Simulate.Client.All", cl);
			}
			
			if (LifeState == LifeState.Dead)
			{
				
				if (KeyPressed(SpawnButtons))
					Respawn();

				return;
			}

			ProcessMoveButtons();
			var controller = GetActiveController(client);
			controller?.FrameSimulate(client, this, GetActiveAnimator());
			Event.Run("Simulate.Client", client);
		}

		public override void OnKilled()
		{
			base.OnKilled();
			EnableDrawing = false;
			Event.Run("OnKilled", this);
			ClientOnKilled(this);
		}

		[ClientRpc] public void ClientOnKilled(MovementPlayer player)
		{
			Event.Run("OnKilled", player);
		}
	}
}
