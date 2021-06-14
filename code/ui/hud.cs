using Sandbox.UI;

namespace OMHUD
{
	public partial class HudEntity : Sandbox.HudEntity<RootPanel>
	{
		public HudEntity()
		{
			if ( IsClient )
			{
				RootPanel.StyleSheet.Load( "/ui/hud.scss" );
				RootPanel.AddChild<Velocity>();
			}
		}
	}

}
