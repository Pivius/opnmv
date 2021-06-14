using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMMovement
{
	public enum MODE : ulong
	{
		SOURCE = 1,
		VQ3 = 2,
		CPMA = 4,
		GRAPPLE = 8,
		PARKOUR = 16
	}

	public enum STATE : int
	{
		GROUND = 0,
		INAIR,
		LADDER,
		WATER
	}

		public enum WATERLEVEL : ulong
	{
		NotInWater = 0,
		Feet,
		Waist,
		Eyes
	}
}
