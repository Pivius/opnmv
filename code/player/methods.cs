using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMCore
{
	partial class MovementPlayer : Player
	{
		public MovementPlayer()
		{
			Log.Info("Movement Player");
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
		}
	}
}
