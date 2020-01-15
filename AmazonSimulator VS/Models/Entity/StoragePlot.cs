using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models
{
    public class StoragePlot : Entity
    {
        private float _length;
        private float _width;

        public float width { get { return _width; } }
        public float length { get { return _length; } }

        public StoragePlot(int width, int length, float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("storage", x, y, z, rotationX, rotationY, rotationZ)
        {
            this._length = length;
            this._width = width;
        }

        public List<Vector3> StoragePositionsInWorld {
            get {
                List<Vector3> positions = new List<Vector3>();

                float startX = -_width / 2 + 0.5f;
                float startZ = -_length / 2 + 0.5f;

                for(int x = 0; x < _width; x++)
                {
                    for (int z = 0; z < _length; z++)
                        positions.Add(new Vector3(this.x + startX + x, this.y, this.z + startZ + z));
                }

                return positions;
            }
        }
    }
}