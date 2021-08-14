using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Sandbox;
using Core;

namespace OMMovement
{
	public abstract partial class MovementController : WalkController
	{
		public BaseProperties Properties;
		public Vector3 LadderNormal;
		public bool IsTouchingLadder = false;
		public float ViewOffset{get; set;} = 64.0f;
		public Vector3 OBBMins{get; set;} = new Vector3(-16, -16, 0);
		public Vector3 OBBMaxs{get; set;} = new Vector3(16, 16, 72);
		public Vector3 OldVelocity{get; set;}

		public MovementController()
		{
			Properties = new BaseProperties();
			AirAccelerate = new AirAccelerate();
			Duck = new Duck(this);
			Accelerate = new Accelerate();
			Gravity = new Gravity();
			Friction = new Friction();
			Water = new Water();
			Unstuck = new Sandbox.Unstuck(this);
		}

		public virtual MovementPlayer GetPlayer()
		{
			return (MovementPlayer)Pawn;
		}
		public override TraceResult TraceBBox(Vector3 start, Vector3 end, float liftFeet = 0.0f)
		{
			return TraceBBox(start, end, OBBMins, OBBMaxs, liftFeet);
		}

		public override BBox GetHull()
		{
			return new BBox(OBBMins, OBBMaxs);
		}

		public virtual Vector3 GetPlayerMins(bool is_ducked)
		{
			return is_ducked ? (Properties.DuckMins * Pawn.Scale) : (Properties.StandMins * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMaxs(bool is_ducked)
		{
			return is_ducked ? (Properties.DuckMaxs * Pawn.Scale) : (Properties.StandMaxs * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMins()
		{
			return Duck.IsDucked ? (Properties.DuckMins * Pawn.Scale) : (Properties.StandMins * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMaxs()
		{
			return Duck.IsDucked ? (Properties.DuckMaxs * Pawn.Scale) : (Properties.StandMaxs * Pawn.Scale);
		}

		public virtual float GetPlayerViewOffset(bool is_ducked)
		{
			return is_ducked ? (Properties.DuckViewOffset * Pawn.Scale) : (Properties.StandViewOffset * Pawn.Scale);
		}

		public virtual float GetViewOffset()
		{
			return (ViewOffset * Pawn.Scale);
		}

		public override void SetBBox(Vector3 mins, Vector3 maxs)
		{
			OBBMins = mins;
			OBBMaxs = maxs;
		}

		public override void UpdateBBox()
		{
			var mins = GetPlayerMins();
			var maxs = GetPlayerMaxs();

			if (OBBMins != mins || OBBMaxs != maxs)
			{
				SetBBox(mins, maxs);
			}
		}

		public bool OnGround()
		{
			return GroundEntity != null;
		}

		public float FallDamage()
		{
			return MathF.Max(Velocity.z - 580.0f, 0) * Properties.FallDamageMultiplier;
		}

		public override void FrameSimulate()
		{
			Duck.TryDuck();
			//BetterLog.Info(Duck.IsDucked);
			EyeRot = Input.Rotation;
			EyePosLocal = Vector3.Up * GetViewOffset() * Pawn.Scale;
			WishVelocity = WishVel(Properties.MaxMove);
		}

		public override void Simulate()
		{
			if (StartMove()) 
				return;

			if (SetupMove()) 
				return;
			
			EndMove();
		}

		void RestoreGroundPos()
		{
			if (GroundEntity == null || GroundEntity.IsWorld)
				return;
		}

		void SaveGroundPos()
		{
			if (GroundEntity == null || GroundEntity.IsWorld)
				return;
		}
	}
}
