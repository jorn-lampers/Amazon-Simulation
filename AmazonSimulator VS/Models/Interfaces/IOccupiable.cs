namespace Models
{
    interface IOccupiable<T>
    { 
        T Occupant { get; }
        bool IsOccupied { get; }
        IReleasable<T> Occupy(T occupant);
    }

    public interface IReleasable<T>
    {
        T Owner { get; }
        bool IsReleased();
        void Release();
    }

    public class DeOccupier<T> : IReleasable<T>
    {
        private bool _released = false;
        private T _owner;

        public T Owner => _owner;

        public DeOccupier(T owner)
        {
            this._owner = owner;
        }

        public bool IsReleased()
        {
            return _released;
        }

        public void Release()
        {
            this._released = true;
        }
    }
}
