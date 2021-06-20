using Sandbox;
using Core;
using System;


namespace OMMovement
{
	public class Duck : NetworkComponent
	{
		protected MovementController Controller;
		public float DuckTime{get; set;} = 0.0f;
		public bool IsDucked{get; set;} = false;
		public bool IsDucking{get; set;} = false;
		public bool InDuckJump{get; set;} = false;
		public float DUCK_TIME{get; set;} = 1000.0f;

		public Duck(MovementController controller)
		{
			Controller = controller;
		}

		private float SimpleSpline(float value)
		{
			float value_squared = value * value;

			return (3 * value_squared - 2 * value_squared * value);
		}

		public virtual Vector3 GetUnDuckOrigin(bool negate)
		{
			Vector3 new_origin = Controller.Position;
			var props = Controller.Properties;

			if (Controller.OnGround())
				new_origin += (props.DuckMins - props.StandMins);
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

			trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller);

			if (!trace.StartedSolid)
				return;
			
			cached_pos = Controller.Position;

			for (i = 0; i < 36; i++)
			{
				Vector3 org = Controller.Position;

				org = org.WithZ(org.z + direction);
				Controller.Position = org;
				trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller);

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
			trace = TraceUtil.PlayerBBox(Controller.Position, new_origin, Controller);
			IsDucked = saved_duck;

			if (trace.StartedSolid || trace.Fraction != 1.0f)
				return false;

			return true;
		}

		public bool CanUnDuckJump()
		{
			var props = Controller.Properties;
			Vector3 new_origin = GetUnDuckOrigin(true);
			bool saved_duck = IsDucked;
			TraceResult trace;

			//new_origin += hull_delta;
			IsDucked = false;
			trace = TraceUtil.PlayerBBox(Controller.Position, new_origin, Controller);
			IsDucked = saved_duck;

			return trace.Hit;
		}

		public void FinishUnDuckJump()
		{
			Vector3 hull_normal = Controller.GetPlayerMaxs(false) - Controller.GetPlayerMins(false);
			Vector3 hull_duck = Controller.GetPlayerMaxs(true) - Controller.GetPlayerMins(true);
			Vector3 hull_delta = (hull_normal - hull_duck) * -1;
			Vector3 new_origin = Controller.Position;
			TraceResult trace;
			
			InDuckJump = false;
			IsDucked = false;
			IsDucking = false;
			DuckTime = 0.0f;
			new_origin += hull_delta;
			trace = TraceUtil.PlayerBBox(Controller.Position, new_origin, Controller);
			Controller.Properties.ViewOffset = Controller.GetPlayerViewOffset(false);
			Controller.CategorizePosition(Controller.OnGround());
		}

		public void FinishUnDuck()
		{
			IsDucked = false;
			IsDucking = false;
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

		public virtual void ReduceTimers()
		{
			float frame_msec = 1000.0f * Time.Delta;

			if (DuckTime > 0)
			{
				DuckTime -= frame_msec;
				if (DuckTime < 0)
					DuckTime = 0;
			}
		}

		public void FinishDuck()
		{
			if (!IsDucked)
			{
				if (Controller.OnGround())
					InDuckJump = false;

				IsDucked = true;
				IsDucking = false;
				Controller.Properties.ViewOffset = Controller.GetPlayerViewOffset(true);

				if (Controller.OnGround())
				{
					for (int i = 0; i < 3; i++)
					{
						Vector3 org = Controller.Position;
						org[i]-= (Controller.GetPlayerMins(true)[i] - Controller.GetPlayerMins(false)[i]);
						Controller.Position = org;
					}
				}
				else
					Controller.Position = GetUnDuckOrigin(false);

				FixPlayerCrouchStuck(true);
				Controller.CategorizePosition(Controller.OnGround());
			}
		}

		public void TryDuck()
		{
			bool in_air = !Controller.OnGround();
			bool duck_keydown = Input.Down(InputButton.Duck);
			bool in_duck = IsDucked;
			float time_to_unduck_inv = Controller.Properties.DuckSpeed - Controller.Properties.UnDuckSpeed;

			if (duck_keydown || IsDucking || in_duck)
			{
				// DUCK
				if (duck_keydown)
				{
					// Have the duck button pressed, but the player currently isn't in the duck position.
					if (Input.Pressed(InputButton.Duck) && !in_duck)
					{
						DuckTime = DUCK_TIME;
						IsDucking = true;
					}
					else if (Input.Pressed(InputButton.Duck) && IsDucking)
					{
						FinishUnDuck();
						DuckTime = DUCK_TIME;
						IsDucking = true;
					}
					
					// The player is in duck transition
					if (IsDucking)
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
							SetDuckedEyeOffset(SimpleSpline(duck_second / Controller.Properties.DuckSpeed));
						}
					}
				}
				// UNDUCK (or attempt to...)
				else
				{
					if ((Controller.Velocity.z < 0.0f) && IsDucked)
					{
						InDuckJump = true;

						if (CanUnDuckJump())
							return;
					}
					else if ((Controller.Velocity.z >= 0.0f) && IsDucked && InDuckJump && CanUnDuck())
						FinishUnDuckJump();

					// Try to unduck unless automovement is not allowed
					// NOTE: When not onground, you can always unduck
					if (Controller.Properties.AllowAutoMovement || in_air || IsDucking)
					{
						if (Input.Released(InputButton.Duck))
						{
							if (in_duck)
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
							if (IsDucking || IsDucked)
							{
								float duck_ms = System.MathF.Max(0.0f, DUCK_TIME - DuckTime);
								float duck_second = duck_ms * 0.001f;
								
								// Finish ducking immediately if duck time is over or not on ground
								if (duck_second > Controller.Properties.UnDuckSpeed || in_air)
								{
									FinishUnDuck();
								}
								else
								{
									// Calc parametric time
									float duck_frac = SimpleSpline(1.0f - (duck_second / Controller.Properties.UnDuckSpeed));
									
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
			else
			{
				if (MathF.Abs(Controller.GetViewOffset() - Controller.GetPlayerViewOffset(false)) > 0.1f)
				{
					SetDuckedEyeOffset(0.0f);
				}
			}
			
			ReduceTimers();

			if (IsDucked)
				Controller.SetTag( "ducked" );
		}
	}
}
