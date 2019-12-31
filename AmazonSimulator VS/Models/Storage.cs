using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Storage : Entity
    {
        public Storage(float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("storage", x, y, z, rotationX, rotationY, rotationZ)
        {

        }
    }
}