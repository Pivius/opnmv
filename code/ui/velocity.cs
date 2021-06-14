using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Velocity : Panel
{
	public Label Label;

	public Velocity()
	{
		Label = Add.Label( "100", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Label.Text = $"{player.Velocity.WithZ(0).Length:n0}";
	}
}
