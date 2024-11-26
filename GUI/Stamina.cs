using Godot;
using System;

namespace DustyTrails
{
    public partial class Stamina : ColorRect
    {
        // Node refs
        private ColorRect _value;

        public override void _Ready()
        {
            _value = GetNode<ColorRect>("Value");
        }

        // Updated UI
        public void UpdateStaminaUI(int stamina, int maxStamina)
        {
            Vector2 newSize = _value.Size;
            newSize.X = 98 * stamina / maxStamina;
            _value.Size = newSize;
        }
    }
}