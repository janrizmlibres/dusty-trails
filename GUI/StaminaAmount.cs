using Godot;
using System;

namespace DustyTrails
{
	public partial class StaminaAmount : ColorRect
	{
		private Label _value;
		private Player _player;

		public override void _Ready()
		{
			_value = GetNode<Label>("Value");
			_player = GetParent().GetParent<Player>();
			// _player = GetTree().Root.FindChild("Player", true, false) as Player;

			_value.Text = _player.StaminaPickup.ToString();
		}

		public void UpdateStaminaPickupUI(int staminaPickup)
		{
			_value.Text = staminaPickup.ToString();
		}
	}
}