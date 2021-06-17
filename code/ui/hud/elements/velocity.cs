using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace OMHUD
{
	public partial class Velocity : Elements
	{
		public Velocity()
		{
			SetStyleSheet("/ui/hud/elements/velocity.scss");
			Label = Add.Label( "100", "label" );
		}

		public Label Label;

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;
			Label.Text = $"{player.Velocity.WithZ(0).Length:n0}";
			//Sandbox.BetterLog.Info(Label.DataBind);
		}
	}
}
