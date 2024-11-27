using Godot;
using System;
using System.Text;

namespace DustyTrails
{
    public partial class Enemy : CharacterBody2D
    {
        private Player _player;
        private Timer _timer;
        private AnimatedSprite2D _animationSprite;

        [Export] private int Speed = 40;
        [Export] private int ChaseDistance = 100;
        [Export] private int PushForce = 10;

        private bool _collidedWithPlayer = false;
        private bool _collidedWithEnvironment = false;
        private bool _playerInCollisionZone = false;
        private bool _isAttacking = false;

        private Vector2 _direction = Vector2.Zero;
        private Vector2 _savedDirection = new(0, 1);
        private Vector2 _pushDirection = Vector2.Zero;

        private RandomNumberGenerator _rng = new();

        public override void _Ready()
        {
            _player = GetTree().Root.GetNode<Player>("Main/Player");
            _timer = GetNode<Timer>("Timer");
            _animationSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            GetNode<Area2D>("Area2D").BodyExited += OnBodyExited;
            _timer.Timeout += OnTimerTimeout;

            _rng.Randomize();
        }

        public override void _PhysicsProcess(double delta)
        {
            KinematicCollision2D collision = HandleMovement(delta);
            HandleCollisions(collision);

            if (!_isAttacking)
            {
                EnemyAnimations(_direction);
            }
        }

        private KinematicCollision2D HandleMovement(double delta)
        {
            if (_collidedWithEnvironment)
            {
                Velocity = _pushDirection * PushForce * (float)delta;
            }
            else if (_collidedWithPlayer)
            {
                Velocity = _pushDirection * _player.Velocity.Length() * 0.5f;
            }
            else
            {
                Velocity = Speed * _direction;
            }

            return MoveAndCollide(Velocity * (float)delta);
        }

        private void HandleCollisions(KinematicCollision2D collision)
        {
            if (collision == null)
            {
                if (_collidedWithPlayer)
                {
                    _direction = Vector2.Zero;
                    _collidedWithPlayer = false;
                }

                _collidedWithEnvironment = false;
                return;
            }

            if (collision.GetCollider() is not Player)
            {
                HandleEnvironmentCollision(collision.GetNormal());
            }
            else
            {
                HandlePlayerCollision();
            }
        }

        public void HandlePlayerCollision()
        {
            _savedDirection = Position.DirectionTo(_player.Position);

            _pushDirection = _player.Position.DirectionTo(Position);

            _timer.Stop();
            _collidedWithPlayer = true;
        }

        private void HandleEnvironmentCollision(Vector2 normal)
        {
            _pushDirection = normal;

            if (!_playerInCollisionZone)
            {
                float angle = _rng.RandfRange((float)(Math.PI / 4), (float)(Math.PI / 2));
                _direction = _direction.Rotated(angle);
            }

            _timer.Start(_rng.RandfRange(2, 5));
            _collidedWithEnvironment = true;
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

        private void EnemyAnimations(Vector2 direction)
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

        private void OnBodyEntered(Node2D body)
        {
            if (body is Player)
            {
                _playerInCollisionZone = true;
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is Player)
            {
                _direction = Position.DirectionTo(_player.Position);
                _playerInCollisionZone = false;
                _timer.Start(1);
            }
        }

        private void SyncNewDirection()
        {
            if (_direction != Vector2.Zero)
            {
                _savedDirection = _direction.Normalized();
            }
        }

        private void OnTimerTimeout()
        {
            _timer.Start(1);

            Vector2 distanceVector = _player.Position - Position;

            if (distanceVector.Length() <= ChaseDistance)
            {
                _direction = distanceVector.Normalized();
                SyncNewDirection();
                return;
            }

            float randomDirection = _rng.Randf();

            // 5% chance of stopping enemy from moving
            if (randomDirection < 0.05)
            {
                _direction = Vector2.Zero;
            }
            // 25% chance of generating new direction for enemy
            else if (randomDirection < 0.3)
            {
                _direction = Vector2.Down.Rotated((float)(_rng.Randf() * 2 * Math.PI));
            }

            // 70% chance of maintaining current direction (idle or moving)
            SyncNewDirection();
        }
    }
}