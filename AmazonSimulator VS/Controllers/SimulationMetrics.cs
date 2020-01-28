using AmazonSimulator_VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    /// <summary>
    /// This struct is where SimulationController Delegates its metrics-related logic to,
    /// providing non-essential yet useful data & utilty to the simulation.
    /// This struct is also the carrier of command the parameters of the SimulationMetricsCommand,
    /// providing performance related data to, and synchronisation between ClientView and SimulationController.
    /// </summary>
    public struct SimulationMetrics
    {
        private DateTime _tickStartTime;    // Helper variables to support time related ...
        private DateTime _tickEndTime;      // ... fields and functions internally

        /// <summary>
        /// The preferred amount of time to span between the start of two seperate simulation ticks
        /// </summary>
        public int TickIntervalInMilliseconds;

        /// <summary>
        /// The current iteration cycle the simulation is in, starting at 0
        /// </summary>
        public int TickNo;

        /// <summary>
        /// Whether or not the simulation is running at time of access
        /// </summary>
        public bool IsRunning;

        /// <summary>
        /// The moment in time where corresponding SimulationController's function Simulate() was called 
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// Queue's caching the amount of milliseconds Simulation Iterations took 
        /// during the previous 100 ticks with intervals of 1 and 10 respectively
        /// </summary>
        public Queue<float> TickTimes1, TickTimes10;

        /// <summary>
        /// Gets the default state of simulation, to be called to accompany a fresh SimulationController
        /// </summary>
        /// <returns>
        /// An instance of SimulationMetrics representing a newly constructed Simulation that is yet to be started
        /// </returns>
        public static SimulationMetrics CreateDefault()
        {
            return new SimulationMetrics
            {
                TickIntervalInMilliseconds = 1000 / Constants.SIM_TPS,   // TPS == 1000 / TickIntervalInMilliseconds

                IsRunning = false,                  // Default state represent a fresh, uncommenced simulation
                StartTime = DateTime.MinValue,      // Defaults to epoch time to signify simulation is yet to be started

                TickNo = 0,                         // Counting begins at 0, as is tradition

                // These Queue's should cache 100 values at all times, initialized at 0 before the start of the simulation
                TickTimes1 = new Queue<float>(Enumerable.Repeat<float>(0.0f, 100)),
                TickTimes10 = new Queue<float>(Enumerable.Repeat<float>(0.0f, 100))
            };
        }

        /// <summary>
        /// The average workload put on simulator during the previous tick
        /// Where workload is defined as time used by simulation over specified available time per tick.
        /// </summary>
        public float AverageWorkload1Tick { get => TickTimes1.First() / TickIntervalInMilliseconds; }

        /// <summary>
        /// The average workload of simulator over the previous 10 ticks
        /// Where workload is defined as time used by simulation over specified available time per tick.
        /// </summary>
        public float AverageWorkload10Ticks { get => TickTimes1.Take(10).Average() / TickIntervalInMilliseconds; }

        /// <summary>
        /// The average workload of simulator over the previous 100 ticks
        /// Where workload is defined as time used by simulation over specified available time per tick.
        /// </summary>
        public float AverageWorkload100Ticks { get => TickTimes1.Take(100).Average() / TickIntervalInMilliseconds; }

        /// <summary>
        /// The estimated average workload of simulator over the previous 1000 ticks
        /// Where workload is defined as time used by simulation over specified available time per tick.
        /// </summary>
        public float AverageWorkload1000Ticks { get => TickTimes10.Take(100).Average() / TickIntervalInMilliseconds; }

        /// <summary>
        /// The optimal starting time for the next tick assuming TickIntervalInMilliseconds
        /// </summary>
        public DateTime NextTickStartTime { get => _tickStartTime.AddMilliseconds(TickIntervalInMilliseconds); }

        /// <summary>
        /// The time spanned between the beginning and the ending of the last simulation tick
        /// </summary>
        public TimeSpan LastTickDuration { get => _tickEndTime - _tickStartTime; }

        /// <summary>
        /// The time remaining from the moment of this call until the optimal start-time of the next tick
        /// </summary>
        public TimeSpan TimeUntilNextTick { get => NextTickStartTime - DateTime.Now; }

        /// <summary>
        /// Returns the maximum workload required by simulation to run a single 
        /// Simulation iteration in the specified amount of previous ticks.
        /// </summary>
        /// <param name="ageInTicks">
        /// The amount of previous tick data to evaluate.
        /// </param>
        /// <returns>
        /// The largest workload required by Simulator to finish a single Simulation iteration.
        /// Where workload is defined as time used by simulation over specified available time per tick.
        /// </returns>
        public float GetMaxWorkloadWithAge(int ageInTicks)
        {
            return ageInTicks > 100 ?
                TickTimes10.Take(ageInTicks % 10).Max() : TickTimes1.Take(ageInTicks).Max();
        }

        /// <summary>
        /// Returns the minimum workload required by simulation to run a single 
        /// Simulation iteration in the specified amount of previous ticks.
        /// </summary>
        /// <param name="ageInTicks">
        /// The amount of previous tick data to evaluate.
        /// </param>
        /// <returns>
        /// The smallest workload required by Simulator to finish a single Simulation iteration.
        /// Where workload is defined as time used by simulation over specified available time per tick.
        /// </returns>
        public float GetMinWorkloadWithAge(int ageInTicks)
        {
            return ageInTicks > 100 ?
                TickTimes10.Take(ageInTicks % 10).Min() : TickTimes1.Take(ageInTicks).Min();
        }

        /// <summary>
        ///     Instruct Metrics to start recording simulation performance
        /// </summary>
        internal void StartRecording()
        {
            if (this.IsRunning) throw new InvalidOperationException("Simulation metrics is already running.");

            this.IsRunning = true;
            this.StartTime = DateTime.Now;
        }

        /// <summary>
        ///     Instruct SimulationMetrics the simulation has paused either temporarily or indefinitely.
        /// </summary>
        internal void StopRecording()
        {
            if (!this.IsRunning) throw new InvalidOperationException("Simulation metrics are not running.");
        }

        /// <summary>
        /// Informs metrics to record the start of a new Simulation loop Iteration.
        /// This starts the timer keeping track of Iteration durations.
        /// </summary>
        /// <returns>
        /// The time spanned by the error between ideal and actual starting-time
        /// </returns>
        internal TimeSpan StartTick()
        {
            DateTime optimalStartTime = NextTickStartTime;
            _tickStartTime = DateTime.Now;
            return _tickStartTime - optimalStartTime;
        }
        /// <summary>
        ///     Informs metrics current Simulation loop's iteration has finished executing, so it can inspect this iteration's performance.
        /// </summary>
        /// <returns>
        ///     Time remaining in milliseconds until the start of the next tick
        /// </returns>
        internal double EndTick()
        {
            _tickEndTime = DateTime.Now;

            float msElapsed = (float)LastTickDuration.TotalMilliseconds;
            float workLoad = msElapsed / TickIntervalInMilliseconds;

            this.TickTimes1.Dequeue(); this.TickTimes1.Enqueue(workLoad);
            this.TickTimes10.Dequeue(); this.TickTimes10.Enqueue(workLoad);

            this.TickNo++;

            return this.TimeUntilNextTick.TotalMilliseconds;
        }
    };
}
