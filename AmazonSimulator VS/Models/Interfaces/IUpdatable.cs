using System;
using System.Collections.Generic;
using System.Linq;

namespace Models {
    public interface IUpdatable
    {
        bool Tick(int tick);
        bool NeedsUpdate(bool evaluateOnly = false);
        void Destroy();
    }
}
