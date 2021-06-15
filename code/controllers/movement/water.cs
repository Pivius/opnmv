using System;
using Sandbox;
using Core;

namespace OMMovement
{
    public class Water : Accelerate
    {
		public float JumpTime {get; set;}
		public Vector3 JumpVel {get; set;}
		public float EntryTime {get; set;}
		public float JumpHeight {get; private set;} = 8.0f;
		public float SinkSpeed {get; set;} = 60.0f;
		public WATERLEVEL WaterLevel {get; set;} = 0;
		public WATERLEVEL OldWaterLevel {get; set;} = 0;

		public virtual Vector3 CheckWaterJump(Vector3 velocity, Vector3 position, BaseProperties props, Entity pawn)
		{
			// Already water jumping.
			if (JumpTime == 0)
			{
			// Don't hop out if we just jumped in
				if (velocity.z >= -180.0f) // only hop out if we are moving up
				{
					Vector3 forward = Input.Rotation.Forward;
					Vector3	flat_forward = forward.WithZ(0).Normal;
					Vector3	flat_velocity = velocity.WithZ(0);

					// Are we backing into water from steps or something?  If so, don't pop forward
					if (flat_velocity.Length != 0.0f && (flat_velocity.Dot(flat_forward) >= 0.0f))
					{
						// Start line trace at waist height (using the center of the player for this here)
						var trace_start = position + (props.OBBMins + props.OBBMaxs) * 0.5f;
						var trace_end = trace_start + flat_forward * 24.0f; //VectorMA( vecStart, 24.0f, flatforward, vecEnd );
						var trace = TraceUtil.PlayerBBox(trace_start, trace_end, props.OBBMins, props.OBBMaxs, pawn);
						
						// solid at waist
						if (trace.Fraction < 1.0f)
						{
							
							trace_start = trace_start.WithZ(position.z + props.ViewOffset + JumpHeight); 
							trace_end = trace_start + flat_forward * 24.0f;
							JumpVel = trace.Normal * -50.0f;
							trace = TraceUtil.PlayerBBox(trace_start, trace_end, props.OBBMins, props.OBBMaxs, pawn);

							// open at eye level
							if (trace.Fraction == 1.0f)
							{
								// Now trace down to see if we would actually land on a standable surface.
								trace_start = trace_end;
								trace_end = trace_end.WithZ(trace_end.z - 1024.0f);
								trace = TraceUtil.PlayerBBox(trace_start, trace_end, props.OBBMins, props.OBBMaxs, pawn);

								if (trace.Fraction < 1.0f && trace.Normal.z >= 0.7f)
								{
									velocity = velocity.WithZ(300.0f); // Push Up
									JumpTime = 200.0f; // Do this for .2 seconds
								}
							}
						}
					}
				}
			}
			return velocity;
		}

		public virtual Vector3 WaterJump(Vector3 velocity)
		{
			if (JumpTime > 10000.0f)
				JumpTime = 10000.0f;

			if (JumpTime != 0)
			{
				JumpTime -= 1000.0f * Time.Delta;

				if (JumpTime <= 0 || WaterLevel == 0)
				{
					JumpTime = 0;
				}
				velocity = new Vector3(JumpVel.x, JumpVel.y, velocity.z);
			}

			return velocity;
		}

		public virtual bool InWater()
		{
			return (WaterLevel > WATERLEVEL.Feet);
		}

		public virtual bool CheckWater(Vector3 position, Vector3 mins, Vector3 maxs, float view_pos, Entity pawn)
		{
			var point = TraceUtil.CreateBBox(position, position, mins, maxs, pawn, 1)
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Water, true )
				.Run();

			// Assume that we are not in water at all.
			WaterLevel = WATERLEVEL.NotInWater;
			
			// Are we under water? (not solid and not empty?)
			if (point.Fraction == 0.0f)
			{
				// We are at least at level one
				WaterLevel = WATERLEVEL.Feet;

				point = TraceUtil.CreateBBox(position, position, mins, maxs, pawn, maxs.z * 0.5f)
					.HitLayer( CollisionLayer.All, false )
					.HitLayer( CollisionLayer.Water, true )
					.Run();

				// Now check a point that is at the player hull midpoint.
				if (point.Fraction == 0.0f)
				{
					// Set a higher water level.
					WaterLevel = WATERLEVEL.Waist;

					point = TraceUtil.CreateBBox(position, position, mins, maxs, pawn, view_pos)
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Water, true )
						.Run();
					// Now check the eye position.  (view_ofs is relative to the origin)
					if (point.Fraction == 0.0f)
						WaterLevel = WATERLEVEL.Eyes;
				}
				// TODO: Add Water current to basevelocity https://github.com/ValveSoftware/source-sdk-2013/blob/master/mp/src/game/shared/gamemovement.cpp#L3557
			}

