using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Models;
using Views;

namespace Controllers {
    struct ObservingClient {
        public ClientView cv;
        public IDisposable unsubscribe;
    }

    public class SimulationController
    {
        private List<ObservingClient> _views = new List<ObservingClient>();
        private SimulationMetrics _metrics;
        private World _world;


        public SimulationController(World w)
        {
            this._metrics = SimulationMetrics.CreateDefault();
            this._world = w;
        }

        public void AddView(ClientView v)
        {
            ObservingClient oc = new ObservingClient();

            oc.unsubscribe = this._world.Subscribe(v);
            oc.cv = v;

            _views.Add(oc);
        }

        public void RemoveView(ClientView v)
        {
            for(int i = 0; i < _views.Count; i++)
            {
                ObservingClient currentOC = _views[i];

                if(currentOC.cv == v)
                {
                    _views.Remove(currentOC);
                    currentOC.unsubscribe.Dispose();
                }
            }
        }

        public void Simulate()
        {
            _metrics.StartRecording();

            while(_metrics.IsRunning)
            {
                // Inform metrics that the tick has started
                TimeSpan error = _metrics.StartTick();
                //Console.WriteLine("Tick started with an error of {0} ms from optimal starting time.", error.TotalMilliseconds);

                // Run a single tick of the simulation
                _world.Tick(_metrics.TickIntervalInMilliseconds);

                // Instruct Model to send updates to its observers
                _world.SendUpdate();

                foreach (ObservingClient client in _views)
                {
                    ServerCommand command;
                    while ((command = client.cv.nextCommandIn()) != null)
                    {
                        try
                        {   // Execute pending client commands with failsave
                            command.Execute(_world); 
                        } catch (Exception e)
                        {   // Catch and Log failure exceptions thrown during command execution
                            Console.WriteLine(e.ToString());
                            Console.WriteLine("Failed to execute command sent by Client: {0} ", command);
                        }
                    }
                }

                // Inform metrics the tick has ended
                _metrics.EndTick();

                /* 
                if(_metrics.TickNo % 1000 == 0)
                {
                    Console.WriteLine("Finished iteration in {0} Ms, next tick should be after {1} Ms.", _metrics.LastTickDuration.TotalMilliseconds, _metrics.TimeUntilNextTick.TotalMilliseconds);
                    Console.WriteLine("Average workload during the previous 100 ticks was {0}%.", _metrics.AverageWorkload100Ticks * 100);
                }
                */

                TimeSpan timeOut = _metrics.TimeUntilNextTick;

                if (timeOut.TotalMilliseconds > 0) Thread.Sleep(timeOut);
            }
        }

        public void EndSimulation()
        {
            _metrics.StopRecording();
        }
    }
}