using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MathTool
{
    //获取三角形外接圆
    public static Circle GetCircumcircle(Vector2 a, Vector2 b, Vector2 c)
    {
        float fabsy1y2 = Mathf.Abs(a.y - b.y);
        float fabsy2y3 = Mathf.Abs(b.y - c.y);

        float[,] det1 = new float[3, 3] { { Mathf.Pow(a.x, 2) + Mathf.Pow(a.y, 2), a.y, 1f },
            { Mathf.Pow(b.x, 2) + Mathf.Pow(b.y, 2), b.y, 1f },
            { Mathf.Pow(c.x, 2) + Mathf.Pow(c.y, 2), c.y, 1f } };

        float[,] det2 = new float[3, 3] { { a.x, Mathf.Pow(a.x, 2) + Mathf.Pow(a.y, 2), 1f },
            { b.x, Mathf.Pow(b.x, 2) + Mathf.Pow(b.y, 2), 1f },
            { c.x, Mathf.Pow(c.x, 2) + Mathf.Pow(c.y, 2), 1f } };

        float[,] det3 = new float[3, 3] { { a.x, a.y, 1f },
            { b.x, b.y, 1f },
            { c.x, c.y, 1f } };

        float x = CalculateDet(det1) / (2 * CalculateDet(det3));
        float y = CalculateDet(det2) / (2 * CalculateDet(det3));

        Vector2 center = new Vector2(x, y);
        float radius = Vector2.Distance(a, center);

        return new Circle() { Center = center, Radius = radius};
    }

    //计算行列式
    public static float CalculateDet(float[,] det)
    {
        float result = 0;
        if(det.GetLength(0) == 1 && det.GetLength(1) == 1)
        {
            return det[0, 0];
        }
        for (int i = 0; i < det.GetLength(1); i++)
        {
            result += Mathf.Pow(-1, i) * det[0, i] * CalculateDet(Cofactor(det, 0, i));
        }
        return result;
    }

    //余子式
    private static float[,] Cofactor(float[,] det, int i, int j)
    {
        float[,] cofactorDet = new float[det.GetLength(0) - 1, det.GetLength(1) - 1];
        int xIndex = 0;
        for (int x = 0; x < det.GetLength(0); x++)
        {
            if (x == i)
                continue;
            int yIndex = 0;
            for (int y = 0; y < det.GetLength(1); y++)
            {
                if (y == j)
                    continue;
                cofactorDet[xIndex, yIndex] = det[x, y];
                yIndex++;
            }
            xIndex++;
        }
        return cofactorDet;
    }

    //计算点集超级三角形
    public static Vector2[] GetSuperTriangle(Vector2[] vertexs)
    {
        if (vertexs == null || vertexs.Length < 3) return null;

        float minX = vertexs[0].x;
        float minY = vertexs[0].y;
        float maxX = vertexs[0].x;
        float maxY = vertexs[0].y;

        foreach (var vertex in vertexs)
        {
            minX = Mathf.Min(minX, vertex.x);
            minY = Mathf.Min(minY, vertex.y);
            maxX = Mathf.Max(maxX, vertex.x);
            maxY = Mathf.Max(maxY, vertex.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        Vector2[] superTriangle = new Vector2[3];
        superTriangle[0] = new Vector2(minX - width * 0.5f - 1.0f, minY - 1.0f);
        superTriangle[1] = new Vector2(minX + width * 0.5f, maxY + height + 1.0f);
        superTriangle[2] = new Vector2(maxX + width * 0.5f + 1.0f, minY - 1.0f);

        return superTriangle;
    }

    #region Delaunay三角剖分

    public static void Delaunay_Origin(Vector2[] vertexs, out List<Vector3> triangles)
    {
        if (vertexs == null || vertexs.Length < 3)
        {
            triangles = null;
            return;
        }
        else if (vertexs.Length == 3)
        {
            triangles = new List<Vector3>();
            triangles.Add(new Vector3(0, 1, 2));
            return;
        }

        triangles = new List<Vector3>();

        Vector2[] superTriangle = GetSuperTriangle(vertexs);

    }

    public static void Delaunay(Vector2[] vertexs, out List<Vector3> triangles)
    {
        if (vertexs == null || vertexs.Length < 3)
        {
            triangles = null;
            return;
        }else if(vertexs.Length == 3)
        {
            triangles = new List<Vector3>();
            triangles.Add(new Vector3(0, 1, 2));
            return;
        }

        List<int> sortIndices = new List<int>();
        for (int i = 0; i < vertexs.Length; i++)
        {
            sortIndices.Add(i);
        }
        for (int i = 0; i < sortIndices.Count - 1; i++)
        {
            for (int j = i + 1; j < sortIndices.Count; j++)
            {
                if(vertexs[sortIndices[i]].x > vertexs[sortIndices[j]].x)
                {
                    int temp = sortIndices[i];
                    sortIndices[i] = sortIndices[j];
                    sortIndices[j] = temp;
                }
            }
        }

        List<Vector2> tempVertexs = new List<Vector2>(vertexs);
        List<Vector3> tempTriangles = new List<Vector3>();
        List<Vector2> tempEdges = new List<Vector2>();
        triangles = new List<Vector3>();

        Vector2[] superTriangle = GetSuperTriangle(vertexs);
        tempVertexs.AddRange(superTriangle);
        tempTriangles.Add(new Vector3(vertexs.Length, vertexs.Length + 1, vertexs.Length + 2));

        List<Vector3> removeTriangles = new List<Vector3>();
        for (int i = 0; i < sortIndices.Count; i++)
        {
            foreach (var triangle in tempTriangles)
            {
                Circle circumcircle = GetCircumcircle(tempVertexs[(int)triangle.x], tempVertexs[(int)triangle.y], tempVertexs[(int)triangle.z]);
                Vector2 center = circumcircle.Center;
                float radius = circumcircle.Radius;
                if (tempVertexs[sortIndices[i]].x - center.x > 0.0 &&  tempVertexs[sortIndices[i]].x - center.x > radius)
                {
                    triangles.Add(triangle);
                    removeTriangles.Add(triangle);
                    continue;
                }else if(Vector2.Distance(tempVertexs[sortIndices[i]], center) > radius)
                {
                    continue;
                }

                tempEdges.Add(new Vector2(triangle.x, triangle.y));
                tempEdges.Add(new Vector2(triangle.y, triangle.z));
                tempEdges.Add(new Vector2(triangle.z, triangle.x));

                removeTriangles.Add(triangle);
            }

            foreach (var triangle in removeTriangles)
            {
                tempTriangles.Remove(triangle);
            }
            removeTriangles.Clear();

            tempEdges = tempEdges.FindAll(
                temp1 => tempEdges.FindAll(
                    temp2 => (temp1.x == temp2.x && temp1.y == temp2.y) || (temp1.x == temp2.y && temp1.y == temp2.x)).Count == 1);
            foreach (var edge in tempEdges)
            {
                tempTriangles.Add(new Vector3(edge.x, edge.y, sortIndices[i]));
            }
            tempEdges.Clear();
        }

        triangles.AddRange(tempTriangles);
        triangles = triangles.FindAll(temp => (temp.x < vertexs.Length) && (temp.y < vertexs.Length) && (temp.z < vertexs.Length));
    }

    public static void Delaunay_DAC(Vector2[] vertexs, out List<Vector3> triangles)
    {
        if (vertexs == null || vertexs.Length < 3)
        {
            triangles = null;
            return;
        }
        else if (vertexs.Length == 3)
        {
            triangles = new List<Vector3>();
            triangles.Add(new Vector3(0, 1, 2));
            return;
        }

        //TODO:顶点索引按x坐标排序
        //TODO:分治逻辑
        triangles = null;
    }

    private List<Vector3> DoDelaunayDAC(Vector2[] vertexs1, Vector2[] vertexs2)
    {
        List<Vector3> triangles1;
        List<Vector3> triangles2;

        int length1 = vertexs1.Length;
        int length2 = vertexs2.Length;
        if (length1 > 3)
        {
            triangles1 = DoDelaunayDAC(vertexs1.Skip(0).Take(length1 / 2).ToArray(), vertexs1.Skip(length1 / 2).Take((length1 + 1) / 2).ToArray());
        }
        if(length2 > 3)
        {
            triangles2 = DoDelaunayDAC(vertexs2.Skip(0).Take(length2 / 2).ToArray(), vertexs2.Skip(length2 / 2).Take((length2 + 1) / 2).ToArray());
        }
        //TODO: 合并逻辑
        return null;
    }

    #endregion

    public static float Align(float n, float m)
    {
        return Mathf.FloorToInt((n + m - 1) / m) * m;
    }

    //获取圆范围内的随机位置
    public static Vector2 GetRandomPointInCircle(float radius, System.Random random, Vector2 cellSize)
    {
        float t = 2 * Mathf.PI * (float)random.NextDouble();
        float u = (float)random.NextDouble() + (float)random.NextDouble();
        float r;
        if (u > 1)
            r = 2 - u;
        else
            r = u;

        return new Vector2(Align(radius * r * Mathf.Cos(t), cellSize.x), Align(radius * r * Mathf.Sin(t), cellSize.y));
    }

    //获取椭圆范围内的随机位置
    public static Vector2 GetRandomPointInEllipse(float majorAxis, float minorAxis, System.Random random, Vector2 cellSize)
    {
        float t = 2 * Mathf.PI * (float)random.NextDouble();
        float u = (float)random.NextDouble() + (float)random.NextDouble();
        float r;
        if (u > 1)
            r = 2 - u;
        else
            r = u;

        return new Vector2(Align(majorAxis * r * Mathf.Cos(t) / 2f, cellSize.x), Align(minorAxis * r * Mathf.Sin(t) / 2f, cellSize.y));
    }

    //正态随机分布 Box-Muller算法 e期望 s标准差
    public static float NormalDistributionRandom(System.Random random, float e, float s)
    {
        float x = (float)random.NextDouble();
        float y = (float)random.NextDouble();

        float r = Mathf.Cos(2 * Mathf.PI * x) * Mathf.Sqrt(-2 * Mathf.Log(y));
        return r * s + e;
    }
}
