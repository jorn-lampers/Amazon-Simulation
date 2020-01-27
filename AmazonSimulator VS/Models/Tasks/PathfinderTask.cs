using System.Numerics;

namespace Models
{
    public class PathfinderTask : SimulationTask<Robot>
    {
        public enum TaskState
        {
            Init,
            MoveToDestination,
            Finished
        }

        private Graph _targetGraph;
        private Vector3 _targetPosition;
        private TaskState _state;

        public PathfinderTask(Robot entity, Vector3 targetPosition, Graph targetGraph) 
            : base(entity)
        {
            _state = TaskState.Init;
            this._targetGraph = targetGraph;
            this._targetPosition = targetPosition;
        }

        public override bool Tick()
        {
            switch (_state)
            {
                case TaskState.Init:
                    _targetEntity.SetPathfindingTarget(_targetPosition, _targetGraph);
                    _state = TaskState.MoveToDestination;
                    break;
                case TaskState.MoveToDestination:
                    if (!_targetEntity.IsAtDestination()) break;

                    _state = TaskState.Finished;
                    this._isFinished = true;
                    break;
                case TaskState.Finished:
                    break;
            }
            return base.Tick();
        }
    }
}
