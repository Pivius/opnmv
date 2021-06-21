using Sandbox;
using System;

namespace OMMovement
{
	public class BaseProperties
	{
		// # Movement Properties

		public virtual float MaxSpeed{get; set;} = 320.0f;
		public virtual float MaxMove{get; set;} = 10000.0f;
		public virtual bool CanWalk{get; set;} = true;
		public virtual bool CanRun{get; set;} = true;
		public virtual float DefaultSpeed{get; set;} = 190.0f;
		public virtual float RunSpeed{get; set;} = 320.0f;
		public virtual float WalkSpeed{get; set;} = 150.0f;
		public virtual float DuckedWalkSpeed{get; set;} = 150.0f;
		public virtual float SwimSpeed{get; set;} = 250.0f;
		public virtual float Gravity{get; set;} = 800.0f;
		public virtual float JumpPower{get; set;} = 268.3281572999747f * 1.2f;
		public virtual float StepSize{get; set;} = 16.0f;
		public virtual float StandableAngle{get; set;} = 1;
		public virtual float FallDamageMultiplier{get; set;} = 0.0563f;
		public virtual bool AutoJump{get; set;} = true;
		public virtual bool AllowAutoMovement{get; set;} = true;
		public STATE MoveState{get; set;} = 0;


		// # Accelerate Properties

		public virtual bool CanAirStrafe{get; set;} = true;
		public virtual float AirAccelerate{get; set;} = 100.0f;
		public virtual bool CanAccelerate{get; set;} = true;
		public virtual float Accelerate{get; set;} = 10.0f;
		public virtual float WaterAccelerate{get; set;} = 10.0f;


		// # Friction Properties
		public virtual float Friction{get; set;} = 8.0f;
		public virtual float WaterFriction{get; set;} = 1.0f;
		public virtual float StopSpeed{get; set;} = 100.0f;


		// # Duck

		public virtual float DuckSpeed{get; set;} = 0.3f;
		public virtual float UnDuckSpeed{get; set;} = 0.3f;
		public virtual bool IsDucking{get; set;} = false;
		public virtual bool CanUnDuckJump{get; set;} = true;


		// # View Offset

		public virtual float ViewOffset{get; set;} = 64.0f;
		public virtual float StandViewOffset{get; set;} = 64.0f;
		public virtual float DuckViewOffset{get; set;} = 28.0f;
		public virtual float DeadViewOffset{get; set;} = 14.0f;


		// # Player Hulls

		public virtual Vector3 OBBMins{get; set;} = new Vector3(-16, -16, 0);
		public virtual Vector3 OBBMaxs{get; set;} = new Vector3(16, 16, 72);
		public virtual Vector3 StandMins{get; set;} = new Vector3(-16, -16, 0);
		public virtual Vector3 StandMaxs{get; set;} = new Vector3(16, 16, 72);
		public virtual Vector3 DuckMins{get; set;} = new Vector3(-16, -16, 0);
		public virtual Vector3 DuckMaxs{get; set;} = new Vector3(16, 16, 32);
		public virtual Vector3 OldVelocity {get; set;}


		// # Quake Properties

		public virtual float SideStrafeMaxSpeed{get; set;} = 400.0f;
		public virtual float StrafeAcceleration{get; set;} = 1.0f;
		public virtual float AirStopAcceleration{get; set;} = 1.0f;
		public virtual float AirControl{get; set;} = 0.0f;
	}
}
