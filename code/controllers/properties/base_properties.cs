using Sandbox;
using System;

namespace OMMovement
{
	public class BaseProperties
	{
		public float DefaulSpeed{get; set;} = 190.0f;
		public float RunSpeed{get; set;} = 320.0f;
		public float WalkSpeed{get; set;} = 150.0f;
		public float SwimSpeed{get; set;} = 250.0f;
		public float Gravity{get; set;} = 800.0f;
		public float AirAccelerate{get; set;} = 100.0f;
		public float Accelerate{get; set;} = 10.0f;
		public float WaterAccelerate{get; set;} = 10.0f;
		public float Friction{get; set;} = 8.0f;
		public float WaterFriction{get; set;} = 1.0f;
		public float StopSpeed{get; set;} = 100.0f;
		public float JumpPower{get; set;} = 220.0f;
		public float DuckSpeed{get; set;} = 0.3f;
		public float UnDuckSpeed{get; set;} = 0.3f;
		public float ViewOffset{get; set;} = 64.0f;
		public float StandViewOffset{get; set;} = 64.0f;
		public float DuckViewOffset{get; set;} = 28.0f;
		public float DeadViewOffset{get; set;} = 14.0f;
		public float StepSize{get; set;} = 16.0f;
		public float FallDamageMultiplier{get; set;} = 0.0563f;
		public float MaxSpeed{get; set;} = 320.0f;
		public float StandableAngle{get; set;} = 45;
		public float MaxMove{get; set;} = 10000.0f;
		public float JumpTime{get; set;} = 0.0f;
		public STATE MoveState{get; set;} = 0;
		public bool AutoJump{get; set;} = true;
		public bool IsDucking{get; set;} = false;
		public bool CanWalk{get; set;} = true;
		public bool CanRun{get; set;} = true;
		public bool CanAirStrafe{get; set;} = true;
		public bool CanAccelerate{get; set;} = true;
		public bool CanUnDuckJump{get; set;} = true;
		public bool AllowAutoMovement{get; set;} = true;
		public Vector3 OBBMins{get; set;} = new Vector3(-16, -16, 0);
		public Vector3 OBBMaxs{get; set;} = new Vector3(16, 16, 72);
		public Vector3 StandMins{get; set;} = new Vector3(-16, -16, 0);
		public Vector3 StandMaxs{get; set;} = new Vector3(16, 16, 72);
		public Vector3 DuckMins{get; set;} = new Vector3(-16, -16, 0);
		public Vector3 DuckMaxs{get; set;} = new Vector3(16, 16, 32);
		public Vector3 OldVelocity {get; set;}

		public float DIST_EPSILON{get; private set;} = 0.03125f;

		public BaseProperties()
		{
		}
	}
}
