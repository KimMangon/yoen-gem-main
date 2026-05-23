using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("맵 설정")]
    public int width = 7;
    public int height = 15;
    public int pathCount = 6;

    [Header("노드 타입 확률")]
    public float shopChance = 0.10f;
    public float restChance = 0.13f;


    private MapNode[,] grid;
    public List<MapNode> allNodes = new List<MapNode>();

    public List<MapNode> GenerateMap()
    {
        grid = new MapNode[width, height];
        allNodes.Clear();

        List<List<Vector2Int>> paths = new List<List<Vector2Int>>();
        for (int i = 0; i < pathCount; i++)
            paths.Add(GeneratePath());

        HashSet<Vector2Int> nodePositions = new HashSet<Vector2Int>();
        foreach (var path in paths)
            foreach (var pos in path)
                nodePositions.Add(pos);

        // 마지막 행 제외하고 노드 생성
        foreach (var pos in nodePositions)
        {
            if (pos.y == height - 1) continue; // [변경] 마지막 행은 건너뜀
            MapNode.NodeType type = AssignNodeType(pos.y);
            MapNode node = new MapNode(pos.x, pos.y, type);
            grid[pos.x, pos.y] = node;
            allNodes.Add(node);
        }

        // 0행 시작 노드 고정
        int startX = width / 2;
        MapNode startNode = new MapNode(startX, 0, MapNode.NodeType.Start);
        startNode.isAccessible = true;
        grid[startX, 0] = startNode;
        allNodes.Add(startNode);

        foreach (var node in allNodes)
        {
            if (node.y == height - 2) node.nodeType = MapNode.NodeType.Shop; // [추가] 보스 전 행 상점 고정
        }

        //6층 노드들 보물방으로 고정
        foreach (var node in allNodes)
        {
            if (node.y == 6) node.nodeType = MapNode.NodeType.Treasure;
        }

        // 마지막 행 보스 노드 하나로 통합 (중앙 x)
        int bossX = width / 2;
        MapNode bossNode = new MapNode(bossX, height - 1, MapNode.NodeType.Boss);
        grid[bossX, height - 1] = bossNode;
        allNodes.Add(bossNode);

        // 경로 연결
        foreach (var path in paths)
        {
            // 경로 첫 노드(y=1)를 시작노드와 연결
            if (path.Count > 0)
            {
                Vector2Int first = path[0];
                MapNode firstNode = grid[first.x, first.y];
                if (firstNode != null)
                {
                    if (!startNode.nextNodes.Contains(firstNode))
                        startNode.nextNodes.Add(firstNode);
                    if (!firstNode.prevNodes.Contains(startNode))
                        firstNode.prevNodes.Add(startNode);
                }
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2Int cur = path[i];
                Vector2Int next = path[i + 1];

                // 마지막 행은 보스 노드로 연결
                if (next.y == height - 1)
                {
                    MapNode curNode = grid[cur.x, cur.y];
                    if (curNode != null)
                    {
                        if (!curNode.nextNodes.Contains(bossNode))
                            curNode.nextNodes.Add(bossNode);
                        if (!bossNode.prevNodes.Contains(curNode))
                            bossNode.prevNodes.Add(curNode);
                    }
                    continue;
                }

                MapNode cn = grid[cur.x, cur.y];
                MapNode nn = grid[next.x, next.y];
                if (cn != null && nn != null)
                {
                    if (!cn.nextNodes.Contains(nn)) cn.nextNodes.Add(nn);
                    if (!nn.prevNodes.Contains(cn)) nn.prevNodes.Add(cn);
                }
            }
        }

        // 0행 접근 가능
        foreach (var node in allNodes)
            if (node.y == 0) node.isAccessible = true;

        return allNodes;
    }

    List<Vector2Int> GeneratePath()
    {
        List<Vector2Int> path = new List<Vector2Int>();
        int x = Random.Range(0, width);

        for (int y = 1; y < height; y++)
        {
            path.Add(new Vector2Int(x, y));
            x += Random.Range(-1, 2);
            x = Mathf.Clamp(x, 0, width - 1);
        }
        return path;
    }

    MapNode.NodeType AssignNodeType(int y)
    {
        float rand = Random.Range(0f, 1f);
        if (rand < shopChance) return MapNode.NodeType.Shop;
        if (rand < shopChance + restChance) return MapNode.NodeType.Rest;
        if (rand < shopChance + restChance + 0.20f) return MapNode.NodeType.Mystery;
        return MapNode.NodeType.Combat;
    }
}