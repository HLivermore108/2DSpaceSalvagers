using System.Collections.Generic;
using UnityEngine;

public class ShipMovementController : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private ShipPiece startingSelectedShip;

    private ShipPiece selectedShip;
    private HexTile pendingDestination;
    private int pendingMoveCost;
    private readonly List<HexTile> reachableTiles = new List<HexTile>();
    private readonly List<HexTile> pendingPath = new List<HexTile>();

    private void Awake()
    {
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
    }

    private void OnEnable()
    {
        HexTile.TileClicked += HandleTileClicked;
        ShipPiece.ShipClicked += HandleShipClicked;
    }

    private void OnDisable()
    {
        HexTile.TileClicked -= HandleTileClicked;
        ShipPiece.ShipClicked -= HandleShipClicked;
    }

    private void Start()
    {
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }

        if (startingSelectedShip != null)
        {
            SelectShip(startingSelectedShip);
        }
    }

    private void HandleShipClicked(ShipPiece ship)
    {
        if (ship == null)
        {
            return;
        }

        SelectShip(ship);
    }

    private void HandleTileClicked(HexTile tile)
    {
        if (boardManager == null || boardManager.editMode || tile == null || selectedShip == null)
        {
            return;
        }

        if (tile == selectedShip.CurrentTile)
        {
            CancelMove();
            return;
        }

        if (!tile.isActiveTile)
        {
            return;
        }

        if (ShipPiece.TryGetShipAtTile(tile, out ShipPiece occupyingShip) && occupyingShip != selectedShip)
        {
            Debug.Log("That hex is already occupied by another ship.");
            return;
        }

        if (!boardManager.TryGetPath(selectedShip.CurrentTile, tile, CanEnterTile, out List<HexTile> path))
        {
            Debug.Log("No valid path to that hex.");
            return;
        }

        int moveCost = Mathf.Max(0, path.Count - 1);
        if (moveCost == 0 || !selectedShip.CanSpendMovement(moveCost))
        {
            Debug.Log("That destination is out of range for this turn.");
            return;
        }

        pendingDestination = tile;
        pendingMoveCost = moveCost;
        pendingPath.Clear();
        pendingPath.AddRange(path);
        UpdateHighlights();
    }

    public void SelectShip(ShipPiece ship)
    {
        if (selectedShip == ship)
        {
            RefreshReachableTiles();
            return;
        }

        if (selectedShip != null)
        {
            selectedShip.SetSelected(false);
        }

        selectedShip = ship;

        if (selectedShip != null)
        {
            selectedShip.SetSelected(true);
        }

        ClearPendingMove();
        RefreshReachableTiles();
    }

    public void ConfirmMove()
    {
        if (selectedShip == null || pendingDestination == null || pendingMoveCost <= 0)
        {
            return;
        }

        if (!selectedShip.CanSpendMovement(pendingMoveCost))
        {
            Debug.Log("This ship no longer has enough movement.");
            ClearPendingMove();
            RefreshReachableTiles();
            return;
        }

        selectedShip.SpendMovement(pendingMoveCost);
        selectedShip.SetCurrentTile(pendingDestination);

        Debug.Log($"{selectedShip.ShipName} moved to ({pendingDestination.q}, {pendingDestination.r}) for {pendingMoveCost} movement.");

        ClearPendingMove();
        RefreshReachableTiles();
    }

    public void CancelMove()
    {
        ClearPendingMove();
        UpdateHighlights();
    }

    public void BeginTurn(ShipPiece ship)
    {
        if (ship == null)
        {
            return;
        }

        ship.ResetMovement();
        SelectShip(ship);
    }

    private void RefreshReachableTiles()
    {
        reachableTiles.Clear();

        if (selectedShip != null && selectedShip.CurrentTile != null && selectedShip.MovementRemaining > 0 && boardManager != null)
        {
            reachableTiles.AddRange(boardManager.GetReachableTiles(selectedShip.CurrentTile, selectedShip.MovementRemaining, CanEnterTile));
        }

        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        if (boardManager == null)
        {
            return;
        }

        boardManager.ClearAllHighlights();

        foreach (HexTile tile in reachableTiles)
        {
            tile.SetHighlight(HexTile.RuntimeHighlight.Reachable);
        }

        for (int i = 1; i < pendingPath.Count - 1; i++)
        {
            pendingPath[i].SetHighlight(HexTile.RuntimeHighlight.Path);
        }

        if (pendingDestination != null)
        {
            pendingDestination.SetHighlight(HexTile.RuntimeHighlight.PendingDestination);
        }

        if (selectedShip != null && selectedShip.CurrentTile != null)
        {
            selectedShip.CurrentTile.SetHighlight(HexTile.RuntimeHighlight.Selected);
        }
    }

    private void ClearPendingMove()
    {
        pendingDestination = null;
        pendingMoveCost = 0;
        pendingPath.Clear();
    }

    private bool CanEnterTile(HexTile tile)
    {
        if (tile == null || !tile.isActiveTile)
        {
            return false;
        }

        return !ShipPiece.IsTileOccupied(tile) || tile == selectedShip.CurrentTile;
    }

    private void OnGUI()
    {
        if (selectedShip == null || boardManager == null || boardManager.editMode)
        {
            return;
        }

        Rect panelRect = new Rect(12f, 12f, 260f, pendingDestination != null ? 130f : 90f);
        GUILayout.BeginArea(panelRect, GUI.skin.box);
        GUILayout.Label($"Selected Ship: {selectedShip.ShipName}");
        GUILayout.Label($"Movement Remaining: {selectedShip.MovementRemaining}");

        if (pendingDestination == null)
        {
            GUILayout.Label("Click a highlighted hex to stage a move.");
        }
        else
        {
            GUILayout.Label($"Destination: ({pendingDestination.q}, {pendingDestination.r})");
            GUILayout.Label($"Move Cost: {pendingMoveCost}");

            if (GUILayout.Button("Confirm Move", GUILayout.Height(28f)))
            {
                ConfirmMove();
            }

            if (GUILayout.Button("Cancel", GUILayout.Height(24f)))
            {
                CancelMove();
            }
        }

        GUILayout.EndArea();
    }
}
