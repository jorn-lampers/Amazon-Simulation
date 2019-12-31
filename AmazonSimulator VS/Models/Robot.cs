using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Robot : PathfindingEntity
    {
        public Robot(float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("robot", x, y, z, rotationX, rotationY, rotationZ)
        {

        }
    }
}