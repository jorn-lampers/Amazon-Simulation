using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Scaffold : Entity
    {
        public Scaffold(double x, double y, double z, double rotationX, double rotationY, double rotationZ) : base("scaffold", x, y, z, rotationX, rotationY, rotationZ)
        {

        }
    }
}