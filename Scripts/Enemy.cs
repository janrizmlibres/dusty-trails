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
            GetNode<Area2D>("Area2D").BodyExited += OnBodyExited;

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
            if (_collidedWithPlayer)
            {
                Vector2 pushDirection = _player.Position.DirectionTo(Position);
                // _direction = pushDirection * _player.Velocity.Length() * 0.5f * (float)delta;
                Velocity = pushDirection * _player.Velocity.Length() * 0.5f;
            }
            else
            {
                Velocity = Speed * _direction;
            }

            return MoveAndCollide(Velocity * (float)delta);
        }

        private void HandleCollisions(KinematicCollision2D collision, double delta)
        {
            if (collision == null)
            {
                if (_collidedWithPlayer)
                {
                    _direction = Vector2.Zero;
                    // _direction = Position.DirectionTo(_player.Position);
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
                HandlePlayerCollision();
            }
        }

        public void HandlePlayerCollision()
        {
            _newDirection = Position.DirectionTo(_player.Position);

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

        private void OnBodyExited(Node2D body)
        {
            if (body is Player)
            {
                _direction = Position.DirectionTo(_player.Position);
                _timer.Start(1);
            }
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
            // 25% chance of generating new direction for enemy
            else if (randomDirection < 0.3)
            {
                _direction = Vector2.Down.Rotated((float)(_rng.Randf() * 2 * Math.PI));
            }

            // 70% chance of maintaining current direction (idle or moving)
        }
    }
}