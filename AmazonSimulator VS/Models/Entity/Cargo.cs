using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Models
{
    public class CargoSlot : IUpdatable, IOccupiable<Shelf>
    {
        private Vector3 _relativePosition;
        private Shelf _content;
        private Shelf _reservedBy;

        // IOccupiable implementation
        private IReleasable<Shelf> _occupant;
        public Shelf Occupant => _reservedBy;
        public bool IsOccupied => _occupant != null && !_occupant.IsReleased();

        private ICargoCarrier parent;
        public bool needsUpdate;

        public CargoSlot(ICargoCarrier parent, Vector3 posRelative)
        {
            this.parent = parent;
            this._relativePosition = posRelative;
        }

        public bool IsEmpty { get => _content == null; }
        public bool IsAvailable { get => _content == null && _reservedBy == null; }
        public bool IsReserved { get => _reservedBy != null; }
        public Shelf Cargo { get => _content; }
        public Vector3 PositionAbsolute { get => parent.Position + _relativePosition; }


        public bool SetCargo(Shelf cargo)
        {
            if (IsAvailable) this._content = cargo;
            else return false;

            return true;
        }

        public bool ReserveForCargo(Shelf cargo)
        {
            if (!IsAvailable || _reservedBy != null) return false;
            this._reservedBy = cargo;
            return true;
        }

        public bool CancelReservation()
        {
            if (!this.IsReserved) return false;
            this._reservedBy = null;
            return true;
        }

        public Shelf ReleaseCargo()
        {
            Shelf cargo = this._content;
            this._content = null;

            return cargo;
        }

        public bool Tick(int tick)
        {
            if (!IsEmpty) Cargo.Move(PositionAbsolute);
            return true;
        }

        bool IUpdatable.NeedsUpdate()
        {
            throw new NotImplementedException();
        }        

        public IReleasable<Shelf> Occupy(Shelf occupant)
        {
            this._occupant = new DeOccupier<Shelf>(occupant);
            return this._occupant;
        }
    }
}
