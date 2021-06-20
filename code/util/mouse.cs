using System.Runtime.Intrinsics;
using System.Numerics;
using System;
using Sandbox;

namespace Core
{
	public static class MouseInput
	{
		public static float GetConCommand(string command)
		{
			return Single.Parse(ConsoleSystem.GetValue(command));
		}

		private static void GetMouseDelta(ref Vector2 delta_in, ref Vector2 prev_delta)
		{
			var mouse_filter = GetConCommand("m_filter") == 1 ? true : false;
			
			// Apply filtering?
			if (mouse_filter)
			{
				// Average over last two samples
				delta_in += (prev_delta) * 0.5f;
			}

			// Latch previous
			prev_delta = delta_in;
		}

		private static void ScaleMouse(ref Vector2 delta)
		{
			float dx = delta.x;
			float dy = delta.y;
			float mouse_sensitivity = GetConCommand("sensitivity");
			float custom_accel = GetConCommand("m_customaccel");

			if (custom_accel == 1.0f || custom_accel == 2.0f) 
			{ 
				float raw_mouse_movement_distance = MathF.Sqrt(dx * dx + dy * dy);
				float acceleration_scale = GetConCommand("m_customaccel_scale");
				float accelerated_sensitivity_max = GetConCommand("m_customaccel_max");
				float accelerated_sensitivity_exponent = GetConCommand("m_customaccel_exponent");
				float accelerated_sensitivity = (MathF.Pow(raw_mouse_movement_distance, accelerated_sensitivity_exponent) * acceleration_scale + mouse_sensitivity);

				if (accelerated_sensitivity_max > 0.0001f && accelerated_sensitivity > accelerated_sensitivity_max)
				{
					accelerated_sensitivity = accelerated_sensitivity_max;
				}

				delta *= accelerated_sensitivity; 

				// Further re-scale by yaw and pitch magnitude if user requests alternate mode 2/4
				// This means that they will need to up their value for m_customaccel_scale greatly (>40x) since m_pitch/yaw default
				// to 0.022
				if (custom_accel == 2.0f || custom_accel == 4.0f)
				{ 
					delta.x *= GetConCommand("m_yaw"); 
					delta.y *= GetConCommand("m_pitch"); 
				} 
			}
			else if (custom_accel == 3.0f)
			{
				float raw_mouse_movement_distance_squared = dx * dx + dy * dy;
				float fExp = MathF.Max(0.0f, (GetConCommand("m_customaccel_exponent") - 1.0f) / 2.0f);
				float accelerated_sensitivity = MathF.Pow( raw_mouse_movement_distance_squared, fExp ) * mouse_sensitivity;

				delta *= accelerated_sensitivity;
			}
			else
			{ 
				delta *= mouse_sensitivity;
			}

			if (Single.IsNaN(delta.x) || Single.IsInfinity(delta.x))
				delta.x = 0.0f;

			if (Single.IsNaN(delta.x) || Single.IsInfinity(delta.x))
				delta.y = 0.0f;
		}

		private static Rotation AdjustView(Rotation view_rot, Vector2 delta)
		{
			Angles view_ang = view_rot.Angles() - new Angles(GetConCommand("m_pitch") * -delta.y, GetConCommand("m_yaw") * delta.x, 0);
			view_ang = view_ang.WithPitch(MathX.Clamp(view_ang.pitch, -85, 85));
			return Rotation.LookAt(view_ang.Direction, Vector3.Up);
		}

		public static void MouseMove(ref Rotation view, Vector2 delta, ref Vector2 prev_delta)
		{
			GetMouseDelta(ref delta, ref prev_delta);
			ScaleMouse(ref delta);
			view = AdjustView(view, delta);
		}
	}
}
