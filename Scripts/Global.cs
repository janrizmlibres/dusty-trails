using Godot;
using System;

namespace DustyTrails.Scripts
{
    public partial class Global : Node
    {
        public static Global Instance { get; private set; }

        public enum LayerType { Water, Grass, Foliage, Exterior1, Exterior2 }
        public enum SceneType { Pickup, Enemy }

        public enum PickupType { Ammo, Stamina, Health }

        public EnumArray<TileMapLayer> MapLayers { get; private set; }
        public EnumArray<PackedScene> Scenes { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            MapLayers = new(Enum.GetValues<LayerType>().Length);
            Scenes = new(Enum.GetValues<SceneType>().Length);

            InitializeScenes();
        }

        private void InitializeScenes()
        {
            Scenes[SceneType.Pickup] = GD.Load<PackedScene>("res://Scenes/pickup.tscn");
            Scenes[SceneType.Enemy] = GD.Load<PackedScene>("res://Scenes/enemy.tscn");
        }
    }

    public class EnumArray<TValue>
    {
        private readonly TValue[] _items;

        public EnumArray(int size)
        {
            _items = new TValue[size];
        }

        public TValue this[Enum key]
        {
            get => _items[Convert.ToInt32(key)];
            set => _items[Convert.ToInt32(key)] = value;
        }
    }
}