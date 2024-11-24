using Godot;
using System;

namespace DustyTrails
{
	public partial class Player : CharacterBody2D
	{
		// Node references
		private AnimatedSprite2D _animationSprite;

		// Player states
		[Export] private int Speed = 50;
		private bool _isAttacking = false;

		// Direction and animation to be updated throughout game state
		private Vector2 _savedDirection = new(0, 1); // Only move one spaces

		public override void _Ready()
		{
			_animationSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			_animationSprite.AnimationFinished += OnAnimationSprite2dAnimationFinished;
		}

		public override void _PhysicsProcess(double delta)
		{
			// Get player input (left, right, up/down)
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			// Normalize movement
			direction = direction.Normalized();

			// Sprinting
			if (Input.IsActionPressed("ui_sprint"))
			{
				Speed = 100;
			}
			else if (Input.IsActionJustReleased("ui_sprint"))
			{
				Speed = 50;
			}

			// Apply movement if the player is not attacking
			Vector2 movement = Speed * direction * (float)delta;
			if (!_isAttacking)
			{
				PlayerAnimations(direction);
				MoveAndCollide(movement);
			}
		}

		public override void _Input(InputEvent @event)
		{
			// Input event for our attacking, i.e. our shooting
			if (@event.IsActionPressed("ui_attack"))
			{
				_isAttacking = true;
				string animation = "attack_" + ReturnedDirection(_savedDirection);
				_animationSprite.Play(animation);
			}
		}

		private string ReturnedDirection(Vector2 direction)
		{
			Vector2 normalizedDirection = direction.Normalized();

			if (normalizedDirection.Y > 0) return "down";

			if (normalizedDirection.Y < 0) return "up";

			if (normalizedDirection.X > 0)
			{
				_animationSprite.FlipH = false;
			}
			else if (normalizedDirection.X < 0)
			{
				_animationSprite.FlipH = true;
			}

			return "side";
		}

		private void PlayerAnimations(Vector2 direction)
		{
			if (direction != Vector2.Zero)
			{
				// Only update the direction if we are moving
				_savedDirection = direction;
				string animation = "walk_" + ReturnedDirection(_savedDirection);
				_animationSprite.Play(animation);
			}
			else
			{
				string animation = "idle_" + ReturnedDirection(_savedDirection);
				_animationSprite.Play(animation);
			}
		}

		private void OnAnimationSprite2dAnimationFinished()
		{
			_isAttacking = false;
		}
	}
}