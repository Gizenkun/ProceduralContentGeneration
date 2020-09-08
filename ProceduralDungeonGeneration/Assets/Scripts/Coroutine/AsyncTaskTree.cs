using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AsyncTaskNode<T>
{
    public bool IsCompleted { get; private set; }
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; } = "";
    public T TaskInfo { get; private set; }

    private Action<AsyncResult> _onComplete;
    private Action _onCancel;
    private AsyncQueueCallback _action;
    private Queue<AsyncTaskNode<T>> _subTasks = new Queue<AsyncTaskNode<T>>();
    private bool _isCancel = false;

    public AsyncTaskNode(T info, Action<AsyncResult> completeCb)
    {
        TaskInfo = info;
        _onComplete = completeCb;
    }


    private AsyncTaskNode(T info, AsyncQueueCallback action, AsyncTaskNode<T> parent)
    {
        TaskInfo = info;
        _action = action;
        _onComplete = result =>
        {
            parent.Do(result);
        };
    }

    public AsyncTaskNode<T> Add(T info, AsyncQueueCallback action)
    {
        AsyncTaskNode<T> task = new AsyncTaskNode<T>(info, action, this);
        _subTasks.Enqueue(task);
        return task;
    }

    private static List<AsyncTaskNode<T>> GetSubTree(AsyncTaskNode<T> task)
    {
        List<AsyncTaskNode<T>> list = new List<AsyncTaskNode<T>>(task._subTasks.SelectMany(t => GetSubTree(t)));
        list.Add(task);
        return list;
    }

    public AsyncTaskNode<T>[] Tasks => GetSubTree(this).Where(task => task.TaskInfo != null).ToArray().ToArray();


    public void Do()
    {
        Do(AsyncResult.Success);
    }

    private void Do(AsyncResult result)
    {
        if (_isCancel)
        {
            _onCancel?.Invoke();
        }
        else
        {
            if (result)
            {
                AsyncTaskNode<T> task = null;
                while (_subTasks.Count > 0 && task == null)
                {
                    task = _subTasks.Peek();
                }
                if (task != null)
                {
                    task._action?.Invoke(r =>
                    {
                        if (r)//如果成功, 移除这项任务
                            {
                            _subTasks.Dequeue();
                        }
                        task.IsCompleted = r;
                        Do(r);
                    });
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
        _subTasks.Clear();
        _onComplete = null;
    }
}
