using Godot;
using System;

using Pickups = DustyTrails.Scripts.Global.Pickups;

namespace DustyTrails
{
	[Tool]
	public partial class Pickup : Area2D
	{
		// Node refs
		private Sprite2D _sprite;

		[Export] public Pickups Item { get; set; }

		private Texture2D _ammoTexture = GD.Load<Texture2D>("res://Assets/Icons/shard_01i.png");
		private Texture2D _staminaTexture = GD.Load<Texture2D>("res://Assets/Icons/potion_02b.png");
		private Texture2D _healthTexture = GD.Load<Texture2D>("res://Assets/Icons/potion_02c.png");

		public override void _Ready()
		{
			_sprite = GetNode<Sprite2D>("Sprite2D");
			BodyEntered += OnBodyEntered;

			if (!Engine.IsEditorHint())
				_sprite.Texture = GetTexture();
		}

		public override void _Process(double delta)
		{
			if (Engine.IsEditorHint())
				_sprite.Texture = GetTexture();
		}

		private Texture2D GetTexture()
		{
			if (Item == Pickups.Ammo) return _ammoTexture;
			if (Item == Pickups.Stamina) return _staminaTexture;
			if (Item == Pickups.Health) return _healthTexture;
			throw new InvalidOperationException("Invalid pickup item type.");
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body.Name == "Player")
			{
				((Player)body).AddPickup(Item);
				QueueFree();
			}
		}
	}
}