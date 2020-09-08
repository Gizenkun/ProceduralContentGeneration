using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Unuse = 0,
    Main,
    Corridor
}

public class Room
{
    public Transform RoomTransform;
    public RoomType RoomType = RoomType.Unuse;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2 RoomPosition
    {
        get
        {
            if (RoomTransform == null) return Vector2.zero;
            return new Vector2(RoomTransform.position.x, RoomTransform.position.y);
        }
        set
        {
            if (RoomTransform == null) return;
            RoomTransform.position = value;
        }
    }

    private float checkInterval = 0.1f;
    private float lastTime = 0;
    private Vector2 lastPos = Vector2.zero;
    public bool IsStatic
    {
        get
        {
            bool isStatic = false;
            if(Time.realtimeSinceStartup - lastTime > checkInterval)
            {
                if((RoomPosition - lastPos).sqrMagnitude < Vector2.kEpsilon)
                {
                    isStatic = true;
                }
                lastTime = Time.realtimeSinceStartup;
                lastPos = RoomPosition;
            }
            return isStatic;
        }
    }
    public Room(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
