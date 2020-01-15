using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Robot : PathfindingEntity
    {
        private Shelf _shelf;

        public void attachShelf(Shelf s)
        {
            this._shelf = s;
        }

        public Robot(float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("robot", x, y, z, rotationX, rotationY, rotationZ)
        {

        }

        public override bool Tick(int tick)
        {
            base.Tick(tick);
            if (_shelf != null) _shelf.Move(this.position + new System.Numerics.Vector3(0.0f, 0.35f, 0.0f));

            return needsUpdate;
        }

        internal Shelf getCurrentShelf()
        {
            return _shelf;
        }

        internal void detachShelf()
        {
            this._shelf = null;
        }
    }
}