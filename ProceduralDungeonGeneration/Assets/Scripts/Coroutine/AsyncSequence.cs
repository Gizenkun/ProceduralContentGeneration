/*----------------------------------------------------------------
// Functional description: 
// Author: Xiexi
// Created Time: 2018/2/7 星期三 下午 5:31:52
// Version: V1.0.0
//--------------------------------------------------------------*/
using System;
using System.Collections.Generic;

public class AsyncSequence
{
    private Action<AsyncResult> _onComplete;
    private Action _onCancel;
    private Queue<AsyncQueueCallback> _queue = new Queue<AsyncQueueCallback>();
    private bool _isCancel = false;
    public AsyncSequence()
    {
        _onComplete = (cb) => { };
    }

    public AsyncSequence(Action<AsyncResult> completeCb)
    {
        _onComplete = completeCb;
    }

    public void Add(AsyncQueueCallback action)
    {
        _queue.Enqueue(action);
    }

    public void Add(IEnumerable<AsyncQueueCallback> actions)
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
            cb?.Invoke(AsyncResult.Success);
        });
    }

    public void Add(AsyncSequence aq)
    {
        while (aq != null && aq._queue.Count > 0)
        {
            _queue.Enqueue(aq._queue.Dequeue());
        }
        _queue.Enqueue((cb) =>
        {
            AsyncResult ar = AsyncResult.Success;
            aq._onComplete?.Invoke(ar);
            cb?.Invoke(ar);
        });
    }

    public void Do()
    {
        Do(AsyncResult.Success);
    }

    public void Do(AsyncResult result)
    {
        if (_isCancel)
        {
            _onCancel?.Invoke();
        }
        else
        {
            if (result.IsSuccess)
            {
                AsyncQueueCallback action = null;
                while (_queue.Count > 0 && action == null)
                {
                    action = _queue.Dequeue();
                }
                if (action != null)
                {
                    action.Invoke(Do);
                }
                else
                {
                    _onComplete?.Invoke(result);
                }
            }
            else
            {
                _onComplete?.Invoke(result);
            }
        }
    }

    public void Break(Action cencelCb)
    {
        _onCancel = cencelCb;
        _isCancel = true;
        _queue.Clear();
        _onComplete = null;
    }
}
