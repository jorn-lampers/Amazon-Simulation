using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Scaffold : Entity
    {
        public Scaffold(float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("scaffold", x, y, z, rotationX, rotationY, rotationZ)
        {

        }
    }
}