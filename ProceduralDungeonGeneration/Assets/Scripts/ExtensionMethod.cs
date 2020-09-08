using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExtensionMethod
{
    public static bool OverlapCheck(this Room room, Room other)
    {
        float xDist = Math.Abs(room.RoomPosition.x - other.RoomPosition.x);
        float yDist = Math.Abs(room.RoomPosition.y - other.RoomPosition.y);

        if (xDist < (room.Width * 0.5 + other.Width * 0.5) && yDist < (room.Height * 0.5 + other.Height * 0.5))
            return true;
        return false;
    }

    public static bool OverlapCheck(this Room room, Corridor corridor)
    {
        for (int i = 0; i < corridor.Path.Length - 1; i++)
        {
            Vector2 point1 = corridor.Path[i];
            Vector2 point2 = corridor.Path[i + 1];

            if (Mathf.Abs(point1.x - point2.x) < Vector2.kEpsilon)
            {
                if ((Mathf.Abs(point1.x - room.RoomPosition.x) - corridor.Width / 2f < room.Width / 2f)
                    && (((point1.y > room.RoomPosition.y - room.Height / 2f)
                    && (point2.y < room.RoomPosition.y + room.Height / 2f))
                    || ((point2.y > room.RoomPosition.y - room.Height / 2f)
                    && (point1.y < room.RoomPosition.y + room.Height / 2f))))
                {
                    return true;
                }
            }
            else if (Mathf.Abs(point1.y - point2.y) < Vector2.kEpsilon)
            {
                if (Mathf.Abs(point1.y - room.RoomPosition.y) - corridor.Width / 2f < room.Height / 2f
                   && (((point1.x > room.RoomPosition.x - room.Width / 2f)
                   && (point2.x < room.RoomPosition.x + room.Width / 2f))
                   || ((point2.x > room.RoomPosition.x - room.Width / 2f)
                   && (point1.x < room.RoomPosition.x + room.Width / 2f))))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool OverlapCheck(this Corridor corridor, Corridor other)
    {
        bool isOverlap = false;

        for (int i = 0; i < corridor.Path.Length - 1; i++)
        {
            Vector2 thisPoint1 = corridor.Path[i];
            Vector2 thisPoint2 = corridor.Path[i + 1];
            for (int j = 0; j < other.Path.Length - 1; j++)
            {
                Vector2 otherPoint1 = other.Path[j];
                Vector2 otherPoint2 = other.Path[j + 1];

                if(Mathf.Abs(thisPoint1.x - thisPoint2.x) < Vector2.kEpsilon)
                {
                    if(Mathf.Abs(otherPoint1.x - otherPoint2.x) < Vector2.kEpsilon)
                    {
                        if(Mathf.Abs(thisPoint1.x - otherPoint1.x) < (corridor.Width + other.Width) / 2f)
                        {
                            if(!((Mathf.Max(thisPoint1.y, thisPoint2.y) < Mathf.Min(otherPoint1.y, otherPoint2.y)) 
                                || (Mathf.Min(thisPoint1.y, thisPoint2.y) < Mathf.Max(otherPoint1.y, otherPoint2.y))))
                            {
                                isOverlap = true;
                            }
                        }
                    }
                    else
                    {
                        if((thisPoint1.x + corridor.Width / 2f) > Mathf.Min(otherPoint1.x, otherPoint2.x) 
                            && (thisPoint2.x - corridor.Width / 2f < Mathf.Max(otherPoint1.x, otherPoint2.x)))
                        {
                            if((otherPoint1.y + other.Width / 2f) > Mathf.Min(thisPoint1.y, thisPoint2.y)
                            && (otherPoint2.y - other.Width / 2f < Mathf.Max(thisPoint1.y, thisPoint2.y)))
                            {
                                isOverlap = true;
                            }
                        }
                    }
                }
                else
                {
                    if (Mathf.Abs(otherPoint1.y - otherPoint2.y) < Vector2.kEpsilon)
                    {
                        if (Mathf.Abs(thisPoint1.y - otherPoint1.y) < (corridor.Width + other.Width) / 2f)
                        {
                            if (!((Mathf.Max(thisPoint1.x, thisPoint2.x) < Mathf.Min(otherPoint1.x, otherPoint2.x))
                                || (Mathf.Min(thisPoint1.x, thisPoint2.x) < Mathf.Max(otherPoint1.x, otherPoint2.x))))
                            {
                                isOverlap = true;
                            }
                        }
                    }
                    else
                    {
                        if ((thisPoint1.y + corridor.Width / 2f) > Mathf.Min(otherPoint1.y, otherPoint2.y)
                            && (thisPoint2.y - corridor.Width / 2f < Mathf.Max(otherPoint1.y, otherPoint2.y)))
                        {
                            if ((otherPoint1.x + other.Width / 2f) > Mathf.Min(thisPoint1.x, thisPoint2.x)
                            && (otherPoint2.x - other.Width / 2f < Mathf.Max(thisPoint1.x, thisPoint2.x)))
                            {
                                isOverlap = true;
                            }
                        }
                    }
                }
            }
        }

        return isOverlap;
    }

    //获得最小生成树 Prim
    public static List<Passage> MST(this List<Passage> graph)
    {
        List<int> pointList = graph.Select(passage => passage.StartRoomIndex).ToList();
        pointList.AddRange(graph.Select(passage => passage.EndRoomIndex));
        pointList = pointList.Distinct().ToList();

        List<Passage> edgeList = new List<Passage>();
        Passage edge = graph.Min(passage => passage);
        int loopCount = Constant.LoopCount;
        while (loopCount > 0)
        {
            loopCount--;
            edgeList.Add(edge);
            pointList.Remove(edge.StartRoomIndex);
            pointList.Remove(edge.EndRoomIndex);
            edge = graph.FindAll(passage =>
            {
                return (!pointList.Exists(point => point == passage.StartRoomIndex) && pointList.Exists(point => point == passage.EndRoomIndex))
                || (!pointList.Exists(point => point == passage.EndRoomIndex) && pointList.Exists(point => point == passage.StartRoomIndex));
            })?.Min(passage => passage);

            if (edge == null)
                break;
        }

        return edgeList;
    }
}

public class ExtensionCompare<T> : IComparer<T>
{
    private Func<T, T, int> _compareFunc;
    public ExtensionCompare(Func<T, T, int> compareFunc)
    {
        _compareFunc = compareFunc;
    }

    int IComparer<T>.Compare(T x, T y)
    {
        if (_compareFunc == null) throw new Exception("比较委托为空！");
        return _compareFunc.Invoke(x, y);
    }
}