using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

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

    private List<HexTile> allTiles = new List<HexTile>();

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
                SetSingleTileType(tile, HexTile.TileType.PlayerStart1);
                break;

            case PaintMode.PlayerStart2:
                SetSingleTileType(tile, HexTile.TileType.PlayerStart2);
                break;

            case PaintMode.PlayerStart3:
                SetSingleTileType(tile, HexTile.TileType.PlayerStart3);
                break;

            case PaintMode.PlayerStart4:
                SetSingleTileType(tile, HexTile.TileType.PlayerStart4);
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
                tile.isActiveTile = true;
                tile.tileType = HexTile.TileType.Retaliator;
                break;
        }

        tile.RefreshVisual();
    }

    private void SetSingleTileType(HexTile tile, HexTile.TileType type)
    {
        // ensures only ONE tile of this type exists
        foreach (var t in allTiles)
        {
            if (t.tileType == type)
                t.tileType = HexTile.TileType.Normal;
        }

        tile.isActiveTile = true;
        tile.tileType = type;
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