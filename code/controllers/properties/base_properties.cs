using Sandbox;
using System;

namespace OMMovement
{
	public class BaseProperties
	{
		public float DefaulSpeed{get; set;} = 190.0f;
		public float RunSpeed{get; set;} = 320.0f;
		public float WalkSpeed{get; set;} = 150.0f;
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
		public float StepSize{get; set;} = 16.0f;
		public float FallDamageMultiplier{get; set;} = 0.0563f;
		public float MaxSpeed{get; set;} = 320.0f;
		public int MaxClipPlanes{get; set;} = 5;
		public int MoveType{get; set;} = 0;
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
		public Vector3 DuckMaxs{get; set;} = new Vector3(16, 16, 32);
		public Vector3 OldVelocity {get; set;}

		public BaseProperties()
		{
		}
	}
}
