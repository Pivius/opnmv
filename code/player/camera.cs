using Sandbox;
using System;
using OMMovement;

namespace Core
{
	public partial class PlayerCamera : FirstPersonCamera
	{
		Vector3 lastPos;
		protected Vector3 PreviousDelta;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;

			lastPos = Pos;
		}

		private void ScaleSensitivity(ref Angles view_angles, Vector2 previous_delta, Vector2 mouse_delta)
		{
			MouseInput.MouseMove(ref view_angles, ref previous_delta, mouse_delta);
			PreviousDelta = previous_delta;
		}

		public override void BuildInput(InputBuilder input)
		{
			ScaleSensitivity(ref input.ViewAngles, PreviousDelta, new Vector2(input.AnalogLook.yaw*-100, input.AnalogLook.pitch*100));
			input.InputDirection = input.AnalogMove;
		}

		protected virtual void DrawPlayerTrails(MovementPlayer player)
		{
			if (player.Trail == null)
			{
				player.Trail = new Trail();
				player.Trail.Parent = player;
			}

			if (player.LifeState == LifeState.Dead && player.Trail.TrailColor.a == 255)
			{
				for (int i = 0; i < (player.Trail.Nodes.Count); i++)
				{
					player.Trail.SafeRemoveNode(i);
				}

				player.Trail.DrawTempSegment = false;
				player.Trail.TrailColor.a = 0;
			}
			else if (player.LifeState == LifeState.Alive)
			{
				player.Trail.DrawTempSegment = true;
				player.Trail.TrailColor.a = 255;
				player.Trail.NewNode();
				player.Trail.UpdateNode();
			}
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

			Rot = pawn.EyeRot;
			FieldOfView = Single.Parse(ConsoleSystem.GetValue("fov_desired"));
			Viewer = pawn;
			lastPos = Pos;
			ZNear = 1;

			foreach (var cl in Client.All)
			{
				var player = ((MovementPlayer) cl.Pawn);

				DrawPlayerTrails(player);
			}
		}
	}
}
