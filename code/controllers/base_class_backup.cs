
/**
		public Vector3 ClipVelocity(Vector3 vel, Vector3 normal, float bounce)
		{
			var new_velocity = new Vector3();
			float adjust;

			new_velocity = vel - (normal * vel.Dot(normal) * bounce);
			adjust = new_velocity.Dot(normal);

			if(adjust < 0.0f)
				new_velocity -= normal * adjust;

			return new_velocity;
		}

		public virtual void ClearGroundEntity()
		{
			if (GroundEntity == null) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
		}
		
		public bool CheckJumpButton()
		{
			bool MovementOptimizations = true;

			if (Classes.Water.WaterJump != 0)
			{
				Classes.Water.WaterJump -= Time.Delta;

				if (Classes.Water.WaterJump < 0)
					Classes.Water.WaterJump = 0;
				
				return false;
			}

			// If we are in the water most of the way...
			if (Classes.Water.WaterLevel >= 2)
			{	
				// swimming, not jumping
				ClearGroundEntity();
				Velocity = Velocity.WithZ(100);

				return false;
			}

			// No more effect
			if (!OnGround())
			{
				return false;
			}

			//if ( mv->m_nOldButtons & IN_JUMP )
			//	return false;		// don't pogo stick

			// Cannot jump while in the unduck transition.
			if (Classes.Duck.IsDucking && !Input.Down(InputButton.Duck) && !Properties.CanUnDuckJump)
				return false;

			// Still updating the eye position.
			if (Classes.Duck.DuckTime > 0.0f && !Properties.CanUnDuckJump )
				return false;


			// In the air now.
			ClearGroundEntity();


			float flMul;
			if (MovementOptimizations)
			{
				flMul = 268.3281572999747f;
			}
			else
			{
				//GAMEMOVEMENT_JUMP_HEIGHT = 21
				flMul = MathF.Sqrt(2.0f * Classes.Gravity.Value * 21.0f);
			}

			// Acclerate upward
			// If we are ducking...
			float startz = Velocity.z;
			if (Classes.Duck.IsDucking)
			{
				Velocity.WithZ(flMul);
			}
			else
			{
				Velocity.WithZ(Velocity.z + flMul);
			}

			//FinishGravity();
			AddEvent("jump");

			return true;
		}

		public virtual void TryPlayerMove(Vector3 *first_dest, TraceResult *first_trace)
		{
			Vector3 current_velocity = Velocity;
			Vector3 primal_velocity = current_velocity;
			Vector3 original_velocity = current_velocity;
			Vector3 new_velocity = new Vector3();
			//Vector3[] planes = new Vector3[Properties.MaxClipPlanes];
			var planes = ArrayPool<Vector3>.Shared.Rent(Properties.MaxClipPlanes);
			Vector3 end_trace;
			var trace;
			int numbumps = 4;
			int numplanes = 0;
			int	i, j;
			float time_left = Time.Delta;
			float allFraction = 0;
			bool MovementOptimizations = true;

			for (bumpcount=0 ; bumpcount < numbumps; bumpcount++)
			{
				current_velocity = Velocity;

				if (current_velocity.Length == 0.0)
					break;

				// Assume we can move all the way from the current origin to the
				//  end point.
				end_trace = Position + current_velocity * time_left;

				// See if we can make it from origin to end point.
				if (g_bMovementOptimizations)
				{
					// If their velocity Z is 0, then we can avoid an extra trace here during WalkMove.
					if (first_dest && end_trace == *first_dest)
						trace = *first_trace;
					else
					{
						trace = TraceBBox(Position, end_trace);
					}
				}
				else
				{
					trace = TraceBBox(Position, end_trace);
				}

				allFraction += trace.Fraction;

				// If we started in a solid object, or we were in solid space
				//  the whole way, zero out our velocity and return that we
				//  are blocked by floor and wall.
				if (trace.StartedSolid)
				{	
					// entity is trapped in another solid
					Velocity = Velocity.Zero;
					return 4;
				}

				// If we moved some portion of the total distance, then
				//  copy the end position into the pmove.origin and 
				//  zero the plane counter.
				if(trace.Fraction > 0)
				{	
					if (numbumps > 0 && trace.Fraction == 1)
					{
						// There's a precision issue with terrain tracing that can cause a swept box to successfully trace
						// when the end position is stuck in the triangle.  Re-run the test with an uswept box to catch that
						// case until the bug is fixed.
						// If we detect getting stuck, don't allow the movement
						var stuck = TraceBBox(trace.EndPos, trace.EndPos);

						if (stuck.StartedSolid || stuck.Fraction != 1.0f)
						{
							Velocity = Velocity.Zero;
							break;
						}
					}

					// actually covered some distance
					Position = trace.EndPos;
					Velocity = original_velocity;
					numplanes = 0;
				}

				// If we covered the entire distance, we are done
				//  and can return.
				if (trace.Fraction == 1)
					break;		// moved the entire distance

				// Reduce amount of m_flFrameTime left by total time left * fraction
				//  that we covered.
				time_left -= time_left * trace.Fraction;

				// Did we run out of planes to clip against?
				if (numplanes >= Properties.MaxClipPlanes)
				{	
					// this shouldn't really happen
					//  Stop our movement if so.
					Velocity = Velocity.Zero;
					break;
				}

				// Set up next clipping plane
				planes[numplanes] = trace.Normal;
				numplanes++;

				// modify original_velocity so it parallels all of the clip planes
				// reflect player velocity 
				// Only give this a try for first impact plane because you can get yourself stuck in an acute corner by jumping in place
				//  and pressing forward and nobody was really using this bounce/reflection feature anyway...
				if (numplanes == 1 && player->GetMoveType() == MOVETYPE.WALK && OnGround())	
				{
					for (i = 0; i < numplanes; i++)
					{
						if (planes[i].z > 0.7)
						{
							// floor or slope
							new_velocity = ClipVelocity(original_velocity, planes[i], 1);
							original_velocity = new_velocity;
						}
						else
						{
							new_velocity = ClipVelocity(original_velocity, planes[i], 1); //+ sv_bounce.GetFloat() * (1 - player->m_surfaceFriction)
						}
					}
					Velocity = Velocity.new_velocity;
					original_velocity = new_velocity;
				}
				else
				{
					for (i=0 ; i < numplanes ; i++)
					{
						Velocity = ClipVelocity(original_velocity, planes[i], 1);

						for (j=0 ; j<numplanes ; j++)
							if (j != i)
							{
								// Are we now moving against this plane?
								if (Velocity.Dot(planes[j]) < 0)
									break;	// not ok
							}
						if (j == numplanes)  // Didn't have to clip, so we're ok
							break;
					}
					
					if (i == numplanes)
					{	// go along the crease
						if (numplanes != 2)
						{
							Velocity = Velocity.Zero;
							break;
						}
						var dir = planes[0].Cross(planes[1]).Normal;
						Velocity = dir * dir.Dot(Velocity);
					}

					//
					// if original velocity is against the original velocity, stop dead
					// to avoid tiny occilations in sloping corners
					//
					if (Velocity.Dot(primal_velocity) <= 0)
					{
						Velocity = Velocity.Zero;
						break;
					}
				}
			}

			if (allFraction == 0)
			{
				Velocity = Velocity.Zero;
			}
		}

		public void CategorizePosition()
		{
			Vector3 point = Position;
			var trace;

			// Doing this before we move may introduce a potential latency in water detection, but
			// doing it after can get us stuck on the bottom in water if the amount we move up
			// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
			// this several times per frame, so we really need to avoid sticking to the bottom of
			// water on each call, and the converse case will correct itself if called twice.
			Classes.Water.CheckWater();

			// observers don't have a ground entity
			if (Properties.MoveType == MOVETYPE.OBSERVER)
				return;

			float offset = 2.0f;
			point = point.WithZ(point.z - offset);

			Vector3 bump_origin = Position;

			// Shooting up really fast.  Definitely not on ground.
			// On ladder moving up, so not on ground either
			// NOTE: 145 is a jump.

			float zvel = Velocity.z;
			bool moving_up = zvel > 0.0f;
			bool moving_up_rapidly = zvel > Properties.JumpPower - 5;
			float ground_ent_velz = 0.0f;

			if (moving_up_rapidly)
			{
				if (OnGround() && GroundEntity.Velocity)
				{
					ground_ent_velz = GroundEntity.Velocity.z;
					moving_up_rapidly = (zvel - ground_ent_velz ) > Properties.JumpPower;
				}
			}

			// Was on ground, but now suddenly am not
			if ( moving_up_rapidly || ( moving_up && Properties.MoveType == MOVETYPE_LADDER ) )   
			{
				ClearGroundEntity();
			}
			else
			{
				// Try and move down.
				trace = TraceBBox(bump_origin, point)
				
				// Was on ground, but now suddenly am not.  If we hit a steep plane, we are not on ground
				if ( !trace.Entity || trace.Normal.z < 0.7 )
				{
					// Test four sub-boxes, to see if any of them would have found shallower slope we could actually stand on
					TryTouchGroundInQuadrants( bumpOrigin, point, MASK_PLAYERSOLID, COLLISION_GROUP_PLAYER_MOVEMENT, pm );

					if ( !pm.m_pEnt || pm.plane.normal[2] < 0.7 )
					{
						ClearGroundEntity();
						// probably want to add a check for a +z velocity too!
						if ( ( mv->m_vecVelocity.z > 0.0f ) && 
							( player->GetMoveType() != MOVETYPE_NOCLIP ) )
						{
							player->m_surfaceFriction = 0.25f;
						}
					}
					else
					{
						SetGroundEntity( &pm );
					}
				}
				else
				{
					SetGroundEntity( &pm );  // Otherwise, point to index of ent under us.
				}

		#ifndef CLIENT_DLL
				
				//Adrian: vehicle code handles for us.
				if ( player->IsInAVehicle() == false )
				{
					// If our gamematerial has changed, tell any player surface triggers that are watching
					IPhysicsSurfaceProps *physprops = MoveHelper()->GetSurfaceProps();
					surfacedata_t *pSurfaceProp = physprops->GetSurfaceData( pm.surface.surfaceProps );
					char cCurrGameMaterial = pSurfaceProp->game.material;
					if ( !player->GetGroundEntity() )
					{
						cCurrGameMaterial = 0;
					}

					// Changed?
					if ( player->m_chPreviousTextureType != cCurrGameMaterial )
					{
						CEnvPlayerSurfaceTrigger::SetPlayerSurface( player, cCurrGameMaterial );
					}

					player->m_chPreviousTextureType = cCurrGameMaterial;
				}
		#endif
			}
		}

		public virtual void FullWalkMove()
		{
			if (!Classes.Water.CheckWater())
			{
				Classes.Gravity.StartGravity();
			}

			if (Classes.Water.JumpTime != 0)
			{
				Classes.Water.Jump();
				TryPlayerMove();
				Classes.Water.CheckWater();

				return;
			}

			if (Classes.Water.WaterLevel >= WATERLEVEL.Waist) 
			{
				if (Classes.Water.WaterLevel == WATERLEVEL.Waist)
				{
					Classes.Water.CheckJump();
				}

					// If we are falling again, then we must not trying to jump out of water any more.
				if (Velocity.z < 0 && Classes.Water.JumpTime != 0)
				{
					Classes.Water.JumpTime = 0;
				}

				// Was jump button pressed?
				if (Input.Pressed(InputButton.Jump))
				{
					CheckJumpButton();
				}

				// Perform regular water movement
				Classes.Water.Move();

				// Redetermine position vars
				CategorizePosition();

				// If we are on ground, no downward velocity.
				if (OnGround())
				{
					Velocity.WithZ(0);
				}
			}
			else
			// Not fully underwater
			{
				// Was jump button pressed?
				if (Input.Pressed(InputButton.Jump))
				{
					CheckJumpButton();
				}

				// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor, 
				//  we don't slow when standing still, relative to the conveyor.
				if (OnGround())
				{
					Velocity.WithZ(0);
					Classes.Friction.Move();
				}

				// Make sure velocity is valid.
				CheckVelocity();

				if (OnGround())
				{
					WalkMove();
				}
				else
				{
					AirMove();  // Take into account movement when in air.
				}

				// Set final flags.
				CategorizePosition();

				// Make sure velocity is valid.
				CheckVelocity();

				// Add any remaining gravitational component.
				if (!Classes.Water.CheckWater())
				{
					FinishGravity();
				}

				// If we are on ground, no downward velocity.
				if (OnGround())
				{
					Velocity.WithZ(0);
				}
				
				CheckFalling();
			}
		}
		**/
