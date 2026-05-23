using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public enum NodeType
    {
        Combat,
        Rest,
        Shop,
        Boss,
        Start,
        Mystery,
        Treasure
    }

    public NodeType nodeType;   // 노드 타입
    public NodeType mysteryActualType; // [추가] 물음표의 실제 타입 (입장 시 결정)
    public int x;               // 격자 x 좌표 (0~6)
    public int y;               // 격자 y 좌표 (0~14)
    public bool isCleared;      // 클리어 여부
    public bool isAccessible;   // 현재 접근 가능 여부
    public bool isVisited;      // 방문 여부

    public List<MapNode> nextNodes = new List<MapNode>(); // 다음 노드들 (y+1층)
    public List<MapNode> prevNodes = new List<MapNode>(); // 이전 노드들 (y-1층)

    public MapNode(int x, int y, NodeType type)
    {
        this.x = x;
        this.y = y;
        this.nodeType = type;
        isCleared = false;
        isAccessible = false;
        isVisited = false;
    }



}
