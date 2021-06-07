using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMCore
{
	partial class MovementPlayer : BasePlayer
	{
		public override void InitProperties()
		{
			movement_mode = OMMovement.MODE.SOURCE;
		}
	}
}