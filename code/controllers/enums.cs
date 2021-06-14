using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMMovement
{
	public enum MODE : int
	{
		SOURCE = 0,
		VQ3,
		CPMA,
		GRAPPLE,
		PARKOUR
	}

	public enum STATE : int
	{
		GROUND = 0,
		INAIR,
		LADDER,
		WATER
	}

		public enum WATERLEVEL : int
	{
		NotInWater = 0,
		Feet,
		Waist,
		Eyes
	}
}
