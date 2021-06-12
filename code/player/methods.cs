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
		public MovementPlayer()
		{
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			Controller = new MovementController();
			Animator = new PlayerAnimator();
			Camera = new FirstPersonCamera();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
			BetterLog.Info("test");
		}

		public override void Simulate(Client client)
		{
			MouseInput.MouseMove();
			EyeRot = ViewAngle;
			base.Simulate(client);
		}

		public override void FrameSimulate(Client client)
		{
			MouseInput.MouseMove();
			base.FrameSimulate(client);
		}

		public override void OnKilled()
		{
			base.OnKilled();
			EnableDrawing = false;
		}
	}
}
