using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    public interface ICommandHandle : IObservable<ICommandResponse>
    {
        void OnResponseReceived(ICommandResponse response);
    }

    public interface ICommandResponse
    {
        Guid GetCommandID();
        bool IsFinalResponse();
    }

    public class CommandHandle<T> : ICommandHandle where T : UICommand
    {
        private List<IObserver<ICommandResponse>> _observers;

        private T _command;
        private bool _sent;
        private Task _timeOutTask;

        public T Command { get => _command; }
        public bool IsCommandSent { get => _sent; }

        public CommandHandle(T command)
        {
            this._command = command;
            this._sent = false;
        }

        public void OnResponseReceived(ICommandResponse response)
        {
            _observers.ForEach(o => o.OnNext(response));

            if (response.IsFinalResponse())
                _observers.ForEach(o => o.OnCompleted());
        }

        public async Task TimeOutAfter(TimeSpan timeout)
        {
            _timeOutTask = new Task(() =>
            {
                _observers.ForEach(o => o.OnError(new TimeoutException("CommandResponse timed out after " + timeout.TotalSeconds + " ms!")));
            });
            await _timeOutTask;
        }

        public IDisposable Subscribe(IObserver<ICommandResponse> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<ICommandResponse>(this._observers, observer);
        }
    }
}
