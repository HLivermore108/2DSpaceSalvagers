using System;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public static event Action<HexTile> TileClicked;

    public int q;
    public int r;

    public bool isActiveTile = true;
    public TileType tileType = TileType.Normal;

    [SerializeField] private SpriteRenderer sr;

    private RuntimeHighlight runtimeHighlight = RuntimeHighlight.None;

    public enum TileType
    {
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

    public enum RuntimeHighlight
    {
        None,
        Reachable,
        Path,
        PendingDestination,
        Selected
    }

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        RefreshVisual();
    }

    private void OnMouseDown()
    {
        if (BoardManager.Instance == null)
        {
            return;
        }

        if (BoardManager.Instance.editMode)
        {
            BoardManager.Instance.PaintTile(this);
            return;
        }

        TileClicked?.Invoke(this);
    }

    public void SetHighlight(RuntimeHighlight highlight)
    {
        runtimeHighlight = highlight;
        RefreshVisual();
    }

    public void ClearHighlight()
    {
        if (runtimeHighlight == RuntimeHighlight.None)
        {
            return;
        }

        runtimeHighlight = RuntimeHighlight.None;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        if (!isActiveTile)
        {
            sr.color = Color.clear;
            return;
        }

        Color baseColor;

        switch (tileType)
        {
            case TileType.PlayerStart1:
            case TileType.PlayerStart2:
            case TileType.PlayerStart3:
            case TileType.PlayerStart4:
                baseColor = Color.green;
                break;

            case TileType.T1:
                baseColor = Color.yellow;
                break;

            case TileType.T2:
                baseColor = new Color(1f, 0.5f, 0f);
                break;

            case TileType.T3:
                baseColor = Color.red;
                break;

            case TileType.Retaliator:
                baseColor = Color.magenta;
                break;

            default:
                baseColor = Color.white;
                break;
        }

        switch (runtimeHighlight)
        {
            case RuntimeHighlight.Reachable:
                sr.color = Color.Lerp(baseColor, new Color(0.2f, 1f, 1f), 0.65f);
                break;

            case RuntimeHighlight.Path:
                sr.color = Color.Lerp(baseColor, new Color(0.35f, 0.6f, 1f), 0.7f);
                break;

            case RuntimeHighlight.PendingDestination:
                sr.color = Color.Lerp(baseColor, new Color(0.15f, 1f, 0.35f), 0.85f);
                break;

            case RuntimeHighlight.Selected:
                sr.color = Color.Lerp(baseColor, Color.cyan, 0.4f);
                break;

            default:
                sr.color = baseColor;
                break;
        }
    }
}
