using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AsyncTaskSequence<T>
{
    public class AsyncTask
    {
        public bool IsCompleted { get; private set; }
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; } = "";
        public T TaskInfo { get; private set; }
        private AsyncQueueCallback _callback;
        public event Action OnStart;
        public event Action<float> OnProgress;
        public event Action<AsyncResult> OnCompleted;
        public AsyncTask(T taskInfo, AsyncQueueCallback callback)
        {
            TaskInfo = taskInfo;
            _callback = callback;
            IsCompleted = false;
            IsSuccess = false;
        }

        public void Execute(Action<AsyncResult> callback)
        {
            OnStart?.Invoke();
            _callback?.Invoke(callback);
        }

        public void Progress(float progress)
        {
            OnProgress?.Invoke(progress);
        }

        public void Complete(AsyncResult result)
        {
            IsCompleted = true;
            IsSuccess = result.IsSuccess;
            ErrorMessage = result.ErrorMsg;
            OnCompleted?.Invoke(result);
        }
    }
    private Action<AsyncResult> _onComplete;
    private Action _onCancel;
    private Queue<AsyncTask> _queue = new Queue<AsyncTask>();
    private bool _isCancel = false;

    public AsyncTaskSequence()
    {
        _onComplete = (cb) => { };
    }

    public AsyncTaskSequence(Action<AsyncResult> completeCb)
    {
        _onComplete = completeCb;
    }

    public void Add(AsyncTask task)
    {
        _queue.Enqueue(task);
    }

    public AsyncTask Add(AsyncQueueCallback action, T taskInfo)
    {
        AsyncTask task = new AsyncTask(taskInfo, action);
        _queue.Enqueue(task);
        return task;
    }

    public AsyncTask Add(Action action, T taskInfo)
    {
        AsyncTask task = new AsyncTask(taskInfo, (cb) =>
        {
            action?.Invoke();
            cb?.Invoke(AsyncResult.Success);
        });
        _queue.Enqueue(task);
        return task;
    }

    //public AsyncTask[] Tasks => _queue.Where(task => task.TaskInfo != null).ToArray();
    public AsyncTask[] Tasks
    {
        get
        {
            var tasks = _queue.Where(task => task.TaskInfo != null).ToArray();
#if DEBUG
            UnityEngine.Debug.Log("QUEUE:" + _queue.Select(t => t.TaskInfo).ToString());
            UnityEngine.Debug.Log("TASK:" + tasks.Select(t => t.TaskInfo).ToString());
#endif
            return tasks;
        }
    }


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
                AsyncTask task = null;
                while (_queue.Count > 0 && task == null)
                {
                    task = _queue.Peek();
                }
                if (task != null)
                {
                    task.Execute(r =>
                    {
                        if (r)//如果成功, 移除这项任务
                            {
                            _queue.Dequeue();
                        }
                        task.Complete(r);
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
        _queue.Clear();
        _onComplete = null;
    }
}
