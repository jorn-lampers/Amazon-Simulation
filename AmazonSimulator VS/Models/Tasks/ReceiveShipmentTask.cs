using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class ReceiveShipmentTask
        : SimulationTask<World>
    {
        /// <summary>
        /// Used internally to keep track of IncomingShipmentTask's state
        /// </summary>
        private enum TaskState : int
        {
            /// <summary>
            /// Wait for base.isStarted => Spawn truck with cargo
            /// </summary>
            Init,
            /// <summary>
            /// Wait for truck to arrive at POI.TruckStop => Order Robots to unload truck
            /// </summary>
            WaitTruckArrival,
            /// <summary>
            /// Wait for Robots to finish unloading Truck => Order Truck to exit the scene
            /// </summary>
            WaitTruckUnloaded,
            /// <summary>
            /// Wait for Robots to place all cargo in their target StoragePlots
            /// </summary>
            WaitCargoTasksFinished,
            /// <summary>
            /// Wait for Truck to reach POI.TruckDespawn => Despawn Truck
            /// </summary>
            WaitTruckExit,
            /// <summary>
            /// Truck is despawned => Mark Task as finished
            /// </summary>
            Finished
        }

        private TaskState _state;

        private Truck _truck;
        private Queue<CargoSlot> _slotsToUnload;
        private List<RobotUnloadTruckTask> _robotTasks;

        public ReceiveShipmentTask(World target)
            : base(target) { }

        public override bool Tick()
        {
            switch (_state)
            {
                case TaskState.Init:
                    _truck = _targetEntity.CreateTruck(Constants.TruckSpawn, true);
                    
                    _truck.SetPathfindingTarget(Constants.TruckStop, _targetEntity.TruckGraph);

                    this._state = TaskState.WaitTruckArrival;
                    break;

                case TaskState.WaitTruckArrival:
                    if (!_truck.IsAtDestination()) break;
                    _slotsToUnload = new Queue<CargoSlot>(_truck.OccupiedCargoSlots);
                    _robotTasks = new List<RobotUnloadTruckTask>();
                    this._state = TaskState.WaitTruckUnloaded;
                    break;

                case TaskState.WaitTruckUnloaded:
                    List<Robot> idleRobots = _targetEntity.ObjectsOfType<Robot>().Where((r) => r.IsStandBy).ToList();
                    List<CargoSlot> availableStorageSlots = _targetEntity.ObjectsOfType<StoragePlot>().SelectMany(plot => plot.FreeCargoSlots).ToList();
                    if (idleRobots.Count == 0 || availableStorageSlots.Count == 0) break;

                    RobotUnloadTruckTask rt = new RobotUnloadTruckTask(idleRobots[0], _truck, _slotsToUnload.Dequeue().ReleaseCargo(), availableStorageSlots[0]);
                    idleRobots[0].AssignTask(rt);
                    _robotTasks.Add(rt);

                    if (_slotsToUnload.Count == 0) _state = TaskState.WaitCargoTasksFinished;
                    break;

                case TaskState.WaitCargoTasksFinished:
                    if (!_robotTasks.All((task) => task.IsFinished)) break;
                    _truck.SetPathfindingTarget(Constants.TruckDespawn, _targetEntity.TruckGraph);
                    _state = TaskState.WaitTruckExit;
                    break;

                case TaskState.WaitTruckExit:
                    if (!_truck.IsAtDestination()) break;
                    _truck.Destroy();
                    _state = TaskState.Finished;
                    break;

                case TaskState.Finished:
                    this._isFinished = true;
                    break;

            }
            return base.Tick();
        }
    }

    public class SendShipmentTask
    : SimulationTask<World>
    {
        /// <summary>
        /// Used internally to keep track of IncomingShipmentTask's state
        /// </summary>
        private enum TaskState : int
        {
            /// <summary>
            /// Wait for base.isStarted => Spawn truck with cargo
            /// </summary>
            Init,
            /// <summary>
            /// Wait for truck to arrive at POI.TruckStop => Order Robots to unload truck
            /// </summary>
            WaitTruckArrival,
            /// <summary>
            /// Wait for Robots to finish unloading Truck => Order Truck to exit the scene
            /// </summary>
            WaitTruckLoaded,
            /// <summary>
            /// Wait for Robots to place all cargo in their target StoragePlots
            /// </summary>
            WaitCargoTasksFinished,
            /// <summary>
            /// Wait for Truck to reach POI.TruckDespawn => Despawn Truck
            /// </summary>
            WaitTruckExit,
            /// <summary>
            /// Truck is despawned => Mark Task as finished
            /// </summary>
            Finished
        }

        private TaskState _state;

        private Truck _truck;
        private Queue<CargoSlot> _slotsToUnload;
        private List<RobotLoadTruckTask> _robotTasks;

        public SendShipmentTask(World target)
            : base(target) { }

        public override bool Tick()
        {
            switch (_state)
            {
                case TaskState.Init:
                    _truck = _targetEntity.CreateTruck(Constants.TruckSpawn, false);
                    _truck.SetPathfindingTarget(Constants.TruckStop, _targetEntity.TruckGraph);

                    this._state = TaskState.WaitTruckArrival;
                    break;

                case TaskState.WaitTruckArrival:
                    if (!_truck.IsAtDestination()) break;
                    var occupied = _targetEntity.ObjectsOfType<StoragePlot>().SelectMany(plot => plot.OccupiedCargoSlots).ToList();
                    _slotsToUnload = new Queue<CargoSlot>(occupied.Take(Math.Min(occupied.Count(), _truck.FreeCargoSlots.Count)));
                    _robotTasks = new List<RobotLoadTruckTask>();
                    this._state = TaskState.WaitTruckLoaded;
                    break;

                case TaskState.WaitTruckLoaded:
                    List<Robot> idleRobots = _targetEntity.ObjectsOfType<Robot>().Where((r) => r.IsStandBy).ToList();
                    List<CargoSlot> availableStorageSlots = _truck.FreeCargoSlots;
                    if (idleRobots.Count == 0 || availableStorageSlots.Count == 0) break;

                    RobotLoadTruckTask rt = new RobotLoadTruckTask(idleRobots[0], _truck, _slotsToUnload.Dequeue().ReleaseCargo(), availableStorageSlots[0]);
                    idleRobots[0].AssignTask(rt);
                    _robotTasks.Add(rt);

                    if (_slotsToUnload.Count == 0) _state = TaskState.WaitCargoTasksFinished;
                    break;

                case TaskState.WaitCargoTasksFinished:
                    if (!_robotTasks.All((task) => task.IsFinished)) break;
                    _truck.SetPathfindingTarget(Constants.TruckDespawn, _targetEntity.TruckGraph);
                    _state = TaskState.WaitTruckExit;
                    break;

                case TaskState.WaitTruckExit:
                    if (!_truck.IsAtDestination()) break;
                    _truck.Destroy();
                    _state = TaskState.Finished;
                    break;

                case TaskState.Finished:
                    this._isFinished = true;
                    break;

            }
            return base.Tick();
        }
    }
}
