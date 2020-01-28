using System;
using System.Collections.Generic;
using System.Linq;

namespace Models {
    interface IUpdatable
    {
        bool Tick(int tick);
        bool NeedsUpdate();

        void Destroy();
    }
}