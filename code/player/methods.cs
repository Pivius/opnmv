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
			Camera = new PlayerCamera();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
			BetterLog.Info("test");
		}

		public override void Simulate(Client client)
		{
			base.Simulate(client);
		}

		public override void BuildInput( InputBuilder input )
		{	
			var prev_delta = PreviousDelta;
			var view_angle = Rotation.From(input.ViewAngles);
			base.BuildInput( input );
			
			MouseInput.MouseMove(ref view_angle, new Vector2(Input.MouseDelta.x, Input.MouseDelta.y), ref prev_delta);
			input.ViewAngles = view_angle.Angles();
			PreviousDelta = prev_delta;
		}
		
		public override void FrameSimulate(Client client)
		{
			base.FrameSimulate(client);
		}

		public override void OnKilled()
		{
			base.OnKilled();
			EnableDrawing = false;
		}
	}
}
