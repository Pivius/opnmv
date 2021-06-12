using Sandbox;
namespace Sandbox
{
	public static class BetterLog
	{
		public static void Error( System.Exception e, params object[] inputs )
		{
			Log.Error( e, GetText( inputs ) );
		}

		public static void Error( System.Exception e )
		{
			Log.Error( e );
		}

		public static void Error( params object[] inputs )
		{
			Log.Error( GetText( inputs ) );
		}

		public static void Info( params object[] inputs )
		{
			Log.Info( GetText( inputs ) );
		}

		public static void Trace( params object[] inputs )
        	{
			Log.Trace( GetText( inputs ) );
        	}

		public static void Warning( params object[] inputs )
		{
			Log.Warning( GetText( inputs ) );
		}

		public static void Warning( System.Exception e, params object[] inputs )
		{
			Log.Warning( e, GetText( inputs ) );
		}

		private static string GetText( params object[] inputs )
		{
			string output = "";

			for ( int i = 0; i < inputs.Length; i++ )
				output += $"{inputs[i]}\t";

			return $"{(Host.IsServer ? "SV" : "CL")}: {output.TrimEnd( '\t' )}";
		}
	}
}