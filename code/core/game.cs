using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMCore
{
	[Library("opnmv")]
	public partial class MovementGame : Sandbox.Game
	{
		public MovementGame()
		{
			if ( IsServer )
			{
				Log.Info( "Game serverside Pass" );
				hudController = new BaseHUD();
			}

			if ( IsClient )
			{
				Log.Info( "Game clientside Pass" );
			}
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined(client);

			var ply = new MovementPlayer();

			client.Pawn = ply;
			ply.Respawn();
		}
	}
}