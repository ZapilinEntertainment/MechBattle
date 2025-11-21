using System;
using System.Collections.Generic;
using VContainer;
using R3;

namespace ZE.Workers
{
    public abstract class Worker : IDisposable
    {
        protected enum Status : byte { Created = 0, Working, Disposed}

        public Observable<Unit> Disposed => _onDisposeCommand;
        protected IObjectResolver ObjectResolver => _objectResolver;
        protected readonly CompositeDisposable CompositeDisposable = new();
        protected Status WorkerStatus { get; private set; }
        private readonly ReactiveCommand _onDisposeCommand = new();        
        private List<Worker> _subWorkers;

        // field-injection: no need to override constructor every time
        [Inject] private IObjectResolver _objectResolver;

        public virtual void Start() 
        {
            WorkerStatus = Status.Working;
            //UnityEngine.Debug.Log(this.GetType() + WorkerStatus.ToString());
        }

        public T StartSubWorker<T>() where T : Worker
        {
            var worker = _objectResolver.Resolve<T>();
            if (worker == null)
                throw new Exception($"cannot resolve {typeof(T)} worker");

            _subWorkers ??= new List<Worker>();
            _subWorkers.Add(worker);
            worker.Disposed.Subscribe(_ => OnSubWorkerDisposed(worker)).AddTo(CompositeDisposable);

            worker.Start();
            return worker;
        }

        public virtual void Dispose()
        {
            WorkerStatus = Status.Disposed;
            if ((_subWorkers?.Count ?? 0) != 0)
            {
                foreach (var worker in _subWorkers)
                    worker.Dispose();
                _subWorkers.Clear();
            }
            CompositeDisposable.Dispose();
            _onDisposeCommand.Execute(Unit.Default);
            _onDisposeCommand.Dispose();
        }    

        private void OnSubWorkerDisposed(Worker worker)
        {
            if (WorkerStatus != Status.Working )
                return;

            _subWorkers.Remove(worker);
        }
    }
}
