using Godot;
using System;

namespace DustyTrails.Scripts
{
    public partial class Global : Node
    {
        public static readonly PackedScene PickupsScene = GD.Load<PackedScene>("res://Scenes/pickup.tscn");

        public enum Pickups { Ammo, Stamina, Health }
    }
}