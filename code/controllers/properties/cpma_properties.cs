using Sandbox;
using System;

namespace OMMovement
{
	public class CPMAProperties : QuakeProperties
	{
		public float DefaulSpeed{get; set;} = 190.0f;
		public float Friction{get; set;} = 8.0f;
		public float Accelerate{get; set;} = 15.0f;
		public float AirAccelerate{get; set;} = 1.0f;
		public float MaxSpeed{get; set;} = 300.0f;
		public float SideStrafeMaxSpeed{get; set;} = 30.0f;
		public float StrafeAcceleration{get; set;} = 70.0f;
		public float AirStopAcceleration{get; set;} = 2.5f;
		public float AirControl{get; set;} = 150.0f;
	}
}
