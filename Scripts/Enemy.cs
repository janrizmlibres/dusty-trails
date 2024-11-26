using Godot;
using System;
using System.Text;

namespace DustyTrails
{
    public partial class Enemy : CharacterBody2D
    {
        private Player _player;
        private Timer _timer;

        [Export] private int Speed = 40;
        [Export] private int ChaseDistance = 100;

        private bool _collidedWithPlayer = false;

        private Vector2 _direction = Vector2.Zero;
        private Vector2 _newDirection = new(0, 1);

        private RandomNumberGenerator _rng = new();

        public override void _Ready()
        {
            _player = GetTree().Root.GetNode<Player>("Main/Player");
            _timer = GetNode<Timer>("Timer");

            _timer.Timeout += OnTimerTimeout;
            _rng.Randomize();
        }

        public override void _PhysicsProcess(double delta)
        {
            KinematicCollision2D collision = HandleMovement(delta);
            HandleCollisions(collision, delta);
        }

        private KinematicCollision2D HandleMovement(double delta)
        {
            Velocity = Speed * _direction;
            return MoveAndCollide(Velocity * (float)delta);
        }

        private void HandleCollisions(KinematicCollision2D collision, double delta)
        {
            if (collision == null)
            {
                if (_collidedWithPlayer)
                {
                    _direction = Vector2.Zero;
                    _collidedWithPlayer = false;
                }

                return;
            }

            if (collision.GetCollider() is not Player)
            {
                HandleEnvironmentCollision();
            }
            else
            {
                HandlePlayerCollision(delta);
            }
        }

        public void HandlePlayerCollision(double delta)
        {
            _newDirection = Position.DirectionTo(_player.Position);

            Vector2 pushDirection = _player.Position.DirectionTo(Position);
            _direction = pushDirection * _player.Velocity.Length() * 0.5f * (float)delta;

            _timer.Start(1);
            _collidedWithPlayer = true;
        }

        private void HandleEnvironmentCollision()
        {
            if (_direction == Vector2.Zero)
            {
                _direction = Vector2.Down.Rotated((float)(_rng.Randf() * 2 * Math.PI));
            }
            else
            {
                float angle = _rng.RandfRange((float)(Math.PI / 4), (float)(Math.PI / 2));
                _direction = _direction.Rotated(angle);
            }

            _timer.Start(_rng.RandfRange(2, 5));
        }

        private void OnTimerTimeout()
        {
            _timer.Start(1);

            Vector2 distanceVector = _player.Position - Position;

            if (distanceVector.Length() <= ChaseDistance)
            {
                _direction = distanceVector.Normalized();
                return;
            }

            float randomDirection = _rng.Randf();

            // 5% chance of stopping enemy from moving
            if (randomDirection < 0.05)
            {
                _direction = Vector2.Zero;
            }
            // 5% chance of generating new direction for enemy
            else if (randomDirection < 0.1)
            {
                _direction = Vector2.Down.Rotated((float)(_rng.Randf() * 2 * Math.PI));
            }

            // 90% chance of maintaining current direction (idle or moving)
        }
    }
}