﻿using System.Numerics;

namespace Models
{
    public class CargoSlot 
        : IUpdatable
    {
        private Vector3 _relativePosition;
        private Shelf _content;
        private Shelf _reservedBy;

        // IOccupiable implementation
        private ICargoCarrier _parent;
        private bool _needsUpdate;

        public CargoSlot(ICargoCarrier parent, Vector3 posRelative)
        {
            this._parent = parent;
            this._relativePosition = posRelative;
        }

        public Shelf Occupant => _reservedBy;
        public bool IsOccupied => _content != null;
        public bool IsEmpty => _content == null;
        public bool IsAvailable => _content == null && _reservedBy == null;
        public bool IsReserved => _reservedBy != null; 
        public Shelf Cargo => _content; 
        public Vector3 PositionAbsolute => _parent.Position + Vector3.Transform(_relativePosition, Matrix4x4.CreateFromYawPitchRoll(_parent.Rotation.Y, _parent.Rotation.X, _parent.Rotation.Z));

        public bool NeedsUpdate(bool evaluateOnly = false)
        {
            bool val = _needsUpdate;
            if(!evaluateOnly) _needsUpdate = false;
            return val;
        }

        public bool SetCargo(Shelf cargo)
        {
            if (IsAvailable)
                ReserveForCargo(cargo);

            if (IsAvailable || _reservedBy == cargo)
            {
                this._content = cargo;
                this._needsUpdate = true;
            }

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
            if (this.IsReserved && this._reservedBy == cargo) CancelReservation();
            this._content = null;
            return cargo;
        }

        public bool Tick(int tick)
        {
            if (!IsEmpty && _parent.NeedsUpdate(true))
            {
                Cargo.Move(PositionAbsolute);
                Cargo.Rotate(_parent.Rotation.X, _parent.Rotation.Y, _parent.Rotation.Z);
            }

            return true;
        }

        public void Destroy()
        {
            if (Cargo != null) Cargo.Destroy();
        }
    }
}
