using Sandbox;
using System;

namespace OMMovement
{
	public class ParkourProperties : BaseProperties
	{
		// # Movement Properties

		public override float MaxSpeed{get; set;} = 300.0f;
		public override bool CanWalk{get; set;} = false;
		public override bool CanRun{get; set;} = false;
		public override float DefaultSpeed{get; set;} = 400.0f;
		public override float DuckedWalkSpeed{get; set;} = 200.0f;
		public override float SwimSpeed{get; set;} = 400.0f;
		public override float Gravity{get; set;} = 600.0f;
		public override float StandableAngle{get; set;} = 45;


		// # Accelerate Properties

		public override float AirAccelerate{get; set;} = 10.0f;


		// # Friction Properties
		public override float WaterFriction{get; set;} = 1.0f;
		public override float StopSpeed{get; set;} = 100.0f;


		// # Duck

		public override float DuckSpeed{get; set;} = 0.2f;
		public override float UnDuckSpeed{get; set;} = 0.2f;
	}
}
