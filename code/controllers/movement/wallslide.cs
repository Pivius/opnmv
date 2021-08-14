using System.Diagnostics;
using Sandbox;
using System;
using Core;

namespace OMMovement
{
	public class WallSlide
	{
		// EMM Wallslide
		public bool CanWallSlide = true;
		public bool IsInfinite = false;
		public bool WallSliding = false;
		public Vector3 Velocity;
		public float Distance = 40;
		public float RegenStep = 0.2f;
		public float DecayStep = 0.2f;
		public float CoolDown = 2;
		public float InitCost = 5;
		public float Time = 0;

		public WallSlide()
		{}

	}
}
