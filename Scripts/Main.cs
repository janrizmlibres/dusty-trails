using DustyTrails.Scripts;
using Godot;
using System;

using static DustyTrails.Scripts.Global.LayerType;

namespace DustyTrails
{
    public partial class Main : Node2D
    {
        private Node2D _spawnedPickups;

        private RandomNumberGenerator _rng = new();

        private readonly EnumArray<TileMapLayer> _mapLayers = Global.Instance.MapLayers;

        public override void _Ready()
        {
            _spawnedPickups = GetNode<Node2D>("SpawnedPickups");

            _mapLayers[Water] = GetNode<TileMapLayer>("Map/Water");
            _mapLayers[Grass] = GetNode<TileMapLayer>("Map/Grass");
            _mapLayers[Foliage] = GetNode<TileMapLayer>("Map/Foliage");
            _mapLayers[Exterior1] = GetNode<TileMapLayer>("Map/Exterior1");
            _mapLayers[Exterior2] = GetNode<TileMapLayer>("Map/Exterior2");

            int spawnPickupAmount = (int)_rng.RandfRange(5, 10);
            SpawnPickups(spawnPickupAmount);
        }

        private bool IsValidSpawnLocation(Vector2I position)
        {
            if (_mapLayers[Water].GetCellSourceId(position) != -1) return false;
            if (_mapLayers[Foliage].GetCellSourceId(position) != -1) return false;
            if (_mapLayers[Exterior1].GetCellSourceId(position) != -1) return false;
            if (_mapLayers[Exterior2].GetCellSourceId(position) != -1) return false;
            return true;
        }

        // Spawn pickup
        private void SpawnPickups(int amount)
        {
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = 1000;

            PackedScene pickupScene = Global.Instance.Scenes[Global.SceneType.Pickup];

            while (spawned < amount && attempts < maxAttempts)
            {
                attempts++;
                // Randomly choose a location on the grass layer
                Vector2I randomPosition = new()
                {
                    X = (int)(GD.Randi() % _mapLayers[Grass].GetUsedRect().Size.X),
                    Y = (int)(GD.Randi() % _mapLayers[Grass].GetUsedRect().Size.Y)
                };

                // Spawn it underneath SpawnedPickups node
                if (IsValidSpawnLocation(randomPosition))
                {
                    Pickup pickupInstance = pickupScene.Instantiate<Pickup>();
                    // Randomly select a pickup type
                    pickupInstance.Item = (Global.PickupType)_rng.RandiRange(0, 2);
                    // Add pickup to scene
                    pickupInstance.Position = _mapLayers[Grass].MapToLocal(randomPosition);
                    _spawnedPickups.AddChild(pickupInstance);
                    spawned++;
                }
            }
        }
    }
}