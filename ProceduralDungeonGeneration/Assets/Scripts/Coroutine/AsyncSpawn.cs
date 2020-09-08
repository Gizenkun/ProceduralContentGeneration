/*----------------------------------------------------------------
// Functional description: 
// Author: Xiexi
// Created Time: 2018/2/7 星期三 下午 5:35:32
// Version: V1.0.0
//--------------------------------------------------------------*/
using System;
using System.Collections.Generic;

public class AsyncSpawn
{
    private Action<AsyncResult> _onComplete;
    private List<AsyncQueueCallback> _list = new List<AsyncQueueCallback>();
    private bool _doing;

    public AsyncSpawn(Action<AsyncResult> completeCb = null)
    {
        _onComplete = completeCb;
        _doing = false;
    }

    public void Add(AsyncQueueCallback action)
    {
        if (_doing)
        {
            throw new Exception("运行时禁止插入新任务! ");
        }
        _list.Add(action);
    }

    public void Do()
    {
        _doing = true;
        int total = _list.Count;
        bool isSuccess = true;
        if (total == 0)
        {
            _onComplete?.Invoke(AsyncResult.Success);
            return;
        }

        foreach (var action in _list)
        {
            action?.Invoke(result =>
            {
                if (!result.IsSuccess)
                {
                    isSuccess = false;
                    _onComplete?.Invoke(result);
                    _onComplete = null;
                }
                total--;
                if (total == 0 && isSuccess)
                {
                    _onComplete?.Invoke(AsyncResult.Success);
                }
            });
        }
    }
}
