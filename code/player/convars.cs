using System;
using Sandbox;

namespace Core
{
	public partial class MovementPlayer
	{
		[ConVar.ClientData( "m_customaccel" )] public static float CustomAccel { get; set; } = 0.0f;
		[ConVar.ClientData( "m_customaccel_scale" )] public static float CustomAccelScale { get; set; } = 0.04f;
		[ConVar.ClientData( "m_customaccel_max" )] public static float CustomAccelMax { get; set; } = 0.0f;
		[ConVar.ClientData( "m_customaccel_exponent" )] public static float CustomAccelExponent { get; set; } = 1.0f;
		[ConVar.ClientData( "m_rawinput" )] public static float RawInput { get; set; } = 1;
		[ConVar.ClientData("m_mouseenable")] public static float MouseEnable { get; set; } = 1;
		[ConVar.ClientData("m_filter")] public static float MouseFilter { get; set; } = 1;
		[ConVar.ClientData("sensitivity")] public static float Sensitivity { get; set; } = 1.0f;
		[ConVar.ClientData("m_yaw")] public static float Yaw { get; set; } = 0.022f;
		[ConVar.ClientData("m_pitch")] public static float Pitch { get; set; } = 0.022f;
	}
}
