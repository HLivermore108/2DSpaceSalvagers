using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    private static readonly Vector2Int[] EvenRowNeighborOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    private static readonly Vector2Int[] OddRowNeighborOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    [Header("Grid Settings")]
    public GameObject hexTilePrefab;
    public Transform boardParent;
    public int width = 14;
    public int height = 18;
    public float xSpacing = 1f;
    public float ySpacing = 0.87f;

    [Header("Paint Settings")]
    public bool editMode = true;
    public PaintMode currentPaintMode = PaintMode.ToggleActive;

    private readonly List<HexTile> allTiles = new List<HexTile>();
    private readonly Dictionary<Vector2Int, HexTile> tilesByCoordinate = new Dictionary<Vector2Int, HexTile>();

    public enum PaintMode
    {
        ToggleActive,
        Normal,
        PlayerStart1,
        PlayerStart2,
        PlayerStart3,
        PlayerStart4,
        T1,
        T2,
        T3,
        Retaliator
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateBoard();
    }

    private void Update()
    {
        HandleHotkeys();
    }

    private void GenerateBoard()
    {
        if (allTiles.Count > 0)
        {
            return;
        }

        for (int r = 0; r < height; r++)
        {
            for (int q = 0; q < width; q++)
            {
                float x = q * xSpacing + (r % 2 == 1 ? xSpacing * 0.5f : 0f);
                float y = -r * ySpacing;

                GameObject tileObj = Instantiate(hexTilePrefab, new Vector3(x, y, 0f), Quaternion.identity, boardParent);
                tileObj.name = $"Hex ({q},{r})";

                HexTile tile = tileObj.GetComponent<HexTile>();
                tile.q = q;
                tile.r = r;

                tile.RefreshVisual();

                allTiles.Add(tile);
                tilesByCoordinate[new Vector2Int(q, r)] = tile;
            }
        }
    }

    public void PaintTile(HexTile tile)
    {
        if (!editMode || tile == null) return;

        switch (currentPaintMode)
        {
            case PaintMode.ToggleActive:
                tile.isActiveTile = !tile.isActiveTile;
                break;

            case PaintMode.Normal:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.Normal;
                break;

            case PaintMode.PlayerStart1:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.PlayerStart1;
                break;

            case PaintMode.PlayerStart2:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.PlayerStart2;
                break;

            case PaintMode.PlayerStart3:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.PlayerStart3;
                break;

            case PaintMode.PlayerStart4:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.PlayerStart4;
                break;

            case PaintMode.T1:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.T1;
                break;

            case PaintMode.T2:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.T2;
                break;

            case PaintMode.T3:
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.T3;
                break;

            case PaintMode.Retaliator:
                SetUniqueTileType(tile, HexTile.TileType.Retaliator);
                break;
        }

        tile.RefreshVisual();
    }

    private void SetUniqueTileType(HexTile tile, HexTile.TileType type)
    {
        foreach (var t in allTiles)
        {
            if (t.tileType == type)
            {
                t.tileType = HexTile.TileType.Normal;
                t.RefreshVisual();
            }
        }

        tile.isActiveTile = true;
        tile.tileType = type;
    }

    public HexTile GetTile(int q, int r)
    {
        tilesByCoordinate.TryGetValue(new Vector2Int(q, r), out HexTile tile);
        return tile;
    }

    public List<HexTile> GetNeighbors(HexTile tile)
    {
        List<HexTile> neighbors = new List<HexTile>();

        if (tile == null)
        {
            return neighbors;
        }

        Vector2Int[] offsets = tile.r % 2 == 0 ? EvenRowNeighborOffsets : OddRowNeighborOffsets;

        foreach (Vector2Int offset in offsets)
        {
            HexTile neighbor = GetTile(tile.q + offset.x, tile.r + offset.y);

            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public List<HexTile> GetReachableTiles(HexTile origin, int maxDistance, Func<HexTile, bool> canEnter)
    {
        List<HexTile> reachableTiles = new List<HexTile>();

        if (origin == null || maxDistance <= 0)
        {
            return reachableTiles;
        }

        Queue<HexTile> frontier = new Queue<HexTile>();
        Dictionary<HexTile, int> costs = new Dictionary<HexTile, int>();

        frontier.Enqueue(origin);
        costs[origin] = 0;

        while (frontier.Count > 0)
        {
            HexTile current = frontier.Dequeue();
            int currentCost = costs[current];

            foreach (HexTile neighbor in GetNeighbors(current))
            {
                if (neighbor == null || !neighbor.isActiveTile)
                {
                    continue;
                }

                if (canEnter != null && !canEnter(neighbor))
                {
                    continue;
                }

                int nextCost = currentCost + 1;
                if (nextCost > maxDistance || costs.ContainsKey(neighbor))
                {
                    continue;
                }

                costs[neighbor] = nextCost;
                frontier.Enqueue(neighbor);
                reachableTiles.Add(neighbor);
            }
        }

        return reachableTiles;
    }

    public bool TryGetPath(HexTile start, HexTile goal, Func<HexTile, bool> canEnter, out List<HexTile> path)
    {
        path = new List<HexTile>();

        if (start == null || goal == null || !goal.isActiveTile)
        {
            return false;
        }

        Queue<HexTile> frontier = new Queue<HexTile>();
        Dictionary<HexTile, HexTile> cameFrom = new Dictionary<HexTile, HexTile>();

        frontier.Enqueue(start);
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            HexTile current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (HexTile neighbor in GetNeighbors(current))
            {
                if (neighbor == null || !neighbor.isActiveTile)
                {
                    continue;
                }

                if (cameFrom.ContainsKey(neighbor))
                {
                    continue;
                }

                if (canEnter != null && !canEnter(neighbor))
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                frontier.Enqueue(neighbor);
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            return false;
        }

        HexTile pathStep = goal;
        while (pathStep != null)
        {
            path.Add(pathStep);
            pathStep = cameFrom[pathStep];
        }

        path.Reverse();
        return true;
    }

    public HexTile GetClosestActiveTile(Vector3 worldPosition)
    {
        HexTile closestTile = null;
        float bestDistance = float.MaxValue;

        foreach (HexTile tile in allTiles)
        {
            if (tile == null || !tile.isActiveTile)
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude((Vector2)(tile.transform.position - worldPosition));
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    public void ClearAllHighlights()
    {
        foreach (HexTile tile in allTiles)
        {
            if (tile != null)
            {
                tile.ClearHighlight();
            }
        }
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMode(PaintMode.ToggleActive);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetMode(PaintMode.Normal);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetMode(PaintMode.PlayerStart1);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetMode(PaintMode.PlayerStart2);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetMode(PaintMode.PlayerStart3);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetMode(PaintMode.PlayerStart4);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SetMode(PaintMode.T1);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SetMode(PaintMode.T2);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SetMode(PaintMode.T3);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetMode(PaintMode.Retaliator);

        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintBoardData();
        }
    }

    private void SetMode(PaintMode mode)
    {
        currentPaintMode = mode;
        Debug.Log($"Paint Mode: {mode}");
    }

    private void PrintBoardData()
    {
        Debug.Log("===== BOARD DATA =====");

        foreach (HexTile tile in allTiles)
        {
            if (!tile.isActiveTile) continue;

            Debug.Log($"({tile.q},{tile.r}) - {tile.tileType}");
        }
    }
}
