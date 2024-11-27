using DustyTrails.Scripts;
using Godot;
using System;

namespace DustyTrails
{
	public partial class EnemySpawner : Node2D
	{
		private Node2D _spawnedEnemies;
		private Node2D _tilemap;

		[Export] private int MaxEnemies = 20;
		private int _enemyCount = 0;
		RandomNumberGenerator _rng = new();

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_spawnedEnemies = GetNode<Node2D>("SpawnedEnemies");
			_tilemap = GetTree().Root.GetNode<TileMapLayer>("Main/Map");
		}

		private void SpawnEnemy()
		{
			Enemy enemy = Global.Instance.Scenes[Global.SceneType.Enemy].Instantiate<Enemy>();
			_spawnedEnemies.AddChild(enemy);
		}
	}
}