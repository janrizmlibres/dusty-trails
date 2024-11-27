using DustyTrails.Scripts;
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
        private AmmoAmount _ammoAmount;
        private StaminaAmount _staminaAmount;
        private HealthAmount _healthAmount;

        // Player states
        [Export] private int BaseSpeed = 60;
        private bool _isAttacking = false;
        private bool _isSprinting = false;

        // Custom signals
        [Signal]
        public delegate void HealthUpdatedEventHandler(int health, int maxHealth);
        [Signal]
        public delegate void StaminaUpdatedEventHandler(int stamina, int maxStamina);
        [Signal]
        public delegate void AmmoPickupsUpdatedEventHandler(int ammoPickup);
        [Signal]
        public delegate void HealthPickupsUpdatedEventHandler(int healthPickup);
        [Signal]
        public delegate void StaminaPickupsUpdatedEventHandler(int staminaPickup);

        public int AmmoPickup = 3;
        public int HealthPickup = 1;
        public int StaminaPickup = 1;

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
            _ammoAmount = GetNode<AmmoAmount>("UI/AmmoAmount");
            _staminaAmount = GetNode<StaminaAmount>("UI/StaminaAmount");
            _healthAmount = GetNode<HealthAmount>("UI/HealthAmount");

            _animationSprite.AnimationFinished += OnAnimationSprite2dAnimationFinished;

            HealthUpdated += _healthBar.UpdateHealthUI;
            StaminaUpdated += _staminaBar.UpdateStaminaUI;
            AmmoPickupsUpdated += _ammoAmount.UpdateAmmoPickupUI;
            HealthPickupsUpdated += _healthAmount.UpdateHealthPickupUI;
            StaminaPickupsUpdated += _staminaAmount.UpdateStaminaPickupUI;
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

                for (int i = 0; i < GetSlideCollisionCount(); i++)
                {
                    KinematicCollision2D collision = GetSlideCollision(i);
                    if (collision.GetCollider() is Enemy enemy)
                    {
                        enemy.HandlePlayerCollision();
                    }
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            // Input event for our attacking, i.e. our shooting
            if (@event.IsActionPressed("ui_attack") && AmmoPickup > 0 && !_isAttacking)
            {
                _isAttacking = true;
                string animation = "attack_" + ReturnedDirection(_savedDirection);
                _animationSprite.Play(animation);

                AmmoPickup -= 1;
                EmitSignal(SignalName.AmmoPickupsUpdated, AmmoPickup);
            }
            // Using health consumables
            else if (@event.IsActionPressed("ui_consume_health") && _health > 0 && HealthPickup > 0)
            {
                HealthPickup -= 1;
                _health = Math.Min(_health + 50, _maxHealth);
                EmitSignal(SignalName.HealthUpdated, _health, _maxHealth);
                EmitSignal(SignalName.HealthPickupsUpdated, HealthPickup);
            }
            // Using stamina consumables
            else if (@event.IsActionPressed("ui_consume_stamina") && _stamina > 0 && StaminaPickup > 0)
            {
                StaminaPickup -= 1;
                _stamina = Math.Min(_stamina + 50, _maxStamina);
                EmitSignal(SignalName.StaminaUpdated, _stamina, _maxStamina);
                EmitSignal(SignalName.StaminaPickupsUpdated, StaminaPickup);
            }
        }

        private string ReturnedDirection(Vector2 direction)
        {
            Vector2 normalizedDirection = direction.Normalized();

            // Handle vertical movement first
            if (Math.Abs(normalizedDirection.Y) > Math.Abs(normalizedDirection.X))
            {
                return normalizedDirection.Y > 0 ? "down" : "up";
            }

            // Handle horizontal movement
            _animationSprite.FlipH = normalizedDirection.X < 0;
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

        public void AddPickup(Global.PickupType item)
        {
            if (item == Global.PickupType.Ammo)
            {
                AmmoPickup += 3;
                EmitSignal(SignalName.AmmoPickupsUpdated, AmmoPickup);
            }

            if (item == Global.PickupType.Health)
            {
                HealthPickup += 1;
                EmitSignal(SignalName.HealthPickupsUpdated, HealthPickup);
            }

            if (item == Global.PickupType.Stamina)
            {
                StaminaPickup += 1;
                EmitSignal(SignalName.StaminaPickupsUpdated, StaminaPickup);
            }
        }

        private void OnAnimationSprite2dAnimationFinished()
        {
            _isAttacking = false;
        }
    }
}