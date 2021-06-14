using Sandbox;
using System;

namespace OMMovement
{
	public class QuakeProperties : BaseProperties
	{
		public float DefaulSpeed{get; set;} = 250.0f;
		public float Friction{get; set;} = 6.0f;
		public float Gravity{get; set;} = 800.0f;
		public float JumpPower{get; set;} = 270.0f;
		public float Accelerate{get; set;} = 10.0f;
		public float AirAccelerate{get; set;} = 1.0f;
		public float WaterAccelerate{get; set;} = 4.0f;
		public float MaxSpeed{get; set;} = 300.0f;
		public float SideStrafeMaxSpeed{get; set;} = 400.0f;
		public float StrafeAcceleration{get; set;} = 1.0f;
		public float AirStopAcceleration{get; set;} = 1.0f;
		public float AirControl{get; set;} = 0.0f;
		public bool CanWalk{get; set;} = false;
		public bool CanRun{get; set;} = false;
	}
}
