/*----------------------------------------------------------------
// Functional description: 协程管理器
// Author: Administrator
// Created Time: 2017/8/28 星期一 下午 8:32:14
// Version: V1.0.0
//--------------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    static private CoroutineHelper s_instance;
    static public CoroutineHelper Instance
    {
        get
        {
            if (s_instance == null)
            {
                GameObject go = new GameObject("CoroutineHelper");
                DontDestroyOnLoad(go);
                s_instance = go.AddComponent<CoroutineHelper>();
            }
            return s_instance;
        }
    }

    public static Coroutine Start(IEnumerator routine) => Instance.StartCoroutine(routine);

    public static void StopAll()
    {
        Instance.StopAllCoroutines();
    }

    public static void Stop(Coroutine routine)
    {
        Instance.StopCoroutine(routine);
    }

    public static void Stop(IEnumerator routine)
    {
        Instance.StopCoroutine(routine);
    }

    public interface ICoroutine
    {
        bool Tick(float deltaTime);
        void Break();
    }

    public class Timer : ICoroutine
    {
        private float _time;
        private float _repeatingTime;
        private float _repeatingT;
        private Action _cb;
        private Action<float> _repeating;
        private bool _enabled;

        public Timer(float time, Action cb, float repeatingTime, Action<float> repeating)
        {
            _time = time;
            _cb = cb;
            _repeatingTime = repeatingTime;
            _repeatingT = repeatingTime;
            _repeating = repeating;
            _enabled = true;
        }

        public void Break()
        {
            _enabled = false;
        }

        public bool Tick(float deltaTime)
        {
            if (!_enabled)
            {
                return true;
            }
            _time -= deltaTime;
            _repeatingT -= deltaTime;
            if (_time > 0)
            {
                if (_repeatingT <= 0)
                {
                    _repeating?.Invoke(_time);
                    _repeatingT += _repeatingTime;
                }
                return false;
            }
            else
            {
                _cb?.Invoke();
                return true;
            }
        }
    }

    public class Repeater : ICoroutine
    {
        private float _repeatingTime;
        private Func<bool> _repeating;
        private float _t;
        private bool _enabled = true;

        public Repeater(float repeatingTime, Func<bool> repeating)
        {
            _repeatingTime = repeatingTime;
            _t = repeatingTime;
            _repeating = repeating;
        }

        public void Break()
        {
            _enabled = false;
        }

        public bool Tick(float deltaTime)
        {
            if (!_enabled)
            {
                return true;
            }
            _t -= deltaTime;
            if (_t <= 0)
            {
                _t += deltaTime;
                return _repeating?.Invoke() ?? true;
            }
            return false;
        }
    }

    public ICoroutine Repeate(Func<bool> func, float time = 0)
    {
        Repeater repeater = new Repeater(time, func);
        _addList.Add(repeater);
        return repeater;
    }

    public ICoroutine Invoke(Action action, float time = 0, Action<float> repeating = null, float repeatTime = 0)
    {
        Timer timer = new Timer(time, action, repeatTime, repeating);
        _addList.Add(timer);
        return timer;
    }

    private readonly HashSet<ICoroutine> _coroutineList = new HashSet<ICoroutine>();
    private readonly HashSet<ICoroutine> _addList = new HashSet<ICoroutine>();
    private readonly HashSet<ICoroutine> _removeList = new HashSet<ICoroutine>();

    private void Update()
    {
        foreach (var timer in _addList)
        {
            _coroutineList.Add(timer);
        }
        _addList.Clear();
        foreach (var timer in _coroutineList)
        {
            if (timer.Tick(Time.unscaledDeltaTime))
            {
                _removeList.Add(timer);
            }
        }
        foreach (var timer in _removeList)
        {
            _coroutineList.Remove(timer);
        }
        _removeList.Clear();
    }
}
