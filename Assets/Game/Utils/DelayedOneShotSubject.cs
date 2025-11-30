using R3;
using System;

namespace ZE.MechBattle
{
    // - doesnt send the default value
    // - stores value after set
    // - notify subscribers when value is set (1 time)
    // - returns value afterwards without subscription
    public class DelayedOneShotSubject<T> : Observable<T>, IDisposable
    { 
        public bool IsValueSet { get; private set;}
        protected T Value { get; private set;}
        private Subject<T> _valueSendCommand;
        
        public virtual void SetValue(T value)
        {
            Value = value;
            IsValueSet = Value != null;
            if (_valueSendCommand != null)
            {
                _valueSendCommand.OnNext(Value);
                _valueSendCommand.Dispose();
                _valueSendCommand = null;
            }
        }

        public virtual void Dispose() => _valueSendCommand?.Dispose();

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            if (IsValueSet)
            {
                observer.OnNext(Value);
                return Disposable.Empty;
            }                
            else
            {
                _valueSendCommand ??= new();
                return _valueSendCommand.Subscribe(observer);
            }
        }
    }
}
