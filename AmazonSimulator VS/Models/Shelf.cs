using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class Shelf : Entity
    {
        public Shelf(float x = 0, float y = 0, float z = 0, float rotationX = 0, float rotationY = 0, float rotationZ = 0) : base("shelf", x, y, z, rotationX, rotationY, rotationZ)
        {
        }
    }
}