			// if we just transitioned from not in water to in water, record the time it happened
			if ((WATERLEVEL.NotInWater == OldWaterLevel) && (WaterLevel > WATERLEVEL.NotInWater))
			{
				EntryTime = Time.Now;
			}

			return InWater();
		}

		protected Vector3 GetSwimVel(float max_speed)
		{
			Vector3 forward = Input.Rotation.Forward;
			Vector3 side = Input.Rotation.Left;
			float forward_speed = Input.Forward * max_speed;
			float side_speed = Input.Left * max_speed;
			float up_speed = Input.Up * max_speed;
			Vector3 strafe_vel = (forward * forward_speed) + (side * side_speed);
			
			if (Input.Down(InputButton.Jump))
			{
				strafe_vel.z += max_speed;
			}
			// Sinking after no other movement occurs
			else if (forward_speed == 0 && side_speed == 0 && up_speed == 0)
			{
				strafe_vel.z -= SinkSpeed;
			}
			else
			{
				strafe_vel.z += up_speed + CapWishSpeed(forward_speed * forward.z * 2.0f, max_speed);
			}

			return strafe_vel;
		}

		protected float GetNewSpeed(float length, float friction, ref Vector3 velocity)
		{
			float new_length;

			if (length > 0)
			{
				new_length = length - Time.Delta * length * friction;

				if (new_length < 0.1f)
					new_length = 0;

				velocity *= new_length/length;
			}
			else
				new_length = 0;

			return new_length;
		}

		public virtual void Move(MovementController controller)
		{
			BaseProperties props = controller.Properties;
			Entity pawn = controller.Pawn;
			Vector3 velocity = controller.Velocity;
			Vector3 position = controller.Position;
			Vector3 strafe_vel = GetSwimVel(props.MaxSpeed);
			Vector3 strafe_dir = strafe_vel.Normal;
			Vector3 start_trace;
			Vector3 end_trace;
			TraceResult trace;
			float slow_mod = 0.8f;
			float speed = velocity.Length;
			float new_speed = GetNewSpeed(speed, props.WaterFriction, ref velocity);
			float strafe_vel_length = CapWishSpeed(strafe_vel.Length, props.MaxSpeed) * slow_mod;

			// water acceleration
			if (strafe_vel_length >= 0.1f)
			{
				float add_speed = (strafe_vel_length - new_speed);

				if (add_speed <= 0)
					add_speed = (strafe_vel_length - new_speed) - velocity.Dot(strafe_dir);

				float accel_speed = CapWishSpeed(props.WaterAccelerate * strafe_vel_length * Time.Delta, add_speed);
				velocity += accel_speed * strafe_dir;
			}

			velocity += controller.BaseVelocity;
			end_trace = position + velocity * Time.Delta; 
			trace = TraceUtil.PlayerBBox(position, end_trace, props.OBBMins, props.OBBMaxs, pawn);

			if (trace.Fraction == 1.0f)
			{
				start_trace = end_trace;

				if (props.AllowAutoMovement)
					start_trace.WithZ(start_trace.z + props.StepSize + 1);
				
				trace = TraceUtil.PlayerBBox(start_trace, end_trace, props.OBBMins, props.OBBMaxs, pawn);

				if (!trace.StartedSolid)
				{	
					//float stepDist = trace.EndPos.z - pos.z;
					//mv->m_outStepHeight += stepDist;
					controller.Position = trace.EndPos;
					controller.Velocity = velocity - controller.BaseVelocity;
					return;
				}

				controller.TryPlayerMove();
			}
			else
			{
				if (!controller.OnGround())
				{
					controller.TryPlayerMove();
					controller.Velocity = velocity - controller.BaseVelocity;
					return;
				}

				controller.StepMove();
			}

			controller.Velocity = velocity - controller.BaseVelocity;
		}
	}
}
