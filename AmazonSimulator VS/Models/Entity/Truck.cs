﻿using AmazonSimulator_VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models
{
    public class Truck : PathfindingEntity, ICargoCarrier, IOccupiable<Robot>
    {
        private List<CargoSlot> _cargoSlots;
        private IReleasable<Robot> _occupant;

        public Truck(EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ) 
            : base("truck", parent, x, y, z, rotationX, rotationY, rotationZ, Constants.TruckSpeed)
        {
            _cargoSlots = new List<CargoSlot>();

            for(int px = -3; px > -13; px--)
                for(int pz = -1; pz < 2; pz++)
                    _cargoSlots.Add(new CargoSlot(this, new Vector3(px, 0f, pz)));
        }

        public List<CargoSlot> CargoSlots 
            => _cargoSlots;

        public List<CargoSlot> FreeCargoSlots 
            => CargoSlots.Where((CargoSlot slot) => slot.IsAvailable).ToList();

        public List<CargoSlot> OccupiedCargoSlots 
            => CargoSlots.Where((CargoSlot slot) => !slot.IsAvailable).ToList();

        public bool HasFreeCargoSlots 
            => FreeCargoSlots.Count > 0;

        public Robot Occupant 
            => _occupant?.Owner;

        public bool IsOccupied 
            => _occupant != null && !_occupant.IsReleased();

        public IReleasable<Robot> Occupy(Robot occupant)
            => IsOccupied 
            ? throw new InvalidOperationException("Truck is already occupied!")
            : this._occupant = new DeOccupier<Robot>(occupant);

        public bool TryAddCargo(Shelf item)
            => this.HasFreeCargoSlots
            ? this.FreeCargoSlots[0].SetCargo(item)
            : false;

        public override bool Tick(int tick)
        {
            base.Tick(tick);
            foreach(CargoSlot slot in _cargoSlots) slot.Tick(tick);

            return _needsUpdate;
        }

        public override void Destroy()
        {
            base.Destroy();
            CargoSlots.ForEach(s => s.Destroy());
        }
    }
}