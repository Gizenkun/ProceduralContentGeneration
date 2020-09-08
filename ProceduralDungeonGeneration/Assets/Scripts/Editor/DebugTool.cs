using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
public class DebugTool : Editor
{
    static Vector2[] vertexs = new Vector2[10];
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawRooms(DugeonGenerator generator, GizmoType gizmoType)
    {
        if (generator != null && generator._map != null)
        {
            foreach (var room in generator._map.RoomList)
            {
                Vector2 size = new Vector2(room.Width, room.Height);
                Color faceColor;
                switch (room.RoomType)
                {
                    case RoomType.Main:
                        if (!generator._drawMainRoom) continue;
                        faceColor = Color.red;
                        break;
                    case RoomType.Corridor:
                        if (!generator._drawCorridorRoom) continue;
                        faceColor = Color.blue;
                        break;
                    case RoomType.Unuse:
                        if (!generator._drawUnuseRoom) continue;
                        faceColor = Color.grey;
                        break;
                    default:
                        faceColor = Color.grey;
                        break;
                }
                Handles.DrawSolidRectangleWithOutline(new Rect(room.RoomPosition - size / 2f, size), faceColor, Color.black);
            }
            if (generator._drawDelaunayGraph)
            {
                Gizmos.color = Color.red;
                foreach (var passage in generator._map.DelaunayGraph)
                {
                    Gizmos.DrawLine(generator._map.MainRoomList[passage.StartRoomIndex].RoomPosition, generator._map.MainRoomList[passage.EndRoomIndex].RoomPosition);
                }
            }
            if (generator._drawPassage)
            {
                Gizmos.color = Color.green;
                foreach (var passage in generator._map.PassageList)
                {
                    Gizmos.DrawLine(generator._map.MainRoomList[passage.StartRoomIndex].RoomPosition, generator._map.MainRoomList[passage.EndRoomIndex].RoomPosition);
                }
            }
            if (generator._drawCorridor)
            {
                Gizmos.color = Color.yellow;
                int index = 0;
                foreach (Corridor corridor in generator._map.CorridorList)
                {
                    for (int i = 0; i < corridor.Path.Length - 1; i++)
                    {
                        Vector2 startPoint = corridor.Path[i];
                        Vector2 endPoint = corridor.Path[i + 1];
                        if (Mathf.Abs(startPoint.x - endPoint.x) < Vector2.kEpsilon)
                        {
                            Handles.DrawSolidRectangleWithOutline(new Rect(startPoint.x - corridor.Width / 2f, startPoint.y, corridor.Width, endPoint.y - startPoint.y), Color.yellow, Color.black);
                            //Gizmos.DrawLine(startPoint, endPoint);
                        }
                        else if(Mathf.Abs(startPoint.y - endPoint.y) < Vector2.kEpsilon)
                        {
                            Handles.DrawSolidRectangleWithOutline(new Rect(startPoint.x, startPoint.y - corridor.Width / 2f, endPoint.x - startPoint.x, corridor.Width), Color.yellow, Color.black);
                            //Gizmos.DrawLine(startPoint, endPoint);
                        }
                    }
                    index++;
                }
            }
        //    foreach (Passage passage in generator._map.PassageList)
        //    {
        //        Room startRoom = generator._map.MainRoomList[passage.StartRoomIndex];
        //        Room endRoom = generator._map.MainRoomList[passage.EndRoomIndex];

        //        if (Mathf.Abs(startRoom.RoomPosition.x - endRoom.RoomPosition.x) < (startRoom.Width + endRoom.Width) / 2f)
        //        {
        //            //TODO : 创建一条竖直走廊
        //            float middle = (startRoom.RoomPosition.x - endRoom.RoomPosition.x) / 2f;
        //            int yDir = startRoom.RoomPosition.y > endRoom.RoomPosition.y ? 1 : -1;
        //            Vector2 startPoint = new Vector2(startRoom.RoomPosition.x - middle, startRoom.RoomPosition.y - yDir * startRoom.Height / 2f);
        //            Vector2 endPoint = new Vector2(endRoom.RoomPosition.x + middle, endRoom.RoomPosition.y + yDir * endRoom.Height / 2f);
        //            Gizmos.DrawLine(startPoint, endPoint);
        //        }
        //        else if (Mathf.Abs(startRoom.RoomPosition.y - endRoom.RoomPosition.y) < (startRoom.Height + endRoom.Height) / 2f)
        //        {
        //            //TODO ： 创建一条水平走廊
        //            float middle = (startRoom.RoomPosition.y - endRoom.RoomPosition.y) / 2f;
        //            int xDir = startRoom.RoomPosition.x > endRoom.RoomPosition.x ? 1 : -1;
        //            Vector2 startPoint = new Vector2(startRoom.RoomPosition.x - xDir * startRoom.Width / 2f, startRoom.RoomPosition.y - middle);
        //            Vector2 endPoint = new Vector2(endRoom.RoomPosition.x + xDir * endRoom.Width / 2f, endRoom.RoomPosition.y + middle);
        //            Gizmos.DrawLine(startPoint, endPoint);
        //        }
        //        else
        //        {
        //            //TODO ： 创建一个'L'形走廊
        //            int xDir = startRoom.RoomPosition.x > endRoom.RoomPosition.x ? 1 : -1;
        //            int yDir = startRoom.RoomPosition.y > endRoom.RoomPosition.y ? 1 : -1;
        //            Vector2 startPoint = new Vector2(startRoom.RoomPosition.x - xDir * startRoom.Width / 2f, startRoom.RoomPosition.y);
        //            Vector2 cornerPoint = new Vector2(endRoom.RoomPosition.x, startRoom.RoomPosition.y);
        //            Vector2 endPoint = new Vector2(endRoom.RoomPosition.x, endRoom.RoomPosition.y + yDir * endRoom.Height / 2f);
        //            Gizmos.DrawLine(startPoint, cornerPoint);
        //            Gizmos.DrawLine(cornerPoint, endPoint);
        //        }
        //    }
        }
        if (generator._drawGrid)
        {
            Gizmos.color = new Color(1, 1, 1, 0.5f);
            int gridRange = 1000;
            for (int i = -gridRange; i <= gridRange; i++)
            {
                if (i % 1 == 0)
                {
                    Gizmos.DrawLine(new Vector3(i, -gridRange, 0), new Vector3(i, gridRange, 0));
                    Gizmos.DrawLine(new Vector3(-gridRange, i, 0), new Vector3(gridRange, i, 0));
                }
            }
        }


        //Vector2 a = new Vector2(1, -3);
        //Vector2 b = new Vector2(-2, 1);
        //Vector2 c = new Vector2(3, 2);

        //Gizmos.DrawSphere(a, 0.2f);
        //Gizmos.DrawSphere(b, 0.2f);
        //Gizmos.DrawSphere(c, 0.2f);

        //Vector3 center = MathTool.GetCircumcircleCenter(a, b, c);
        //float radius = MathTool.GetCircumcircleRadius(a, b, c);
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(center, 0.2f);
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(center, radius);

        ////Vector2[] vertexs = new Vector2[] { a, b, c };
        //if (!generator.randomDrawPoint)
        //{
        //    for (int i = 0; i < vertexs.Length; i++)
        //    {
        //        vertexs[i] = Random.insideUnitCircle * 5.0f;
        //    }
        //    generator.randomDrawPoint = true;
        //}

        //Gizmos.color = Color.yellow;
        //Vector2[] superTriangle = MathTool.GetSuperTriangle(vertexs);
        //for (int i = 0; i < superTriangle.Length; i++)
        //{
        //    Gizmos.DrawLine(superTriangle[i], superTriangle[(i + 1) % superTriangle.Length]);
        //}

        //for (int i = 0; i < vertexs.Length; i++)
        //{
        //    Gizmos.DrawSphere(vertexs[i], 0.2f);
        //}
        //List<Vector3> triangles;
        //MathTool.Delaunay(vertexs, out triangles);
        //foreach (var triangle in triangles)
        //{
        //    Vector3 randomColor = Random.insideUnitSphere * 0.4f + Vector3.one * 0.6f;
        //    Gizmos.color = new Color(randomColor.x, randomColor.y, randomColor.z);
        //    Gizmos.DrawLine(vertexs[(int)triangle.x], vertexs[(int)triangle.y]);
        //    Gizmos.DrawLine(vertexs[(int)triangle.y], vertexs[(int)triangle.z]);
        //    Gizmos.DrawLine(vertexs[(int)triangle.z], vertexs[(int)triangle.x]);
        //}
    }
}
