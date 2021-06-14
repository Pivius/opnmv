using Sandbox;
using Core;

namespace OMMovement
{
    public class Water : Accelerate
    {
		public float JumpTime {get; set;}
		public Vector3 JumpVel {get; set;}
		public float EntryTime {get; set;}
		public float JumpHeight {get; set;} = 8.0f;
		public WATERLEVEL WaterLevel {get; set;} = 0;
		public WATERLEVEL OldWaterLevel {get; set;} = 0;

		public virtual Vector3 CheckWaterJump(Vector3 velocity, Vector3 position, BaseProperties props, Entity pawn)
		{
			Vector3 forward = Input.Rotation.Forward;
			Vector3	flat_forward = forward.WithZ(0);
			Vector3	flat_velocity = velocity;

			// Already water jumping.
			if (JumpTime == 0)
			{
			// Don't hop out if we just jumped in
				if (velocity.z >= -180.0f) // only hop out if we are moving up
				{
					// See if we are backing up
					flat_velocity = flat_velocity.WithZ(0);
					
					// see if near an edge
					flat_forward = flat_forward.Normal;
		
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
									// Push Up
									velocity = velocity.WithZ(300.0f);
									// Do this for 2 seconds
									JumpTime = 200.0f;
									//player->AddFlag( FL_WATERJUMP );
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

		public virtual void Move(MovementController controller)
		{
			Vector3 forward = Input.Rotation.Forward;
			Vector3 side = new Vector3(-forward.y, forward.x, 0);
			BaseProperties props = controller.Properties;
			Entity pawn = controller.Pawn;
			Vector3 velocity = controller.Velocity;
			Vector3 position = controller.Position;
			Vector3 strafe_dir;
			Vector3 start_trace;
			Vector3 end_trace;
			TraceResult trace;
			float speed;
			float accel_speed;
			float new_speed;
			float add_speed;
			float strafe_vel_length;
			Vector3 strafe_vel = (forward * (Input.Forward * props.MaxSpeed)) + (side * (Input.Left * props.MaxSpeed));

			// if we have the jump key down, move us up as well
			if (Input.Down(InputButton.Jump))
			{
				strafe_vel = strafe_vel.WithZ(strafe_vel.z + props.MaxSpeed);
			}
			// Sinking after no other movement occurs
			else if (Input.Forward == 0 && Input.Left == 0 && Input.Up == 0)
			{
				strafe_vel = strafe_vel.WithZ(strafe_vel.z - 60.0f);
				
			}
			else  // Go straight up by upmove amount.
			{
				// exaggerate upward movement along forward as well
				strafe_vel = strafe_vel.WithZ(strafe_vel.z + ((Input.Up * props.MaxSpeed) + MathX.Clamp((Input.Forward * props.MaxSpeed * forward.z * 2.0f), 0.0f, props.MaxSpeed)));
			}

			// Copy it over and determine speed
			strafe_dir = strafe_vel.Normal;
			strafe_vel_length = CapWishSpeed(strafe_vel.Length, props.MaxSpeed);

			// Slow us down a bit.
			strafe_vel_length *= 0.8f;
			
			// Water friction
			speed = velocity.Length;

			if (speed > 0)
			{
				new_speed = speed - Time.Delta * speed * props.Friction;

				if (new_speed < 0.1f)
				{
					new_speed = 0;
				}

				velocity *= new_speed/speed;
			}
			else
			{
				new_speed = 0;
			}

			// water acceleration
			if (strafe_vel_length >= 0.1f)
			{
				add_speed = strafe_vel_length - new_speed;

				if (add_speed > 0)
				{
					var delta_speed = GetAccelSpeed(strafe_dir, strafe_vel_length, add_speed, props.WaterAccelerate);
					velocity += delta_speed;
				}
			}
			velocity += controller.BaseVelocity;

			// Now move
			// assume it is a stair or a slope, so press down from stepheight above
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
					controller.Velocity = velocity;
					controller.Velocity -= controller.BaseVelocity;
					return;
				}

				// Try moving straight along out normal path.
				controller.TryPlayerMove();
			}
			else
			{
				if (!controller.OnGround())
				{
					controller.TryPlayerMove();
					controller.Velocity = velocity;
					controller.Velocity -= controller.BaseVelocity;
					return;
				}

				//controller.StepMove(end_trace, trace);
				controller.StepMove();
			}
			controller.Velocity = velocity;
			controller.Velocity -= controller.BaseVelocity;
		}
	}
}
