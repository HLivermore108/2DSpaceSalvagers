using UnityEngine;

public class HexTile : MonoBehaviour
{
    public int q;
    public int r;

    public bool isActiveTile = true;
    public TileType tileType = TileType.Normal;

    [SerializeField] private SpriteRenderer sr;

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

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        RefreshVisual();
    }

    private void OnMouseDown()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.PaintTile(this);
        }
    }

    public void RefreshVisual()
    {
        if (!isActiveTile)
        {
            sr.color = Color.clear;
            return;
        }

        switch (tileType)
        {
            case TileType.PlayerStart1:
            case TileType.PlayerStart2:
            case TileType.PlayerStart3:
            case TileType.PlayerStart4:
                sr.color = Color.green;
                break;

            case TileType.T1:
                sr.color = Color.yellow;
                break;

            case TileType.T2:
                sr.color = new Color(1f, 0.5f, 0f);
                break;

            case TileType.T3:
                sr.color = Color.red;
                break;

            case TileType.Retaliator:
                sr.color = Color.magenta;
                break;

            default:
                sr.color = Color.white;
                break;
        }
    }
}