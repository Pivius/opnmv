using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public class PlayerCamera : FirstPersonCamera
	{
		[ConVar.ClientData("fov_desired")] public static float FOV { get; set; } = 90f;
		Vector3 lastPos;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;

			lastPos = Pos;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePos;
			if ( eyePos.Distance( lastPos ) < 300 ) // TODO: Tweak this, or add a way to invalidate lastpos when teleporting
			{
				Pos = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
			}
			else
			{
				Pos = eyePos;
			}
			//MouseInput.MouseMove();

			Rot = pawn.EyeRot;

			FieldOfView = Single.Parse(ConsoleSystem.GetValue("fov_desired"));

			Viewer = pawn;
			lastPos = Pos;
		}
	}
}