using System;

namespace Models
{
    public class RobotLoadTruckTask 
        : SimulationTask<Robot>
    {
        public enum TaskState
        {
            Init,
            MoveToItem,
            PickupCargo,
            MoveToQueue,
            AwaitTruckAvailable,
            DropOffDestination,
            LeaveTruck,
            Finished
        }

        private Shelf _item;
        private CargoSlot _destination;
        private TaskState _state;
        private Truck _truck;
        private EntityEnvironmentInfoProvider _info;

        private IReleasable<Robot> _lock;

        public Robot Robot
            => _targetEntity as Robot;

        public TaskState State
            => _state;

        public Truck Truck
            => _truck;

        public Shelf Cargo
            => _item;

        public CargoSlot Destination
            => _destination;

        public RobotLoadTruckTask(Robot robot, Truck truck, EntityEnvironmentInfoProvider info, Shelf item, CargoSlot destination)
            : base(robot)
        {
            this._item = item;
            this._destination = destination;
            this._truck = truck;
            this._info = info;

            if (!this._destination.ReserveForCargo(item))
                throw new InvalidOperationException("Robot.CargoTask could not reserve CargoSlot!");
        }

        public override bool Tick()
        {
            switch (State)
            {
                case RobotLoadTruckTask.TaskState.Init:
                    _targetEntity.SetPathfindingTarget(_item.Position, _targetEntity.PathfindingGraph);
                    _state = TaskState.MoveToItem;
                    break;

                case RobotLoadTruckTask.TaskState.MoveToItem:
                    if (!_targetEntity.IsAtDestination()) break;
                    _state = TaskState.PickupCargo;
                    break;

                case RobotLoadTruckTask.TaskState.PickupCargo:
                    if (!_targetEntity.TryAddCargo(_item)) break;
                    _state = TaskState.MoveToQueue;
                    _targetEntity.SetPathfindingTarget(_info.RobotQueueStart, _targetEntity.PathfindingGraph);
                    break;
                case RobotLoadTruckTask.TaskState.MoveToQueue:
                    if (_targetEntity.IsAtDestination()) _state = TaskState.AwaitTruckAvailable;
                    break;
                case RobotLoadTruckTask.TaskState.AwaitTruckAvailable:
                    if (_truck.IsOccupied) break;
                    _lock = _truck.Occupy(_targetEntity);
                    _targetEntity.SetPathfindingTarget(_destination.PositionAbsolute);
                    _state = TaskState.DropOffDestination;
                    break;
                case RobotLoadTruckTask.TaskState.DropOffDestination:
                    if (!_targetEntity.IsAtDestination()) break;
                    _destination.SetCargo(_targetEntity.CargoSlots[0].ReleaseCargo());
                    _targetEntity.SetPathfindingTarget(_info.RobotTruckExit);
                    _state = TaskState.LeaveTruck;
                    break;
                case RobotLoadTruckTask.TaskState.LeaveTruck:
                    if (!_targetEntity.IsAtDestination()) break;

                    _targetEntity.SetPathfindingTarget(_targetEntity.IdlePos, _targetEntity.PathfindingGraph);
                    _lock.Release();
                    _state = TaskState.Finished;
                    this._isFinished = true;
                    break;
                case RobotLoadTruckTask.TaskState.Finished:
                    break;
            }
            return base.Tick();
        }
    }
}
