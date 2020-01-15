using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Truck : PathfindingEntity
    {
        public Truck(float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("truck", x, y, z, rotationX, rotationY, rotationZ)
        {

        }
    }
}