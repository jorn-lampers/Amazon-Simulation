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

        public string type { get; }
        public Guid guid { get; }

        public float x { get { return _x; } }
        public float y { get { return _y; } }
        public float z { get { return _z; } }
        public Vector3 position { get { return new Vector3(x, y, z); } }

        public float rotationX { get { return _rX; } }
        public float rotationY { get { return _rY; } }
        public float rotationZ { get { return _rZ; } }
        public Vector3 rotation { get { return new Vector3(_rX, _rY, _rZ);  } }

        public bool needsUpdate = true;

        public Entity(string type, float x = 0.0f, float y = 0f, float z = 0f, float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f)
        {
            this.type = type;
            this.guid = Guid.NewGuid();

            this._x = x;
            this._y = y;
            this._z = z;

            this._rX = rotationX;
            this._rY = rotationY;
            this._rZ = rotationZ;
        }

        public virtual void Move(float x, float y, float z)
        {
            this._x = x;
            this._y = y;
            this._z = z;

            needsUpdate = true;
        }

        public virtual void Move(Vector3 pos)
        {
            Move(pos.X, pos.Y, pos.Z);
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
            return needsUpdate;
        }
    }
}