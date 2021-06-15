using Sandbox;
using Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OMMovement
{
	public class Duck : NetworkComponent
	{
		protected MovementController Controller;
		public float DuckTime{get; set;} = 0.0f;
		public float DuckJumpTime{get; set;} = 0.0f;
		public float JumpTime{get; set;} = 0.0f;
		public bool IsDucked{get; set;} = false;
		public bool IsDucking{get; set;} = false;
		public bool InDuckJump{get; set;} = false;
		public float DUCK_TIME{get; set;} = 1000.0f;

		public Duck(MovementController controller)
		{
			Controller = controller;
		}

		public virtual Vector3 GetUnDuckOrigin(bool negate)
		{
			Vector3 new_origin = Controller.Position;
			var props = Controller.Properties;

			if (Controller.OnGround())
			{
				new_origin += (props.DuckMins - props.StandMins);
			}
			else
			{
				Vector3 hull_normal = Controller.GetPlayerMaxs(false) - Controller.GetPlayerMins(false);
				Vector3 hull_duck = Controller.GetPlayerMaxs(true) - Controller.GetPlayerMins(true);
				Vector3 view_delta = (hull_normal - hull_duck);

				if (negate)
					view_delta *= -1;

				new_origin += view_delta;
			}

			return new_origin;
		}

		public void FixPlayerCrouchStuck(bool upward)
		{
			int i;
			Vector3 cached_pos;
			TraceResult trace;
			int direction = upward ? 1 : 0;

			trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller.GetPlayerMins(), Controller.GetPlayerMaxs(), Controller.Pawn);

			if (!trace.StartedSolid)
				return;
			
			cached_pos = Controller.Position;

			for (i = 0; i < 36; i++)
			{
				Vector3 org = Controller.Position;

				org = org.WithZ(org.z + direction);
				Controller.Position = org;
				trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller.GetPlayerMins(), Controller.GetPlayerMaxs(), Controller.Pawn);

				if (!trace.StartedSolid)
					return;
			}

			Controller.Position = cached_pos;
		}

		public bool CanUnDuck()
		{
			var props = Controller.Properties;
			Vector3 new_origin = GetUnDuckOrigin(true);
			bool saved_duck = IsDucked;
			TraceResult trace;
			IsDucked = false;
			trace = TraceUtil.PlayerBBox(Controller.Position, new_origin, Controller.GetPlayerMins(false), Controller.GetPlayerMaxs(false), Controller.Pawn);
			IsDucked = saved_duck;

			if (trace.StartedSolid || trace.Fraction != 1.0f)
				return false;

			return true;
		}


		public void FinishUnDuck()
		{
			IsDucked = false;
			IsDucking = false;
			InDuckJump = false;
			Controller.Properties.ViewOffset = Controller.GetPlayerViewOffset(false);
			DuckTime = 0.0f;
			Controller.Position = GetUnDuckOrigin(true);
			Controller.CategorizePosition(Controller.OnGround());
		}

		public void SetDuckedEyeOffset(float frac)
		{
			Vector3 duck_mins = Controller.GetPlayerMins(true);
			Vector3 stand_mins = Controller.GetPlayerMins(false);
			float more = duck_mins.z - stand_mins.z;
			float duck_view = Controller.GetPlayerViewOffset(true);
			float stand_view = Controller.GetPlayerViewOffset(false);
			float view_offset = ((duck_view - more) * frac) + (stand_view * (1 - frac));

			Controller.Properties.ViewOffset = view_offset;
		}

		public virtual void UpdateDuckJumpEyeOffset() 
		{
			if (DuckJumpTime != 0.0f)
			{
				float duck_ms = System.MathF.Max(0.0f, DUCK_TIME - DuckJumpTime);
				float duck_sec = duck_ms / DUCK_TIME;

				if (duck_sec > Controller.Properties.UnDuckSpeed)
				{
					DuckJumpTime = 0.0f;
					SetDuckedEyeOffset(0.0f);
				}
				else
				{
					SetDuckedEyeOffset(Ease.SimpleSpline(1.0f - (duck_sec / Controller.Properties.UnDuckSpeed)));
				}
			}
		}

		public virtual bool CanUnDuckJump(ref TraceResult trace)
		{
			Vector3 end_trace = Controller.Position.WithZ(Controller.Position.z - 36.0f);
			trace = TraceUtil.PlayerBBox(Controller.Position, end_trace, Controller.GetPlayerMins(), Controller.GetPlayerMaxs(), Controller.Pawn);

			if (trace.Fraction < 1.0f)
			{
				end_trace = end_trace.WithZ(Controller.Position.z + (-36.0f * trace.Fraction));

				bool was_ducked = IsDucked;
				IsDucked = false;
				TraceResult trace_up = TraceUtil.PlayerBBox(end_trace, end_trace, Controller.GetPlayerMins(), Controller.GetPlayerMaxs(), Controller.Pawn);
				IsDucked = was_ducked;

				if (!trace_up.StartedSolid)
					return true;
			}

			return false;
		}

		public virtual void ReduceTimers()
		{
			float frame_msec = 1000.0f * Time.Delta;

			if (DuckTime > 0)
			{
				DuckTime -= frame_msec;
				if (DuckTime < 0)
				{
					DuckTime = 0;
				}
			}

			if (DuckJumpTime > 0)
			{
				DuckJumpTime -= frame_msec;
				if (DuckJumpTime < 0)
				{
					DuckJumpTime = 0;
				}
			}

			if (JumpTime > 0)
			{
				JumpTime -= frame_msec;
				if (JumpTime < 0)
				{
					JumpTime = 0;
				}
			}
		}

		public void FinishUnDuckJump(TraceResult trace)
		{
			Vector3 new_origin = Controller.Position;
			var props = Controller.Properties;
			Vector3 hull_normal = Controller.GetPlayerMaxs(false) - Controller.GetPlayerMins(false);
			Vector3 hull_duck = Controller.GetPlayerMaxs(true) - Controller.GetPlayerMins(true);
			Vector3 view_delta = (hull_normal - hull_duck);
			float delta_z = view_delta.z;
			
			view_delta.z *= trace.Fraction;
			delta_z -= view_delta.z;
			IsDucked = false;
			IsDucking = false;
			InDuckJump = false;
			DuckTime = 0.0f;
			DuckJumpTime = 0.0f;
			JumpTime = 0.0f;

			float view_offset = Controller.GetPlayerViewOffset(false);

			view_offset -= delta_z;
			Controller.Properties.ViewOffset = view_offset;
			new_origin -= view_delta;
			Controller.Position = new_origin;
			Controller.CategorizePosition(Controller.OnGround());
		}

		public void FinishDuck()
		{
			if (!IsDucked)
			{
				Controller.SetTag( "ducked" );
				IsDucked = true;
				IsDucking = false;
				Controller.Properties.ViewOffset = Controller.GetPlayerViewOffset(true);

				if (Controller.OnGround())
				{
					for (int i = 0; i < 3; i++)
					{
						Vector3 org = Controller.Position;
						org[i]-= ( Controller.GetPlayerMins(true)[i] - Controller.GetPlayerMins(false)[i] );
						Controller.Position = org;
					}
				}
				else
				{
				Controller.Position = GetUnDuckOrigin(false);
				}

				FixPlayerCrouchStuck(true);
				Controller.CategorizePosition(Controller.OnGround());
			}
		}

		public void StartUnDuckJump()
		{
			Controller.SetTag( "ducked" );
			IsDucked = true;
			IsDucking = false;
			Controller.Properties.ViewOffset = Controller.GetPlayerViewOffset(true);
			Controller.Position = GetUnDuckOrigin(false);
			FixPlayerCrouchStuck(true);
			Controller.CategorizePosition(Controller.OnGround());
		}
		
		public void TryDuck()
		{
			// Check to see if we are in the air.
			bool in_air = !Controller.OnGround();
			bool duck_jump = JumpTime > 0.0f;
			bool duck_jump_time = DuckJumpTime > 0.0f;
			bool duck_keydown = Input.Down(InputButton.Duck);
			bool in_duck = IsDucked;
			float time_to_unduck_inv = Controller.Properties.DuckSpeed - Controller.Properties.UnDuckSpeed;
			// Slow down ducked players.
			//HandleDuckingSpeedCrop();
			// If the player is holding down the duck button, the player is in duck transition, ducking, or duck-jumping.
			if (duck_keydown || IsDucking  || in_duck || duck_jump)
			{
				// DUCK
				if (duck_keydown || duck_jump)
				{
					// Have the duck button pressed, but the player currently isn't in the duck position.
					if (Input.Pressed(InputButton.Duck) && !in_duck && !duck_jump && !duck_jump_time)
					{
						DuckTime = DUCK_TIME;
						IsDucking = true;
					}
					
					// The player is in duck transition and not duck-jumping.
					if (IsDucking && !duck_jump && !duck_jump_time)
					{
						float duck_ms = System.MathF.Max(0.0f, DUCK_TIME - DuckTime);
						float duck_second = duck_ms * 0.001f;
						// Finish in duck transition when transition time is over, in "duck", in air.
						if ((duck_second > Controller.Properties.DuckSpeed) || in_duck || in_air)
						{
							FinishDuck();
						}
						else
						{
							// Calc parametric time
							float duck_frac = Ease.SimpleSpline(duck_second / Controller.Properties.DuckSpeed);
							SetDuckedEyeOffset(duck_frac);
						}
					}

					if (duck_jump)
					{
						// Make the bounding box small immediately.
						if (!in_duck)
						{
							StartUnDuckJump();
						}
						else
						{
							// Check for a crouch override.
							if (!duck_keydown)
							{
								TraceResult trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller.GetPlayerMins(), Controller.GetPlayerMaxs(), Controller.Pawn);
								if (CanUnDuckJump(ref trace))
								{
									FinishUnDuckJump(trace);
									DuckJumpTime = (Controller.Properties.UnDuckSpeed * (1.0f - trace.Fraction)) + time_to_unduck_inv;
								}
							}
						}
					}
				}
				// UNDUCK (or attempt to...)
				else
				{
					if (InDuckJump)
					{
						// Check for a crouch override.
						if (!duck_keydown)
						{
							TraceResult trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller.GetPlayerMins(), Controller.GetPlayerMaxs(), Controller.Pawn);

							if (CanUnDuckJump(ref trace))
							{
								Sandbox.BetterLog.Info(trace.Fraction);
								FinishUnDuckJump(trace);
							
								if (trace.Fraction < 1.0f)
								{
									DuckJumpTime = (Controller.Properties.UnDuckSpeed * (1.0f - trace.Fraction)) + time_to_unduck_inv;
								}
							}
						}
						else
						{
							InDuckJump = false;
						}
					}

					if (duck_jump_time)
						return;

					// Try to unduck unless automovement is not allowed
					// NOTE: When not onground, you can always unduck
					if (Controller.Properties.AllowAutoMovement || in_air || IsDucking)
					{
						// We released the duck button, we aren't in "duck" and we are not in the air - start unduck transition.
						if (Input.Released(InputButton.Duck))
						{
							if (in_duck && !duck_jump)
							{
								DuckTime = DUCK_TIME;
							}
							else if (IsDucking && !in_duck)
							{
								// Invert time if release before fully ducked!!!
								float unduck_ms = 1000.0f * Controller.Properties.UnDuckSpeed;
								float duck_ms = 1000.0f * Controller.Properties.DuckSpeed;
								float elapsed_ms = DUCK_TIME - DuckTime;
								float frac_ducked = elapsed_ms / duck_ms;
								float remaining_unduck_ms = frac_ducked * duck_ms;

								DuckTime = DUCK_TIME - duck_ms + remaining_unduck_ms;
							}
						}
						

						// Check to see if we are capable of unducking.
						if (CanUnDuck())
						{
							// or unducking
							if ((IsDucking || IsDucked))
							{
								float duck_ms = System.MathF.Max(0.0f, DUCK_TIME - DuckTime);
								float duck_second = duck_ms * 0.001f;
								
								// Finish ducking immediately if duck time is over or not on ground
								if (duck_second > Controller.Properties.UnDuckSpeed || (in_air && !duck_jump))
								{
									FinishUnDuck();
								}
								else
								{
									// Calc parametric time
									float duck_frac = Ease.SimpleSpline(1.0f - (duck_second / Controller.Properties.UnDuckSpeed));
									
									SetDuckedEyeOffset(duck_frac);
									IsDucking = true;
								}
							}
						}
						else
						{
							// Still under something where we can't unduck, so make sure we reset this timer so
							//  that we'll unduck once we exit the tunnel, etc.
							if (DuckTime != DUCK_TIME)
							{
								SetDuckedEyeOffset(1.0f);
								DuckTime = DUCK_TIME;
								IsDucked = true;
								IsDucking = false;
								Controller.SetTag( "ducked" );
							}
						}
					}
				}
			}

			ReduceTimers();
			if (IsDucked)
				Controller.SetTag( "ducked" );
		}
	}
}
