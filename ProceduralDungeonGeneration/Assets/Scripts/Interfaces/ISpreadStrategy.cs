using System;
using System.Collections.Generic;

public interface ISpreadStrategy
{
    void Spread(List<Room> roomList, Action<AsyncResult> callback);
}