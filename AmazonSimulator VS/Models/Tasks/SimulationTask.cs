using System;

namespace Models
{
    public abstract class SimulationTask<T> 
    {
        protected T _targetEntity;
        protected bool _isFinished = false;
        protected int _tickRuntime = 0;

        public T TargetEntity 
            => TargetEntity; 

        public bool IsFinished
            => _isFinished; 

        public int TickRuntime 
            => _tickRuntime; 

        public SimulationTask(T target)
            => this._targetEntity = target;

        /// <summary> Ticks the task once, all task-related logic should go here</summary>
        /// <param name="model">The world model to run this task on.</param>
        /// <returns>Whether the task has completed</returns>
        public virtual bool Tick()
        {
            if (!this._isFinished) _tickRuntime++;
            else Console.WriteLine("Task {0} finished after {1} ticks!", this.ToString(), this.TickRuntime);

            return this._isFinished;
        }
    }
}
