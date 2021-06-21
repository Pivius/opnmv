using System.Linq;
using Sandbox;
using OMMovement;
using System;
using System.Collections.Generic;

namespace Core
{
	public partial class MovementPlayer : Player
	{
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
		private void ScaleSensitivity(ref Angles view_angles, Vector2 previous_delta, Vector2 mouse_delta)
		{
			MouseInput.MouseMove(ref view_angles, ref previous_delta, mouse_delta);
			PreviousDelta = previous_delta;
		}

		[Event.BuildInput] private void ProcessSensitivty(InputBuilder input)
		{
			ScaleSensitivity(ref input.ViewAngles, PreviousDelta, new Vector2(Input.MouseDelta.x, Input.MouseDelta.y));
		}

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

		public bool KeyDown(InputButton button)
		{
			var buttons = Buttons;

			return (buttons &= ((ulong) button) << 1) != 0;
		}

		public bool KeyPressed(InputButton button)
		{
			var buttons = Buttons;
			var old_buttons = OldButtons;

			if ((old_buttons != buttons))
			{
				buttons &= ((ulong) button) << 1;
				old_buttons &= ((ulong) button) << 1;

				return buttons != old_buttons && buttons != 0;
			}

			return false;
		}

		public bool KeyReleased(InputButton button)
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
