using System.Numerics;

namespace Models
{
    public class CargoSlot 
        : IUpdatable
        , IOccupiable<Shelf>
    {
        private Vector3 _relativePosition;
        private Shelf _content;
        private Shelf _reservedBy;

        // IOccupiable implementation
        private IReleasable<Shelf> _occupant;
        private ICargoCarrier _parent;
        private bool _needsUpdate;

        public CargoSlot(ICargoCarrier parent, Vector3 posRelative)
        {
            this._parent = parent;
            this._relativePosition = posRelative;
        }

        public Shelf Occupant => _reservedBy;
        public bool IsOccupied => _occupant != null && !_occupant.IsReleased();
        public bool IsEmpty => _content == null;
        public bool IsAvailable => _content == null && _reservedBy == null;
        public bool IsReserved => _reservedBy != null; 
        public Shelf Cargo => _content; 
        public Vector3 PositionAbsolute => _parent.Position + _relativePosition;

        bool IUpdatable.NeedsUpdate() => _needsUpdate;

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

        public IReleasable<Shelf> Occupy(Shelf occupant)
        {
            this._occupant = new DeOccupier<Shelf>(occupant);
            return this._occupant;
        }
    }
}
