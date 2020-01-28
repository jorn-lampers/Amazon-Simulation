using System;
using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public interface ICargoCarrier
    {
        List<CargoSlot> CargoSlots { get; }
        List<CargoSlot> FreeCargoSlots { get; }
        List<CargoSlot> OccupiedCargoSlots { get; }
        bool HasFreeCargoSlots { get; }
        Vector3 Position { get; }

        void Destroy();
        bool TryAddCargo(Shelf item);

    }
}
