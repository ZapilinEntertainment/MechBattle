using System;
using System.Collections.Concurrent;
using R3;

public static class MessageBroker
{
    // deepseek generated

    private static readonly ConcurrentDictionary<Type, object> _subjects = new();

    public static void Publish<T>(T message)
    {
        if (_subjects.TryGetValue(typeof(T), out var subjectObj) &&
            subjectObj is Subject<T> subject)
        {
            subject.OnNext(message);
        }
    }

    public static Observable<T> Receive<T>()
    {
        var subject = _subjects.GetOrAdd(typeof(T), _ => new Subject<T>()) as Subject<T>;
        return subject!;
    }
}