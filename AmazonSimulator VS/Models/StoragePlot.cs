using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class StoragePlot : Entity
    {
        private float _length;
        private float _width;

        public float width { get { return _width; } }
        public float length { get { return _length; } }

        public StoragePlot(float width, float length, float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("storage", x, y, z, rotationX, rotationY, rotationZ)
        {
            this._length = length;
            this._width = width;
        }
    }
}