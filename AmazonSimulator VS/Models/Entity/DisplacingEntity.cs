﻿using AmazonSimulator_VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Models
{
    public abstract class DisplacingEntity : Entity
    {

        private float _acceleration;
        private float _maxMovementSpeed;
        protected float _velocity = 0;

        private float _rotationSpeed;

        protected Vector3 _target;

        public float MovementSpeed => _maxMovementSpeed; // In units per second.
        public float RotationSpeed => _rotationSpeed;
        public float Velocity => _velocity;


        public DisplacingEntity(string type, EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ, float movementPerSecond, float rotationPerSecond, float accelerationPerSecond)
            : base(type, parent, x, y, z, rotationX, rotationY, rotationZ)
        {
            this._maxMovementSpeed = movementPerSecond / Constants.SIM_TPS;
            this._rotationSpeed = rotationPerSecond / Constants.SIM_TPS;
            this._acceleration = accelerationPerSecond / Constants.SIM_TPS;
            this._target = Position;
        }


        public void SetTarget(Vector3 target)
        {
            this._target = target;
        }

        public bool IsAtTarget => this._target == Position;

        public override bool Tick(int tick)
        {
            if (!_target.Equals(this.Position))
            {
                var tDir = Vector3.Normalize(_target - this.Position);
                var cDir = Forward;

                var cross2 = Vector3.Cross(tDir, cDir);

                float pi = (float)Math.PI * 0.5f;
                float deltaRY = Math.Min(Math.Max(cross2.Y * pi, -RotationSpeed), RotationSpeed);

                this.Rotate(this.RotationX, this.RotationY + deltaRY, this.RotationZ);

                if (cross2.Length() > 0.001)
                {
                    _velocity = 0f;
                    return _needsUpdate;
                }



                // Accelerate velocity by acceleration if top speed has not been reached
                this._velocity += Math.Min(this._acceleration, this._maxMovementSpeed - this._velocity);

                float distanceToTarget = (_target - Position).Length();

                if (this._velocity > distanceToTarget) Move(_target);
                else
                {   // Move entity over the vector spanned between target position and current position with length == distance
                    Vector3 direction = Vector3.Normalize(_target - this.Position);
                    Move(this.Position + direction * this._velocity);
                }
            }

            return _needsUpdate;
        }
    }
}