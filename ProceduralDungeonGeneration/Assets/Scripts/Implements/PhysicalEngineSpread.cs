using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalEngineSpread : ISpreadStrategy
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
            collider.size = Vector2.one;
            Rigidbody2D rigi = roomObj.AddComponent<Rigidbody2D>();
            rigi.gravityScale = 0;
            rigi.freezeRotation = true;
        }
        CoroutineHelper.Start(CheckSpreadOver(roomList, result =>
        {
            foreach (Room room in roomList)
            {
                GameObject roomObj = room.RoomTransform.gameObject;
                UnityEngine.Object.Destroy(roomObj.GetComponent<BoxCollider2D>());
                UnityEngine.Object.Destroy(roomObj.GetComponent<Rigidbody2D>());
            }
            callback?.Invoke(result);
        }));
    }

    private IEnumerator CheckSpreadOver(List<Room> roomList, Action<AsyncResult> callback)
    {
        int loopCount = Constant.LoopCount;
        while (loopCount > 0)
        {
            loopCount--;
            bool isOver = true;
            foreach (var room in roomList)
            {
                if (!room.RoomTransform.GetComponent<Rigidbody2D>().IsSleeping())
                    isOver = false;
            }
            yield return null;
            if (isOver)
                break;
        }
        callback?.Invoke(AsyncResult.Success);
    }
}
