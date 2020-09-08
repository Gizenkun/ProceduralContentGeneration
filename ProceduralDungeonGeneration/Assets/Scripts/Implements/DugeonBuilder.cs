using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = System.Random;

public class DugeonBuilder : IDugeonBuilder
{
    private DugeonData _dugeonData;
    private Map _map = new Map();

    public void Create(IDugeonData dugeonData , Action<Map> completeCb)
    {
        _dugeonData = dugeonData as DugeonData;

        Random dugeonRandom = new Random(_dugeonData.Seed);
        _map = new Map();

        AsyncSequence aq = new AsyncSequence(result =>
        {
            if (result.IsSuccess)
            {
                Debug.Log("创建完成");
                completeCb?.Invoke(_map);
            }
            else
            {
                Debug.LogError($"创建失败 : {result.ErrorMsg}");
            }
        });

        aq.Add(callback =>
        {
            for (int i = 0; i < _dugeonData.RoomCount; i++)
            {
                Vector2 randomCenter = MathTool.GetRandomPointInEllipse(_dugeonData.DugeonRange.x, _dugeonData.DugeonRange.y, dugeonRandom, _dugeonData.CellSize * 2f);//MathTool.GetRandomPointInCircle(_dugeonData.DugeonRange.x, dugeonRandom, _dugeonData.CellSize);
                int randomWidth = (int)MathTool.Align(MathTool.NormalDistributionRandom(dugeonRandom, (int)_dugeonData.Expectation.x, (int)_dugeonData.StandardDeviation.x), _dugeonData.CellSize.x * 2f);//(int)MathTool.Align(dugeonRandom.Next((int)_dugeonData.MinRoomSize.x, (int)_dugeonData.MaxRoomSize.x), _dugeonData.CellSize.x * 2f);
                int randomHeight = (int)MathTool.Align(MathTool.NormalDistributionRandom(dugeonRandom, (int)_dugeonData.Expectation.y, (int)_dugeonData.StandardDeviation.y), _dugeonData.CellSize.y * 2f);//(int)MathTool.Align(dugeonRandom.Next((int)_dugeonData.MinRoomSize.y, (int)_dugeonData.MaxRoomSize.y), _dugeonData.CellSize.y * 2f);
                Room room = new Room(randomWidth, randomHeight);
                GameObject roomObj = new GameObject($"Room_{i}");
                roomObj.transform.parent = _dugeonData.RoomRoot.transform;
                roomObj.transform.position = randomCenter;
                roomObj.transform.localScale = new Vector3(randomWidth, randomHeight);
                room.RoomTransform = roomObj.transform;
                _map.RoomList.Add(room);
            }
            callback?.Invoke(AsyncResult.Success);
        });

        aq.Add(callback =>
        {
            ISpreadStrategy spreadStrategy = new PhysicalEngineSpread();//new CustomSpread();
            spreadStrategy.Spread(_map.RoomList, callback);
        });

        aq.Add(callback =>
        {
            CoroutineHelper.Start(Align(callback));
        });

        aq.Add(callback =>
        {
            float widthThreshold = (float)_map.RoomList.Average(room => room.Width) * _dugeonData.MainRoomThreshold.x;
            float heightThreshold = (float)_map.RoomList.Average(room => room.Height) * _dugeonData.MainRoomThreshold.y;

            foreach (var room in _map.RoomList)
            {
                if (room.Width > widthThreshold && room.Height > heightThreshold)
                {
                    room.RoomType = RoomType.Main;
                    room.RoomTransform.name += "_Main";
                }
            }
            callback?.Invoke(AsyncResult.Success);
        });

        aq.Add(callback =>
        {
            CreateRoomMap(dugeonRandom, callback);
        });

        aq.Do();
    }

