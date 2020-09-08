/*----------------------------------------------------------------
// Functional description: 
// Author: Xiexi
// Created Time: 2018/1/25 星期四 下午 2:35:26
// Version: V1.0.0
//--------------------------------------------------------------*/
using System;


public class AsyncResult
{
    public object Data { get; protected set; }
    public string ErrorMsg { get; protected set; }
    public bool IsSuccess { get; protected set; }
    public static AsyncResult Success = new AsyncResult { IsSuccess = true };
    public static AsyncResult GetFailed(string errMsg) => new AsyncResult { IsSuccess = false, ErrorMsg = errMsg };
    public static implicit operator bool(AsyncResult r) => r != null ? r.IsSuccess : false;
}
public class AsyncResult<T> : AsyncResult
{
    public T Object { get; private set; }
    public static AsyncResult<T> GetSuccess(T obj) => new AsyncResult<T> { IsSuccess = true, Object = obj, Data = obj };
    public static new AsyncResult<T> GetFailed(string errMsg) => new AsyncResult<T> { IsSuccess = false, ErrorMsg = errMsg };
}
public delegate void AsyncQueueCallback(Action<AsyncResult> callback);
