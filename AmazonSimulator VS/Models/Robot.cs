using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models {
    public class Robot : Entity
    {
        public Robot(double x, double y, double z, double rotationX, double rotationY, double rotationZ) : base("robot", x, y, z, rotationX, rotationY, rotationZ)
        {

        }
    }
}