    private IEnumerator Align(Action<AsyncResult> callback)
    {
        List<Room> sortRoomList = null;
        foreach (var room in _map.RoomList)
        {
            room.RoomPosition = new Vector2(MathTool.Align(room.RoomPosition.x, _dugeonData.CellSize.x * 2f), MathTool.Align(room.RoomPosition.y, _dugeonData.CellSize.y * 2f));
        }
        int loopCount = Constant.LoopCount;
        while (loopCount > 0)
        {
            loopCount--;
            sortRoomList = _map.RoomList.OrderByDescending(room =>
            {
                return Vector2.Dot(room.RoomPosition, room.RoomPosition);
            }).ToList();

            for (int i = 0; i < sortRoomList.Count; i++)
            {
                for (int j = 0; j < sortRoomList.Count; j++)
                {
                    if (i != j)
                    {
                        if (sortRoomList[i].OverlapCheck(sortRoomList[j]))
                        {
                            Vector2 dir = Vector2.zero;
                            if (sortRoomList[i].RoomPosition.x > 0)
                            {
                                if (sortRoomList[j].RoomPosition.x < sortRoomList[i].RoomPosition.x)
                                {
                                    dir.x = sortRoomList[i].RoomPosition.x - sortRoomList[j].RoomPosition.x;
                                }
                                else if (Mathf.Abs(sortRoomList[j].RoomPosition.x - sortRoomList[i].RoomPosition.x) < Vector2.kEpsilon)
                                {
                                    dir.x = _dugeonData.CellSize.x;
                                }
                            }
                            else
                            {
                                if (sortRoomList[j].RoomPosition.x > sortRoomList[i].RoomPosition.x)
                                {
                                    dir.x = sortRoomList[i].RoomPosition.x - sortRoomList[j].RoomPosition.x;
                                }
                                else if (Mathf.Abs(sortRoomList[j].RoomPosition.x - sortRoomList[i].RoomPosition.x) < Vector2.kEpsilon)
                                {
                                    dir.x = -_dugeonData.CellSize.x;
                                }
                            }

                            if (sortRoomList[i].RoomPosition.y > 0)
                            {
                                if (sortRoomList[j].RoomPosition.y < sortRoomList[i].RoomPosition.y)
                                {
                                    dir.y = sortRoomList[i].RoomPosition.y - sortRoomList[j].RoomPosition.y;
                                }
                                else if (Mathf.Abs(sortRoomList[j].RoomPosition.y - sortRoomList[i].RoomPosition.y) < Vector2.kEpsilon)
                                {
                                    dir.y = _dugeonData.CellSize.y;
                                }
                            }
                            else
                            {
                                if (sortRoomList[j].RoomPosition.y > sortRoomList[i].RoomPosition.y)
                                {
                                    dir.y = sortRoomList[i].RoomPosition.y - sortRoomList[j].RoomPosition.y;
                                }
                                else if (Mathf.Abs(sortRoomList[j].RoomPosition.y - sortRoomList[i].RoomPosition.y) < Vector2.kEpsilon)
                                {
                                    dir.y = -_dugeonData.CellSize.y;
                                }
                            }
                            if (dir != Vector2.zero)
                            {
                                if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                                {
                                    dir.x = (dir.x > 0 ? _dugeonData.CellSize.x : -_dugeonData.CellSize.x);
                                    dir.y = 0;
                                }
                                else
                                {
                                    dir.y = (dir.y > 0 ? _dugeonData.CellSize.y : -_dugeonData.CellSize.y);
                                    dir.x = 0;
                                }
                            }
                            sortRoomList[i].RoomPosition += dir;
                        }
                    }
                }
            }
            yield return null;
            bool isOver = true;
            foreach (var room in _map.RoomList)
            {
                if (!room.IsStatic)
                    isOver = false;
            }
            if (isOver)
            {
                break;
            }
        }
        callback?.Invoke(AsyncResult.Success);
    }

    private void CreateRoomMap(Random random, Action<AsyncResult> callback)
    {
        List<Vector3> triLinks;
        MathTool.Delaunay(_map.MainRoomList.Select(room => new Vector2(room.RoomPosition.x, room.RoomPosition.y)).ToArray(), out triLinks);

        if(triLinks == null)
        {
            callback?.Invoke(AsyncResult.GetFailed("三角剖分失败！"));
            return;
        }

        foreach (var link in triLinks)
        {
            for (int i = 0; i < 3; i++)
            {
                int from = i;
                int to = (i + 1) % 3;
                if (!_map.DelaunayGraph.Exists(item =>
                {
                    return (item.StartRoomIndex == link[from] && item.EndRoomIndex == link[to])
                    || (item.StartRoomIndex == link[to] && item.EndRoomIndex == link[from]);
                }))
                {
                    Passage passage = new Passage()
                    {
                        StartRoomIndex = (int)link[from],
                        EndRoomIndex = (int)link[to],
                        Distance = Vector2.Distance(_map.MainRoomList[(int)link[from]].RoomPosition, _map.MainRoomList[(int)link[to]].RoomPosition)
                    };
                    _map.DelaunayGraph.Add(passage);
                }
            }
        }

        _map.PassageList = _map.DelaunayGraph.MST();

        List<Passage> loopPassage = _map.DelaunayGraph.FindAll(passage1 =>
        !_map.PassageList.Exists(passage2 => (passage1.StartRoomIndex == passage2.StartRoomIndex && passage1.EndRoomIndex == passage2.EndRoomIndex)
        || (passage1.EndRoomIndex == passage2.StartRoomIndex && passage1.StartRoomIndex == passage2.EndRoomIndex)));

        int loopCount = (int)(loopPassage.Count * 0.1f);
        Debug.Log(loopCount);
        for (int i = 0; i < loopCount; i++)
        {
            _map.PassageList.Add(loopPassage[random.Next(0, loopPassage.Count)]);
            loopPassage.RemoveAt(random.Next(0, loopPassage.Count));
        }

        callback?.Invoke(AsyncResult.Success);
    }
}
