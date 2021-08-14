using Sandbox;
using Core;
using System;


namespace OMMovement
{
	public partial class Duck : NetworkComponent
	{
		protected MovementController Controller;
		private TimeAssociatedMap<float> ClientDuckTime;
		private TimeSince ServerDuckTime;
		public float DuckTime 
		{
			get
			{
				if (Host.IsServer)
					return (float) ServerDuckTime;
				else if(Host.IsClient)
					return MathF.Max(0, (Time.Now + ClientDuckTime.LastValue) - ClientDuckTime.LastTime);
				return 0;
			}

			set
			{
				if (Host.IsServer)
					ServerDuckTime = value;
				else if(Host.IsClient)
					ClientDuckTime.Set(value);
			}
		}
		public bool IsDucked {get; set;}
		public bool InDuckJump {get; set;}
		public float DUCK_TIME{get; set;} = 1000.0f;

		public Duck(MovementController controller)
		{
			Controller = controller;
			ClientDuckTime = new TimeAssociatedMap<float>(1, GetDuckTime);
		}

		private float GetDuckTime()
		{
			return (float) ServerDuckTime;
		}

		private bool IsDucking()
		{
			return ((DuckTime <= Controller.Properties.UnDuckSpeed && IsDucked) || (DuckTime <= Controller.Properties.DuckSpeed && !IsDucked));
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
			bool saved_duck = IsDucked;
			TraceResult trace;
			
			InDuckJump = false;
			IsDucked = false;
			DuckTime = 99;
			new_origin += hull_delta;
			trace = TraceUtil.PlayerBBox(Controller.Position, new_origin, Controller);
			Controller.ViewOffset = Controller.GetPlayerViewOffset(false);
			Controller.Position = trace.EndPos;
			Controller.CategorizePosition(Controller.OnGround());
		}

		public void FinishUnDuck()
		{
			IsDucked = false;
			DuckTime = 99;
			Controller.ViewOffset = Controller.GetPlayerViewOffset(false);
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

			Controller.ViewOffset = view_offset;
		}

		public void FinishDuck()
		{
			if (!IsDucked)
			{
				if (Controller.OnGround())
					InDuckJump = false;

				IsDucked = true;
				DuckTime = 99;
				Controller.ViewOffset = Controller.GetPlayerViewOffset(true);

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
			bool duck_keydown = Controller.GetPlayer().KeyDown(InputButton.Duck);
			bool in_duck = IsDucked;
			float time_to_unduck_inv = Controller.Properties.DuckSpeed - Controller.Properties.UnDuckSpeed;
			
			if (duck_keydown || IsDucking() || in_duck)
			{
				// DUCK
				if (duck_keydown)
				{

						// Have the duck button pressed, but the player currently isn't in the duck position.
						if (Controller.GetPlayer().KeyPressed(InputButton.Duck) && !in_duck)
						{
							DuckTime = 0;
						}
						else if (Controller.GetPlayer().KeyPressed(InputButton.Duck) && IsDucking())
						{
							FinishUnDuck();
							DuckTime = 0;
						}
						
						// The player is in duck transition
						if (IsDucking() && !in_duck && !in_air)
						{
							// Calc parametric time
							SetDuckedEyeOffset(SimpleSpline(DuckTime / Controller.Properties.DuckSpeed));
						}
						else
						{
							FinishDuck();
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
					else if (Controller.OnGround() && IsDucked && InDuckJump && CanUnDuck())
						FinishUnDuckJump();

					// Try to unduck unless automovement is not allowed
					// NOTE: When not onground, you can always unduck
					if (Controller.Properties.AllowAutoMovement || in_air || IsDucking())
					{
						if (Controller.GetPlayer().KeyReleased(InputButton.Duck))
						{
							if (in_duck)
							{
								DuckTime = 0;
							}
							else if (IsDucking() && !in_duck)
							{
								// Invert time if release before fully ducked!!!
								float unduck_time = Controller.Properties.UnDuckSpeed;
								float duck_time = Controller.Properties.DuckSpeed;
								float elapsed_time = DuckTime;
								float frac_ducked = elapsed_time / duck_time;
								float remaining_unduck_time = frac_ducked * duck_time;

								DuckTime = remaining_unduck_time;
							}
						}
						

						// Check to see if we are capable of unducking.
						if (CanUnDuck())
						{
							// or unducking
							if (IsDucking() && IsDucked && !in_air)
							{
								// Calc parametric time
								float duck_frac = SimpleSpline(1.0f - (DuckTime / Controller.Properties.UnDuckSpeed));
								
								SetDuckedEyeOffset(duck_frac);
							}
							else if ((!IsDucking() || in_air) && IsDucked)
							{
								FinishUnDuck();
							}
						}
						else
						{
							// Still under something where we can't unduck, so make sure we reset this timer so
							//  that we'll unduck once we exit the tunnel, etc.
							if (DuckTime != 0)
							{
								
								SetDuckedEyeOffset(1.0f);
								DuckTime = 0;
								IsDucked = true;
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
			
			
			//ReduceTimers();

			if (IsDucked)
				Controller.SetTag( "ducked" );
		}
	}
}
