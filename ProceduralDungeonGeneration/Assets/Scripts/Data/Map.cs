using System;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public List<Room> RoomList = new List<Room>();
    public List<Passage> DelaunayGraph = new List<Passage>();
    public List<Passage> PassageList = new List<Passage>();

    private List<Room> _mainRoomList = null;
    private List<Room> _corridorRoomList = null;
    private List<Room> _unuseRoomList = null;
    private List<Corridor> _corridorList = null;

    public List<Room> MainRoomList
    {
        get
        {
            if (RoomList == null)
            {
                _mainRoomList = null;
            }
            if(_mainRoomList == null)
            {
                _mainRoomList = RoomList.FindAll(room => room.RoomType == RoomType.Main);
            }
            return _mainRoomList;
        }
    }
    public List<Room> CorridorRoomList
    {
        get
        {
            if (RoomList == null)
            {
                _corridorRoomList = null;
            }
            if (_corridorRoomList == null)
            {
                _corridorRoomList = RoomList.FindAll(room => room.RoomType == RoomType.Corridor);
            }
            return _corridorRoomList;
        }
    }
    public List<Room> UnuseRoomList
    {
        get
        {
            if (RoomList == null)
            {
                _unuseRoomList = null;
            }
            if (_unuseRoomList == null)
            {
                _unuseRoomList = RoomList.FindAll(room => room.RoomType == RoomType.Unuse);
            }
            return _unuseRoomList;
        }
    }

    public List<Corridor> CorridorList
    {
        get
        {
            if(PassageList == null || RoomList == null)
            {
                return null;
            }
            if(_corridorList == null)
            {
                _corridorList = new List<Corridor>();
                foreach (Passage passage in PassageList)
                {
                    Room startRoom = MainRoomList[passage.StartRoomIndex];
                    Room endRoom = MainRoomList[passage.EndRoomIndex];

                    if (Mathf.Abs(startRoom.RoomPosition.x - endRoom.RoomPosition.x) < (startRoom.Width + endRoom.Width) / 2f)
                    {
                        //TODO : 创建一条竖直走廊
                        float middle = (startRoom.RoomPosition.x - endRoom.RoomPosition.x) / 2f;
                        int yDir = startRoom.RoomPosition.y > endRoom.RoomPosition.y ? 1 : -1;
                        Vector2 startPoint = new Vector2(startRoom.RoomPosition.x - middle, startRoom.RoomPosition.y - yDir * startRoom.Height / 2f);
                        Vector2 endPoint = new Vector2(endRoom.RoomPosition.x + middle, endRoom.RoomPosition.y + yDir * endRoom.Height / 2f);
                        _corridorList.Add(new Corridor() { Path = new Vector2[] { startPoint, endPoint } });
                    }
                    else if (Mathf.Abs(startRoom.RoomPosition.y - endRoom.RoomPosition.y) < (startRoom.Height + endRoom.Height) / 2f)
                    {
                        //TODO ： 创建一条水平走廊
                        float middle = (startRoom.RoomPosition.y - endRoom.RoomPosition.y) / 2f;
                        int xDir = startRoom.RoomPosition.x > endRoom.RoomPosition.x ? 1 : -1;
                        Vector2 startPoint = new Vector2(startRoom.RoomPosition.x - xDir * startRoom.Width / 2f, startRoom.RoomPosition.y - middle);
                        Vector2 endPoint = new Vector2(endRoom.RoomPosition.x + xDir * endRoom.Width / 2f, endRoom.RoomPosition.y + middle);
                        _corridorList.Add(new Corridor() { Path = new Vector2[] { startPoint, endPoint } });
                    }
                    else
                    {
                        //TODO ： 创建一个'L'形走廊
                        int xDir = startRoom.RoomPosition.x > endRoom.RoomPosition.x ? 1 : -1;
                        int yDir = startRoom.RoomPosition.y > endRoom.RoomPosition.y ? 1 : -1;
                        Vector2 startPoint = new Vector2(startRoom.RoomPosition.x - xDir * startRoom.Width / 2f, startRoom.RoomPosition.y);
                        Vector2 cornerPoint = new Vector2(endRoom.RoomPosition.x, startRoom.RoomPosition.y);
                        Vector2 endPoint = new Vector2(endRoom.RoomPosition.x, endRoom.RoomPosition.y + yDir * endRoom.Height / 2f);
                        Corridor corridor = new Corridor() { Path = new Vector2[] { startPoint, cornerPoint, endPoint } };
                        //bool corridorOverlap = false;
                        //foreach (Corridor c in _corridorList)
                        //{
                        //    if (c.OverlapCheck(corridor))
                        //    {
                        //        corridorOverlap = true;
                        //    }
                        //}
                        //if (corridorOverlap)
                        //{
                        //    startPoint = new Vector2(startRoom.RoomPosition.x, startRoom.RoomPosition.y - yDir * startRoom.Height / 2f);
                        //    cornerPoint = new Vector2(startRoom.RoomPosition.x, endRoom.RoomPosition.y);
                        //    endPoint = new Vector2(endRoom.RoomPosition.x + xDir * endRoom.Width / 2f, endRoom.RoomPosition.y);
                        //    corridor.Path = new Vector2[] { startPoint, cornerPoint, endPoint };
                        //}

                        bool roomOverlap = false;
                        foreach (Room room in MainRoomList)
                        {
                            if(room.OverlapCheck(corridor))
                            {
                                roomOverlap = true;
                            }
                        }
                        if(roomOverlap)
                        {
                            startPoint = new Vector2(startRoom.RoomPosition.x, startRoom.RoomPosition.y - yDir * startRoom.Height / 2f);
                            cornerPoint = new Vector2(startRoom.RoomPosition.x, endRoom.RoomPosition.y);
                            endPoint = new Vector2(endRoom.RoomPosition.x + xDir * endRoom.Width / 2f, endRoom.RoomPosition.y);
                            corridor.Path = new Vector2[] { startPoint, cornerPoint, endPoint };
                        }
                        _corridorList.Add(corridor);
                    }
                }

                foreach (Room room in UnuseRoomList)
                {
                    foreach (Corridor corridor in _corridorList)
                    {
                        if(room.OverlapCheck(corridor))
                        {
                            room.RoomType = RoomType.Corridor;
                            corridor.CorridorRoomList.Add(room);
                            room.RoomTransform.name += "_Corridor";
                        }
                    }
                }

                _unuseRoomList = null;
                _corridorRoomList = null;
            }

            return _corridorList;
        }
    }
}
