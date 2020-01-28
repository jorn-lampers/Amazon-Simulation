using System;
using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public interface ICargoCarrier 
        : IUpdatable
    {
        List<CargoSlot> CargoSlots { get; }
        List<CargoSlot> FreeCargoSlots { get; }
        List<CargoSlot> OccupiedCargoSlots { get; }
        bool HasFreeCargoSlots { get; }
        Vector3 Position { get; }

        bool TryAddCargo(Shelf item);

    }
}
