using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShipPiece : MonoBehaviour
{
    public static event Action<ShipPiece> ShipClicked;

    private static readonly List<ShipPiece> AllShips = new List<ShipPiece>();

    [Header("Ship Info")]
    [SerializeField] private string shipName = "Ship";
    [SerializeField] private int baseMovement = 2;
    [SerializeField] private bool snapToNearestTileOnStart = true;

    [Header("Runtime State")]
    [SerializeField] private HexTile currentTile;
    [SerializeField] private int movementRemaining;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer shipRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.4f, 1f, 1f);
    [SerializeField] private float selectedScaleMultiplier = 1.1f;

    private Vector3 defaultScale;

    public string ShipName => shipName;
    public int MovementRemaining => movementRemaining;
    public HexTile CurrentTile => currentTile;

    private void Awake()
    {
        defaultScale = transform.localScale;

        if (shipRenderer == null)
        {
            shipRenderer = GetComponent<SpriteRenderer>();
        }

        if (!AllShips.Contains(this))
        {
            AllShips.Add(this);
        }
    }

    private void Start()
    {
        if (BoardManager.Instance != null && currentTile == null && snapToNearestTileOnStart)
        {
            HexTile nearestTile = BoardManager.Instance.GetClosestActiveTile(transform.position);
            SetCurrentTile(nearestTile);
        }

        ResetMovement();
        SetSelected(false);
    }

    private void OnDestroy()
    {
        AllShips.Remove(this);
    }

    private void OnMouseDown()
    {
        if (BoardManager.Instance != null && BoardManager.Instance.editMode)
        {
            return;
        }

        ShipClicked?.Invoke(this);
    }

    public void ResetMovement()
    {
        movementRemaining = Mathf.Max(1, baseMovement);
    }

    public bool CanSpendMovement(int amount)
    {
        return amount >= 0 && amount <= movementRemaining;
    }

    public void SpendMovement(int amount)
    {
        movementRemaining = Mathf.Max(0, movementRemaining - Mathf.Max(0, amount));
    }

    public void SetCurrentTile(HexTile tile)
    {
        currentTile = tile;

        if (currentTile == null)
        {
            return;
        }

        Vector3 tilePosition = currentTile.transform.position;
        tilePosition.z = transform.position.z;
        transform.position = tilePosition;
    }

    public void SetSelected(bool isSelected)
    {
        transform.localScale = isSelected ? defaultScale * selectedScaleMultiplier : defaultScale;

        if (shipRenderer != null)
        {
            shipRenderer.color = isSelected ? selectedColor : normalColor;
        }
    }

    public static bool TryGetShipAtTile(HexTile tile, out ShipPiece ship)
    {
        foreach (ShipPiece candidate in AllShips)
        {
            if (candidate != null && candidate.currentTile == tile)
            {
                ship = candidate;
                return true;
            }
        }

        ship = null;
        return false;
    }

    public static bool IsTileOccupied(HexTile tile)
    {
        return TryGetShipAtTile(tile, out _);
    }
}
