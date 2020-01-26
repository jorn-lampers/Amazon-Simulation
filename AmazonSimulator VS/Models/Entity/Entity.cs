using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models
{

    public abstract class Entity : IUpdatable
    {
        private float _x = 0;
        private float _y = 0;
        private float _z = 0;

        private float _rX = 0;
        private float _rY = 0;
        private float _rZ = 0;

        protected EntityEnvironmentInfoProvider _environment;

        private bool _discard;

        public bool needsUpdate;
        public string Type { get; }
        public Guid Guid { get; }
        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Z { get { return _z; } }
        public Vector3 Position { get { return new Vector3(X, Y, Z); } }

        public float RotationX { get { return _rX; } }
        public float RotationY { get { return _rY; } }
        public float RotationZ { get { return _rZ; } }
        public Vector3 Rotation { get { return new Vector3(_rX, _rY, _rZ);  } }

        public Entity(string type, EntityEnvironmentInfoProvider parent, float x = 0.0f, float y = 0f, float z = 0f, float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f)
        {
            this.Type = type;
            this.Guid = Guid.NewGuid();
            this._environment = parent;

            this._x = x;
            this._y = y;
            this._z = z;

            this._rX = rotationX;
            this._rY = rotationY;
            this._rZ = rotationZ;

            this._discard = false;
        }

        public void Destroy()
        {
            this._discard = true;
        }

        public bool DiscardRequested()
        {
            return this._discard;
        }

        public virtual Vector3 Move(float x, float y, float z)
        {
            this._x = x;
            this._y = y;
            this._z = z;

            this.needsUpdate = true;

            return Position;
        }

        public virtual Vector3 Move(Vector3 pos)
        {
            return Move(pos.X, pos.Y, pos.Z);
        }

        public virtual void Rotate(float rotationX, float rotationY, float rotationZ)
        {
            this._rX = rotationX;
            this._rY = rotationY;
            this._rZ = rotationZ;

            needsUpdate = true;
        }

        public virtual bool Tick(int tick)
        {
            return this.needsUpdate;
        }

        public bool NeedsUpdate()
        {
            return this.needsUpdate;
        }
    }
}