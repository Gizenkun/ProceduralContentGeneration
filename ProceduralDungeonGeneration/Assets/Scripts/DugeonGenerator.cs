using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//1.根据正态分布随机生成房间
//2.将房间向外扩散直到没有房间重叠
//3.随机挑选出主房间
//4.连接所有主房间
//5.求出最小生成树
//6.随机添加一些环形回路

public class DugeonGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject _roomPrefab;
    [SerializeField]
    private GameObject _roomRoot;

    public Map _map;

    public bool _randomDrawPoint = false;
    public bool _drawMainRoom = true;
    public bool _drawCorridorRoom = true;
    public bool _drawUnuseRoom = true;
    public bool _drawDelaunayGraph = true;
    public bool _drawPassage = true;
    public bool _drawCorridor = true;
    public bool _drawGrid = true;

    public AnimationCurve _curve = new AnimationCurve();

    private IDugeonBuilder _builder;

    private void Awake()
    {
        _builder = new DugeonBuilder();
        GenerationDugeon(25, new Vector2(60, 10));
        System.Random random = new System.Random();
        Dictionary<int, int> randomDic = new Dictionary<int, int>();
        for (int i = 0; i < 1000; i++)
        {
            float r = MathTool.NormalDistributionRandom(random, 0, 10);
            if(randomDic.ContainsKey((int)r))
            {
                randomDic[(int)r]++;
            }
            else
            {
                randomDic[(int)r] = 1;
            }
        }

        foreach (var item in randomDic)
        {
            _curve.AddKey(item.Key, item.Value);
        }
    }

    public void GenerationDugeon(int seed, Vector2 range)
    {
        if(_roomRoot == null)
            _roomRoot = new GameObject("RoomRoot");
        DugeonData dugeonData = new DugeonData()
        {
            Seed = seed,
            RoomCount = 100,
            Expectation = new Vector2(10, 10),
            StandardDeviation = new Vector2(3, 3),
            DugeonRange = range,
            RoomPrefab = _roomPrefab,
            RoomRoot = _roomRoot,
            CellSize = new Vector2(1, 1),
            MainRoomThreshold = new Vector2(1.25f, 1f)
        };
        _builder.Create(dugeonData, result =>
        {
            _map = result;
        });
    }
}
