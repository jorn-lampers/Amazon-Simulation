using AmazonSimulator_VS;
using System;

namespace Models
{
    public class RobotUnloadTruckTask 
        : SimulationTask<Robot>
    {
        public enum TaskState
        {
            Init,
            MoveToQueue,
            AwaitTruckAvailable,
            MoveToCargo,
            PickupCargo,
            LeaveTruck,
            MoveToDestination,
            DropOffDestination,
            Finished
        }

        private Shelf _item;
        private CargoSlot _destination;
        private TaskState _state;
        private Truck _truck;

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

        public RobotUnloadTruckTask(Robot robot, Truck truck, Shelf item, CargoSlot destination)
            : base(robot)
        {
            this._item = item;
            this._destination = destination;
            this._truck = truck;

            if (!this._destination.ReserveForCargo(item))
                throw new InvalidOperationException("Robot.CargoTask could not reserve CargoSlot!");
        }

        public override bool Tick()
        {
            switch (State)
            {
                case RobotUnloadTruckTask.TaskState.Init:
                    _targetEntity.SetPathfindingTarget(Constants.RobotEnterTruck, _targetEntity.GetPathfindingGraph());
                    _state = TaskState.MoveToQueue;
                    break;

                case RobotUnloadTruckTask.TaskState.MoveToQueue:
                    if (!_targetEntity.IsAtDestination()) break;
                    _state = TaskState.AwaitTruckAvailable;
                    break;

                case RobotUnloadTruckTask.TaskState.AwaitTruckAvailable:
                    if (Truck.IsOccupied) break;

                    _lock = Truck.Occupy(Robot);
                    _targetEntity.SetTarget(Cargo.Position);
                    _state = TaskState.MoveToCargo;
                    break;
                case RobotUnloadTruckTask.TaskState.MoveToCargo:
                    if (_targetEntity.IsAtDestination()) _state = TaskState.PickupCargo;
                    break;
                case RobotUnloadTruckTask.TaskState.PickupCargo:
                    if (!_targetEntity.TryAddCargo(_item)) break;
                    _targetEntity.SetTarget(Constants.RobotExitTruck);
                    _state = TaskState.LeaveTruck;
                    break;
                case RobotUnloadTruckTask.TaskState.LeaveTruck:
                    if (!_targetEntity.IsAtDestination()) break;

                    _targetEntity.SetPathfindingTarget(_destination.PositionAbsolute, _targetEntity.GetPathfindingGraph());
                    _lock.Release();
                    _state = TaskState.MoveToDestination;
                    break;
                case RobotUnloadTruckTask.TaskState.MoveToDestination:
                    if (!_targetEntity.IsAtDestination()) break;

                    _state = TaskState.DropOffDestination;
                    break;
                case RobotUnloadTruckTask.TaskState.DropOffDestination:
                    _destination.SetCargo(_targetEntity.ReleaseCargo());
                    _state = TaskState.Finished;
                    this._isFinished = true;
                    _targetEntity.SetPathfindingTarget(_targetEntity.IdlePos, _targetEntity.GetPathfindingGraph());
                    break;
                case RobotUnloadTruckTask.TaskState.Finished:
                    break;
            }
            return base.Tick();
        }
    }
}
