using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models
{
    public class Truck : PathfindingEntity, ICargoCarrier, IOccupiable<Robot>
    {
        List<CargoSlot> cargoSlots;
        IReleasable<Robot> _occupant;

        public Truck(EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("truck", parent, x, y, z, rotationX, rotationY, rotationZ)
        {
            cargoSlots = new List<CargoSlot>();

            for(int i = 0; i < 36; i++)
                cargoSlots.Add(new CargoSlot(this, new Vector3(0f, 0f, 0f)));

        }

        public List<CargoSlot> CargoSlots => cargoSlots;

        public List<CargoSlot> FreeCargoSlots => CargoSlots.Where((CargoSlot slot) => slot.IsAvailable).ToList();

        public List<CargoSlot> OccupiedCargoSlots => CargoSlots.Where((CargoSlot slot) => !slot.IsAvailable).ToList();

        public bool HasFreeCargoSlots => FreeCargoSlots.Count > 0;

        public Robot Occupant => _occupant?.Owner;

        public bool IsOccupied => _occupant != null && !_occupant.IsReleased();

        public IReleasable<Robot> Occupy(Robot occupant)
        {
            if (IsOccupied) throw new InvalidOperationException("Truck is already occupied!");

            this._occupant = new DeOccupier<Robot>(occupant);
            return _occupant;
        }

        public int Test(int i)
        {
            return i;
        }

        public override bool Tick(int tick)
        {
            base.Tick(tick);
            foreach(CargoSlot slot in cargoSlots) slot.Tick(tick);

            return needsUpdate;
        }

        public bool TryAddCargo(Shelf item)
        {
            if (this.HasFreeCargoSlots)
                return this.FreeCargoSlots[0].SetCargo(item);
            else return false;
        }
    }
}