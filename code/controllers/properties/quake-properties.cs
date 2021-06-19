using Sandbox;
using System;

namespace OMMovement
{
	public class QuakeProperties : BaseProperties
	{
		// # Movement Properties

		public override float MaxSpeed{get; set;} = 300.0f;
		public override bool CanWalk{get; set;} = false;
		public override bool CanRun{get; set;} = false;
		public override float DefaultSpeed{get; set;} = 250.0f;
		public override float Friction{get; set;} = 6.0f;
		public override float Gravity{get; set;} = 800.0f;
		public override float JumpPower{get; set;} = 270.0f;


		// # Accelerate Properties

		public override float Accelerate{get; set;} = 10.0f;
		public override float AirAccelerate{get; set;} = 1.0f;
		public override float WaterAccelerate{get; set;} = 4.0f;
		public override float SideStrafeMaxSpeed{get; set;} = 400.0f;
		public override float StrafeAcceleration{get; set;} = 1.0f;
		public override float AirStopAcceleration{get; set;} = 1.0f;
		public override float AirControl{get; set;} = 0.0f;
	}
}
