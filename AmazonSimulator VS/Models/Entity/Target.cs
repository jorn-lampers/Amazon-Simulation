using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Target : Entity
    {
        public Target(EntityEnvironmentInfoProvider parent, float x, float y, float z) : base("target", parent, x, y, z, 0f, 0f, 0f)
        {
        }
    }
}