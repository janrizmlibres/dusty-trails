using Godot;
using System;

namespace DustyTrails
{
    public partial class Health : ColorRect
    {
        // Node refs
        private ColorRect _value;

        public override void _Ready()
        {
            _value = GetNode<ColorRect>("Value");
        }

        public void UpdateHealthUI(int health, int maxHealth)
        {
            Vector2 newSize = _value.Size;
            newSize.X = 98 * health / maxHealth;
            _value.Size = newSize;
        }
    }
}