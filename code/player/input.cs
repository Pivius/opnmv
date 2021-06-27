using System.Linq;
using Sandbox;
using OMMovement;
using System;
using System.Collections.Generic;

namespace Core
{
	public partial class MovementPlayer : Player
	{
		ulong Buttons;
		ulong OldButtons;
		public ulong[] ValidMoveButtons =
		{
			(ulong)InputButton.Jump,
			(ulong)InputButton.Duck,
			(ulong)InputButton.Forward,
			(ulong)InputButton.Back,
			(ulong)InputButton.Left,
			(ulong)InputButton.Right,
			(ulong)InputButton.Run,
			(ulong)InputButton.Walk
		};

		private void ProcessMoveButtons()
		{
			ulong buttons = 0;

			for (int i = 0; i < ValidMoveButtons.Count(); i++)
			{
				if (Input.Down((InputButton)ValidMoveButtons[i]))
					buttons |= ValidMoveButtons[i] << 1;
			}
			
			if (Input.MouseWheel != 0)
				buttons |= ((ulong)InputButton.Jump) << 1;

			OldButtons = Buttons;
			Buttons = buttons;
		}

		public bool KeyDown(object button)
		{
			var buttons = Buttons;

			return (buttons &= ((ulong) button) << 1) != 0;
		}

		public bool KeyPressed(object button)
		{
			var buttons = Buttons;
			var old_buttons = OldButtons;

			if ((old_buttons != buttons) && Buttons > OldButtons)
			{
				buttons &= ((ulong) button) << 1;
				old_buttons &= ((ulong) button) << 1;

				return buttons != old_buttons && buttons != 0;
			}

			return false;
		}

		public bool KeyReleased(object button)
		{
			var buttons = Buttons;
			var old_buttons = OldButtons;

			if ((old_buttons != buttons) && OldButtons > Buttons)
			{
				buttons &= ((ulong) button) << 1;
				old_buttons &= ((ulong) button) << 1;

				return buttons != old_buttons && old_buttons != 0;
			}

			return false;
		}
	}
}
