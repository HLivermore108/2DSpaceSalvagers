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

    [Header("Layout Settings")]
    public LayoutPreset layoutPreset = LayoutPreset.PrototypeArena;
    public bool usePresetDimensions = true;
    public bool centerBoardOnOrigin = true;
    public Vector2 boardOffset = Vector2.zero;
    public bool applyPrototypeMarkers = true;

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

    public enum LayoutPreset
    {
        Rectangle,
        PrototypeArena
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyPresetDimensions();
        GenerateBoard();
        ApplyPresetMarkers();
        PositionBoard();
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
                tile.isActiveTile = IsTileActiveForLayout(q, r);
                tile.tileType = HexTile.TileType.Normal;

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

    private void ApplyPresetDimensions()
    {
        if (!usePresetDimensions)
        {
            return;
        }

        switch (layoutPreset)
        {
            case LayoutPreset.PrototypeArena:
                width = 18;
                height = 19;
                xSpacing = 1f;
                ySpacing = 0.87f;
                break;
        }
    }

    private bool IsTileActiveForLayout(int q, int r)
    {
        if (layoutPreset == LayoutPreset.Rectangle)
        {
            return true;
        }

        if (!TryGetPrototypeArenaRange(r, out int startQ, out int length))
        {
            return false;
        }

        return q >= startQ && q < startQ + length;
    }

    private bool TryGetPrototypeArenaRange(int row, out int startQ, out int length)
    {
        startQ = 0;
        length = 0;

        int[] rowStarts =
        {
            1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 1
        };

        int[] rowLengths =
        {
            16, 16, 16,
            17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17,
            16, 16, 16
        };

        if (row < 0 || row >= rowStarts.Length)
        {
            return false;
        }

        startQ = rowStarts[row];
        length = rowLengths[row];
        return true;
    }

    private void ApplyPresetMarkers()
    {
        if (!applyPrototypeMarkers || layoutPreset != LayoutPreset.PrototypeArena)
        {
            return;
        }

        SetTileTypeIfActive(1, 1, HexTile.TileType.PlayerStart1);
        SetTileTypeIfActive(15, 1, HexTile.TileType.PlayerStart2);
        SetTileTypeIfActive(1, 17, HexTile.TileType.PlayerStart3);
        SetTileTypeIfActive(15, 17, HexTile.TileType.PlayerStart4);
        SetTileTypeIfActive(8, 9, HexTile.TileType.Retaliator);
    }

    private void SetTileTypeIfActive(int q, int r, HexTile.TileType tileType)
    {
        HexTile tile = GetTile(q, r);
        if (tile == null || !tile.isActiveTile)
        {
            return;
        }

        if (tileType == HexTile.TileType.Retaliator)
        {
            SetUniqueTileType(tile, tileType);
        }
        else
        {
            tile.tileType = tileType;
            tile.RefreshVisual();
        }
    }

    private void PositionBoard()
    {
        if (boardParent == null)
        {
            return;
        }

        Vector3 localPosition = boardParent.localPosition;

        if (!centerBoardOnOrigin || !GetActiveBoardBounds(out Bounds bounds))
        {
            boardParent.localPosition = new Vector3(boardOffset.x, boardOffset.y, localPosition.z);
            return;
        }

        Vector3 centeredPosition = new Vector3(boardOffset.x - bounds.center.x, boardOffset.y - bounds.center.y, localPosition.z);
        boardParent.localPosition = centeredPosition;
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

    public bool GetActiveBoardBounds(out Bounds bounds)
    {
        bounds = default;
        bool hasBounds = false;

        foreach (HexTile tile in allTiles)
        {
            if (tile == null || !tile.isActiveTile)
            {
                continue;
            }

            SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
            Bounds tileBounds = tileRenderer != null ? tileRenderer.bounds : new Bounds(tile.transform.position, Vector3.one);

            if (!hasBounds)
            {
                bounds = tileBounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(tileBounds);
        }

        return hasBounds;
    }

    private void HandleHotkeys()
    {
        if (TryCheckHotkeyPressed(Hotkey.Digit1)) SetMode(PaintMode.ToggleActive);
        if (TryCheckHotkeyPressed(Hotkey.Digit2)) SetMode(PaintMode.Normal);
        if (TryCheckHotkeyPressed(Hotkey.Digit3)) SetMode(PaintMode.PlayerStart1);
        if (TryCheckHotkeyPressed(Hotkey.Digit4)) SetMode(PaintMode.PlayerStart2);
        if (TryCheckHotkeyPressed(Hotkey.Digit5)) SetMode(PaintMode.PlayerStart3);
        if (TryCheckHotkeyPressed(Hotkey.Digit6)) SetMode(PaintMode.PlayerStart4);
        if (TryCheckHotkeyPressed(Hotkey.Digit7)) SetMode(PaintMode.T1);
        if (TryCheckHotkeyPressed(Hotkey.Digit8)) SetMode(PaintMode.T2);
        if (TryCheckHotkeyPressed(Hotkey.Digit9)) SetMode(PaintMode.T3);
        if (TryCheckHotkeyPressed(Hotkey.Digit0)) SetMode(PaintMode.Retaliator);

        if (TryCheckHotkeyPressed(Hotkey.PrintBoard))
        {
            PrintBoardData();
        }
    }

    private enum Hotkey
    {
        Digit0,
        Digit1,
        Digit2,
        Digit3,
        Digit4,
        Digit5,
        Digit6,
        Digit7,
        Digit8,
        Digit9,
        PrintBoard
    }

    private bool TryCheckHotkeyPressed(Hotkey hotkey)
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return hotkey switch
        {
            Hotkey.Digit0 => Input.GetKeyDown(KeyCode.Alpha0),
            Hotkey.Digit1 => Input.GetKeyDown(KeyCode.Alpha1),
            Hotkey.Digit2 => Input.GetKeyDown(KeyCode.Alpha2),
            Hotkey.Digit3 => Input.GetKeyDown(KeyCode.Alpha3),
            Hotkey.Digit4 => Input.GetKeyDown(KeyCode.Alpha4),
            Hotkey.Digit5 => Input.GetKeyDown(KeyCode.Alpha5),
            Hotkey.Digit6 => Input.GetKeyDown(KeyCode.Alpha6),
            Hotkey.Digit7 => Input.GetKeyDown(KeyCode.Alpha7),
            Hotkey.Digit8 => Input.GetKeyDown(KeyCode.Alpha8),
            Hotkey.Digit9 => Input.GetKeyDown(KeyCode.Alpha9),
            Hotkey.PrintBoard => Input.GetKeyDown(KeyCode.P),
            _ => false
        };
#else
        // The project is currently using the new Input System in Player Settings,
        // but the package namespace is not available in code yet. Disable hotkeys
        // until a proper input action asset is wired up.
        return false;
#endif
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
