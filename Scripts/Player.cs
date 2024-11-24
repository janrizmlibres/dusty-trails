using Godot;
using System;

namespace DustyTrails
{
	public partial class Player : CharacterBody2D
	{
		// Node references
		private AnimatedSprite2D _animationSprite;
		private Health _healthBar;
		private Stamina _staminaBar;

		// Player states
		[Export] private int BaseSpeed = 60;
		private bool _isAttacking = false;
		private bool _isSprinting = false;

		// Custom signals
		[Signal]
		public delegate void HealthUpdatedEventHandler(int health, int maxHealth);
		[Signal]
		public delegate void StaminaUpdatedEventHandler(int stamina, int maxStamina);

		// UI variables
		private double _health = 100;
		private double _maxHealth = 100;
		private double _regenHealth = 1;
		private double _stamina = 100;
		private double _maxStamina = 100;
		private double _regenStamina = 5;

		// Direction and animation to be updated throughout game state
		private Vector2 _savedDirection = new(0, 1); // Only move one spaces

		public override void _Ready()
		{
			_animationSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			_healthBar = GetNode<Health>("UI/HealthBar");
			_staminaBar = GetNode<Stamina>("UI/StaminaBar");

			_animationSprite.AnimationFinished += OnAnimationSprite2dAnimationFinished;
			HealthUpdated += _healthBar.UpdateHealthUI;
			StaminaUpdated += _staminaBar.UpdateStaminaUI;
		}

		public override void _Process(double delta)
		{
			double updatedHealth = Math.Min(_health + _regenHealth * delta, _maxHealth);
			if (updatedHealth != _health)
			{
				_health = updatedHealth;
				EmitSignal(SignalName.HealthUpdated, _health, _maxHealth);
			}

			if (!_isSprinting)
			{
				double updatedStamina = Math.Min(_stamina + _regenStamina * delta, _maxStamina);
				if (updatedStamina != _stamina)
				{
					_stamina = updatedStamina;
					EmitSignal(SignalName.StaminaUpdated, _stamina, _maxStamina);
				}
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			// Get player input (left, right, up/down)
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			// Normalize movement
			direction = direction.Normalized();

			int speed = BaseSpeed;

			// Sprinting
			if (Input.IsActionPressed("ui_sprint"))
			{
				_isSprinting = true;

				if (_stamina >= 25)
				{
					speed = BaseSpeed + 50;
					_stamina -= 20.0 * delta;
					EmitSignal(SignalName.StaminaUpdated, _stamina, _maxStamina);
				}
			}
			else
			{
				_isSprinting = false;
			}

			// Apply movement if the player is not attacking
			Velocity = speed * direction;
			if (!_isAttacking)
			{
				PlayerAnimations(direction);
				MoveAndSlide();
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