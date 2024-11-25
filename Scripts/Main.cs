using DustyTrails.Scripts;
using Godot;
using System;

namespace DustyTrails
{
	public partial class Main : Node2D
	{
		private Node2D _spawnedPickups;

		private TileMapLayer _waterLayer;
		private TileMapLayer _grassLayer;
		private TileMapLayer _foliageLayer;
		private TileMapLayer _exterior1Layer;
		private TileMapLayer _exterior2Layer;

		private RandomNumberGenerator _rng = new();

		public override void _Ready()
		{
			_spawnedPickups = GetNode<Node2D>("SpawnedPickups");

			_waterLayer = GetNode<TileMapLayer>("Map/Water");
			_grassLayer = GetNode<TileMapLayer>("Map/Grass");
			_foliageLayer = GetNode<TileMapLayer>("Map/Foliage");
			_exterior1Layer = GetNode<TileMapLayer>("Map/Exterior1");
			_exterior2Layer = GetNode<TileMapLayer>("Map/Exterior2");

			int spawnPickupAmount = (int)_rng.RandfRange(5, 10);
			SpawnPickups(spawnPickupAmount);
		}

		private bool IsValidSpawnLocation(Vector2I position)
		{
			if (_waterLayer.GetCellSourceId(position) != -1) return false;
			if (_foliageLayer.GetCellSourceId(position) != -1) return false;
			if (_exterior1Layer.GetCellSourceId(position) != -1) return false;
			if (_exterior2Layer.GetCellSourceId(position) != -1) return false;
			return true;
		}

		// Spawn pickup
		private void SpawnPickups(int amount)
		{
			int spawned = 0;
			// int attempts = 0;
			// int maxAttempts = 1000;

			while (spawned < amount)
			{
				// attempts++;
				// Randomly choose a location on the grass layer
				Vector2I randomPosition = new()
				{
					X = (int)(GD.Randi() % _grassLayer.GetUsedRect().Size.X),
					Y = (int)(GD.Randi() % _grassLayer.GetUsedRect().Size.Y)
				};

				// Spawn it underneath SpawnedPickups node
				if (IsValidSpawnLocation(randomPosition))
				{
					Pickup pickupInstance = Global.PickupsScene.Instantiate<Pickup>();
					// Randomly select a pickup type
					pickupInstance.Item = (Global.Pickups)_rng.RandiRange(0, 2);
					// Add pickup to scene
					pickupInstance.Position = _grassLayer.MapToLocal(randomPosition);
					_spawnedPickups.AddChild(pickupInstance);
					spawned++;
				}
			}
		}
	}
}