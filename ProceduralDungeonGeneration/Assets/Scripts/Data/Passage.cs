using System;
using System.Collections.Generic;

//目前通路是无向的
public class Passage : IComparable<Passage>
{
    public int StartRoomIndex;
    public int EndRoomIndex;
    public float Distance;

    public int CompareTo(Passage other)
    {
        return Distance.CompareTo(other.Distance);
    }
}
