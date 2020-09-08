/*----------------------------------------------------------------
// Functional description: 异步队列
// Author: Xiexi
// Created Time: 2017/9/12 星期二 下午 3:46:02
// Version: V1.0.0
//--------------------------------------------------------------*/
using System;
using System.Collections.Generic;
public class AsyncQueue
{
    private Action _onComplete;
    private Queue<Action<Action>> _queue = new Queue<Action<Action>>();


    public AsyncQueue(Action completeCb = null)
    {
        _onComplete = completeCb;
    }

    public void Add(Action<Action> action)
    {
        _queue.Enqueue(action);
    }

    public void Add(AsyncQueue aq)
    {
        while (aq != null && aq._queue.Count > 0)
        {
            _queue.Enqueue(aq._queue.Dequeue());
        }
        _queue.Enqueue((cb) =>
        {
            aq._onComplete?.Invoke();
            cb?.Invoke();
        });
    }

    public void Add(IEnumerable<Action<Action>> actions)
    {
        foreach (var action in actions)
        {
            _queue.Enqueue(action);
        }
    }

    public void Add(Action action)
    {
        _queue.Enqueue((cb) =>
        {
            action?.Invoke();
            cb?.Invoke();
        });
    }

    public void Add(IEnumerable<Action> actions)
    {
        foreach (var action in actions)
        {
            _queue.Enqueue((cb) =>
            {
                action?.Invoke();
                cb?.Invoke();
            });
        }
    }

    public void Do()
    {
        Action<Action> action = null;
        while (_queue.Count > 0 && action == null)
        {
            action = _queue.Dequeue();
        }
        if (action != null)
        {
            action?.Invoke(Do);
        }
        else
        {
            _onComplete?.Invoke();
        }
    }

    public void Break()
    {
        //UnityEngine.Debug.Log("<color=red>打断队列! </color>");
        _queue.Clear();
        _onComplete = null;
    }
}
