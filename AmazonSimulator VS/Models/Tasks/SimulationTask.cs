using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Models
{
    public abstract class SimulationTask<T> 
    {
        protected T _targetEntity;
        protected bool _isFinished = false;
        protected int _tickRuntime = 0;

        public T TargetEntity { get => TargetEntity; }
        public bool IsFinished { get => _isFinished; }
        public int TickRuntime { get => _tickRuntime; }

        public SimulationTask(T target)
        {
            this._targetEntity = target;
            Console.WriteLine("Created new SimulationTask with derived type: {0}", this.ToString());
        }

        /// <summary>
        /// Ticks the task once, all task-related logic should go here
        /// </summary>
        /// <param name="model">The world model to run this task on.</param>
        /// <returns>Whether the task has completed</returns>
        public virtual bool Tick()
        {
            if(_tickRuntime == 0) Console.WriteLine("Task {0} started running!", this.ToString(), this.TickRuntime);
            if (!this._isFinished) _tickRuntime++;
            else Console.WriteLine("Task {0} finished after {1} ticks!", this.ToString(), this.TickRuntime);
            return this._isFinished;
        }
    }

    public class EmptyTask<T> : SimulationTask<T>
    {
        public EmptyTask(T target) : base(target)
        {
            this._isFinished = true;
        }

        public override bool Tick()
        {
            return this._isFinished;
        }
    }

    public class ReceiveShipmentTask : SimulationTask<World>
    {
        /// <summary>
        /// Used internally to keep track of IncomingShipmentTask's state
        /// </summary>
        private enum TaskState : int {
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

        public ReceiveShipmentTask(World target) : base(target)
        {
            
        }

        public override bool Tick()
        {
            switch (_state)
            {
                case TaskState.Init:
                    _truck = _targetEntity.CreateTruck(_targetEntity.POI.TruckSpawn);
                    _truck.SetPathfindingTarget(_targetEntity.POI.TruckStop, _targetEntity.TruckGraph);

                    this._state = TaskState.WaitTruckArrival;
                    break;

                case TaskState.WaitTruckArrival:
                    if (!_truck.IsAtDestination()) break;
                    _slotsToUnload = new Queue<CargoSlot>(_truck.OccupiedCargoSlots);
                    _robotTasks = new List<RobotUnloadTruckTask>();
                    this._state = TaskState.WaitTruckUnloaded;
                    break;

                case TaskState.WaitTruckUnloaded:
                    List<Robot> idleRobots = _targetEntity.GetObjectsOfType<Robot>().Where((r) => r.IsStandBy).ToList();
                    List<CargoSlot> availableStorageSlots = _targetEntity.GetFreeStoragePlotCargoSlots();
                    if (idleRobots.Count == 0 || availableStorageSlots.Count == 0) break;

                    RobotUnloadTruckTask rt = new RobotUnloadTruckTask(idleRobots[0], _truck, _targetEntity, _slotsToUnload.Dequeue().ReleaseCargo(), availableStorageSlots[0]);
                    idleRobots[0].AssignTask(rt);
                    _robotTasks.Add(rt);

                    if (_slotsToUnload.Count == 0) _state = TaskState.WaitCargoTasksFinished;
                    break;

                case TaskState.WaitCargoTasksFinished:
                    if (!_robotTasks.All((task) => task.IsFinished)) break;
                    _truck.SetPathfindingTarget(_targetEntity.POI.TruckDespawn, _targetEntity.TruckGraph);
                    _state = TaskState.WaitTruckExit;
                    break;

                case TaskState.WaitTruckExit:
                    if (!_truck.IsAtDestination()) break;
                    _targetEntity.DestroyObject(_truck);
                    _state = TaskState.Finished;
                    break;

                case TaskState.Finished:
                    this._isFinished = true;
                    break;

            }
            return base.Tick();
        }
    }

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

        public PathfinderTask(Robot entity, Vector3 targetPosition, Graph targetGraph) : base(entity)
        {
            _state = TaskState.Init;
            this._targetGraph = targetGraph;
            this._targetPosition = targetPosition;
        }

        public override bool Tick()
        {
            switch(_state)
            {
                case TaskState.Init:
                    _targetEntity.SetPathfindingTarget(_targetPosition, _targetGraph);
                    _state = TaskState.MoveToDestination;
                    break;
                case TaskState.MoveToDestination:
                    if (_targetEntity.IsAtDestination()) _state = TaskState.Finished;
                    this._isFinished = true;
                    break;
                case TaskState.Finished:
                    break;
            }

            return base.Tick();
        }
    }

    public class RobotUnloadTruckTask : SimulationTask<Robot>
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
        private EntityEnvironmentInfoProvider _info;

        private IReleasable<Robot> _lock;

        public Robot Robot { get => _targetEntity as Robot; }
        public TaskState State { get => _state; }
        public Truck Truck { get => _truck; }
        public Shelf Cargo { get => _item; }
        public CargoSlot Destination { get => _destination; }

        public RobotUnloadTruckTask(Robot robot, Truck truck, EntityEnvironmentInfoProvider info, Shelf item, CargoSlot destination) : base(robot)
        {
            this._item = item;
            this._destination = destination;
            this._truck = truck;
            this._info = info;

            if (!this._destination.ReserveForCargo(item)) throw new InvalidOperationException("Robot.CargoTask could not reserve CargoSlot!");
        }

        public override bool Tick()
        {
            switch (State)
            {
                case RobotUnloadTruckTask.TaskState.Init:
                    _targetEntity.SetPathfindingTarget(_info.RobotQueueStart.Position, _targetEntity.PathfindingGraph);
                    _state = TaskState.MoveToQueue;
                    break;

                case RobotUnloadTruckTask.TaskState.MoveToQueue:
                    if (_targetEntity.IsAtDestination()) _state = TaskState.AwaitTruckAvailable;
                    break;

                case RobotUnloadTruckTask.TaskState.AwaitTruckAvailable:
                    if (!Truck.IsOccupied)
                    {
                        _lock = Truck.Occupy(Robot);
                        _targetEntity.SetPathfindingTarget(Cargo.Position);
                        _state = TaskState.MoveToCargo;
                    }
                    break;
                case RobotUnloadTruckTask.TaskState.MoveToCargo:
                    if (_targetEntity.IsAtDestination()) _state = TaskState.PickupCargo;
                    break;
                case RobotUnloadTruckTask.TaskState.PickupCargo:
                    if (!_targetEntity.TryAddCargo(_item)) break;
                    _targetEntity.SetPathfindingTarget(_info.RobotTruckExit.Position);
                    _state = TaskState.LeaveTruck;
                    break;
                case RobotUnloadTruckTask.TaskState.LeaveTruck:
                    if (_targetEntity.IsAtDestination())
                    {
                        _targetEntity.SetPathfindingTarget(_destination.PositionAbsolute, _targetEntity.PathfindingGraph);
                        _lock.Release();
                        _state = TaskState.MoveToDestination;
                    }
                    break;
                case RobotUnloadTruckTask.TaskState.MoveToDestination:
                    if (_targetEntity.IsAtDestination())
                    {
                        _state = TaskState.DropOffDestination;
                        _targetEntity.ReleaseCargo();
                    }
                    break;
                case RobotUnloadTruckTask.TaskState.DropOffDestination:
                    _destination.SetCargo(_item);
                    _state = TaskState.Finished;
                    this._isFinished = true;
                    break;
                case RobotUnloadTruckTask.TaskState.Finished:
                    break;
            }
            return base.Tick();
        }
    }
}
