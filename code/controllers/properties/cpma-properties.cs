using Sandbox;
using System;

namespace OMMovement
{
	public class CPMAProperties : QuakeProperties
	{
		// # Movement Properties

		public override float MaxSpeed{get; set;} = 300.0f;
		public override float DefaultSpeed{get; set;} = 190.0f;
		public override float Friction{get; set;} = 8.0f;


		// # Accelerate Properties
		public override float Accelerate{get; set;} = 15.0f;
		public override float AirAccelerate{get; set;} = 1.0f;
		public override float SideStrafeMaxSpeed{get; set;} = 30.0f;
		public override float StrafeAcceleration{get; set;} = 70.0f;
		public override float AirStopAcceleration{get; set;} = 2.5f;
		public override float AirControl{get; set;} = 150.0f;
	}
}
