using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models
{
    public class StoragePlot : Entity, ICargoCarrier
    {
        private float _length;
        private float _width;
        private List<CargoSlot> _cargoSlots;

        public float Width { get { return _width; } }
        public float Length { get { return _length; } }

        public StoragePlot(EntityEnvironmentInfoProvider parent, int width, int length, float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("storage", parent, x, y, z, rotationX, rotationY, rotationZ)
        {
            this._length = length;
            this._width = width;
            this._cargoSlots = new List<CargoSlot>();

            float startX = -_width / 2 + 0.5f;
            float startZ = -_length / 2 + 0.5f;

            for (int lx = 0; lx < _width; lx++)
                for (int lz = 0; lz < _length; lz++)
                    this._cargoSlots.Add(
                        new CargoSlot(this, new Vector3(startX + lx, 0, startZ + lz))
                    );
            

        }

        public List<CargoSlot> CargoSlots => _cargoSlots;

        public List<CargoSlot> FreeCargoSlots => CargoSlots.Where((CargoSlot slot) => slot.IsAvailable).ToList();

        public List<CargoSlot> OccupiedCargoSlots => CargoSlots.Where((CargoSlot slot) => slot.IsOccupied).ToList();

        public bool HasFreeCargoSlots => FreeCargoSlots.Count > 0;

        public bool TryAddCargo(Shelf item)
        {
            if(this.HasFreeCargoSlots)
            {
                this.FreeCargoSlots[0].SetCargo(item);
                return true;
            }
            return false;
        }

        public override bool Tick(int tick)
        {
            base.Tick(tick);

            foreach (CargoSlot slot in _cargoSlots)
                slot.Tick(tick);

            return _needsUpdate;
        }
    }
}