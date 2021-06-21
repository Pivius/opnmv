using Sandbox;
using OMMovement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public partial class MovementPlayer
	{
		public Vector2 PreviousDelta {get; set;}
		public ulong Buttons {get; set;}
		public ulong OldButtons {get; set;}
	}
}
