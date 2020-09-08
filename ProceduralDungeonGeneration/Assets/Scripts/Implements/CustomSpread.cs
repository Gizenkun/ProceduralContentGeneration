using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomSpread : ISpreadStrategy
{
    public void Spread(List<Room> roomList, Action<AsyncResult> callback)
    {
        if(roomList == null)
        {
            callback?.Invoke(AsyncResult.GetFailed("房间列表为空"));
        }
        foreach (Room room in roomList)
        {
            GameObject roomObj = room.RoomTransform.gameObject;
            BoxCollider2D collider = roomObj.AddComponent<BoxCollider2D>();
        }
        CoroutineHelper.Start(DoSpread(roomList, callback));
    }

    private IEnumerator DoSpread(List<Room> roomList, Action<AsyncResult> callback)
    {
        List<Room> sortRoomList = null;
        Vector2 repulsiveForce = Vector2.zero;

        int loopCount = Constant.LoopCount;
        while (loopCount > 0)
        {
            loopCount--;
            sortRoomList = roomList.OrderByDescending(room =>
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
                            Vector2 dir = sortRoomList[i].RoomPosition - sortRoomList[j].RoomPosition;
                            if (dir == Vector2.zero)
                            {
                                if (sortRoomList[i].Width > sortRoomList[j].Height)
                                {
                                    repulsiveForce.y += sortRoomList[i].RoomPosition.y > 0 ? 1f : -1f;
                                }
                                else
                                {
                                    repulsiveForce.x += sortRoomList[i].RoomPosition.x > 0 ? 1f : -1f;
                                }
                            }
                            else
                            {
                                repulsiveForce += dir;//Vector2.Scale(dir, new Vector2(sortRoomList[i].Width, sortRoomList[i].Height));
                            }
                        }
                    }
                }
                sortRoomList[i].RoomPosition += repulsiveForce.normalized;
                repulsiveForce = Vector2.zero;
            }
            yield return null;
            bool isOver = true;
            foreach (var room in roomList)
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
}
