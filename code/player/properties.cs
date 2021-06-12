using Sandbox;
using OMMovement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public partial class MovementPlayer : Player
	{
		[Net, Predicted] public static Rotation ViewAngle{get;set;}
	}
}
