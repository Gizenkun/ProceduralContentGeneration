using System;
using UnityEngine;

public class DugeonData : IDugeonData
{
    public int Seed;
    public int RoomCount;
    //public Vector2 MinRoomSize;
    //public Vector2 MaxRoomSize;
    public Vector2 Expectation;
    public Vector2 StandardDeviation;
    public Vector2 DugeonRange;
    public GameObject RoomPrefab;
    public GameObject RoomRoot;
    public Vector2 CellSize;
    public Vector2 MainRoomThreshold;
}
