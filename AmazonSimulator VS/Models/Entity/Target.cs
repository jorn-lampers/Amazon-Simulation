using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Target : Entity
    {
        public Target(float x, float y, float z) : base("target", x, y, z, 0f, 0f, 0f)
        {
        }
    }
}