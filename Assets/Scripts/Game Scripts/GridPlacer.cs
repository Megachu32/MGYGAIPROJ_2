using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    [Header("Grid Setup")]
    public Grid gameGrid; 
    
    [Header("Standard Player Prefabs")]
    public GameObject player1Prefab; 
    public GameObject player2Prefab; 

    [Header("Special 'Migo Yogo' Prefabs")]
    [Tooltip("The special piece Player 1 gets for connecting 4")]
    public GameObject player1SpecialPrefab;
    [Tooltip("The special piece Player 2 gets for connecting 4")]
    public GameObject player2SpecialPrefab;

    [Header("Grid Boundaries")]
    public Vector2Int minBounds = new Vector2Int(-5, -5);
    public Vector2Int maxBounds = new Vector2Int(5, 5);

    private bool isPlayer1Turn = true;

    // --- NEW: A custom container to hold both the Player ID and the actual piece on the board ---
    private class PieceData
    {
        public int playerID;
        public GameObject pieceObject;

        public PieceData(int id, GameObject obj)
        {
            playerID = id;
            pieceObject = obj;
        }
    }

    // Our Dictionary now stores our custom PieceData instead of just an integer
    private Dictionary<Vector2Int, PieceData> gridData = new Dictionary<Vector2Int, PieceData>();

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlacePieceAtMouse();
        }
    }

    void PlacePieceAtMouse()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0f));
        mouseWorldPosition.z = 0f; 

        Vector3Int cellCoordinate3D = gameGrid.WorldToCell(mouseWorldPosition);
        Vector2Int cellCoordinate2D = new Vector2Int(cellCoordinate3D.x, cellCoordinate3D.y);

        if (!IsCellInBounds(cellCoordinate3D))
        {
            Debug.Log($"Placement out of bounds at cell {cellCoordinate3D}");
            return; 
        }

        if (gridData.ContainsKey(cellCoordinate2D))
        {
            Debug.Log($"Cell {cellCoordinate2D} is already taken! Try another spot.");
            return; 
        }

        // Spawn the standard piece
        Vector3 snapPosition = gameGrid.GetCellCenterWorld(cellCoordinate3D);
        GameObject currentPrefabToPlace = isPlayer1Turn ? player1Prefab : player2Prefab;
        GameObject spawnedPiece = Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);

        int currentPlayerID = isPlayer1Turn ? 1 : 2;

        // --- NEW: Save BOTH the Player ID and the spawned piece into our Dictionary ---
        gridData.Add(cellCoordinate2D, new PieceData(currentPlayerID, spawnedPiece));

        // Check for 4 in a row!
        CheckMigoYogo(cellCoordinate2D, currentPlayerID);

        isPlayer1Turn = !isPlayer1Turn;
    }

    bool IsCellInBounds(Vector3Int cellPos)
    {
        return cellPos.x >= minBounds.x && cellPos.x <= maxBounds.x &&
               cellPos.y >= minBounds.y && cellPos.y <= maxBounds.y;
    }

    // ==========================================
    // --- THE MIGO YOGO TRANSFORMATION LOGIC ---
    // ==========================================

    void CheckMigoYogo(Vector2Int placedPos, int playerID)
    {
        // We check each direction. If we find a line of 4 and transform it, 
        // we "return" immediately so we don't accidentally try to destroy the same pieces twice.
        if (CheckAndTransformLine(placedPos, playerID, Vector2Int.right)) return;
        if (CheckAndTransformLine(placedPos, playerID, Vector2Int.up)) return;
        if (CheckAndTransformLine(placedPos, playerID, new Vector2Int(1, 1))) return;
        if (CheckAndTransformLine(placedPos, playerID, new Vector2Int(1, -1))) return;
    }

    // This method looks for a winning line, and if it finds one, it transforms the pieces
    bool CheckAndTransformLine(Vector2Int startPos, int playerID, Vector2Int direction)
    {
        // Create a list to hold the coordinates of the winning pieces
        List<Vector2Int> matchingCells = new List<Vector2Int>();
        
        // Add the piece we just placed
        matchingCells.Add(startPos);

        // Gather all matching pieces in the positive direction (e.g., Right)
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, direction));
        
        // Gather all matching pieces in the negative direction (e.g., Left)
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, -direction));

        // If we found 4 or more pieces in this line...
        if (matchingCells.Count >= 4)
        {
            Debug.Log(playerID == 1 ? "⭐⭐⭐ RED MIGO! ⭐⭐⭐" : "⭐⭐⭐ YUGO! ⭐⭐⭐");

            // 1. Destroy all 4 old pieces
            foreach (Vector2Int pos in matchingCells)
            {
                if (gridData.ContainsKey(pos))
                {
                    Destroy(gridData[pos].pieceObject); // Remove it from the Unity Scene
                    gridData.Remove(pos);               // Remove it from our script's memory
                }
            }

            // 2. Spawn the new Special Prefab where the player just clicked
            Vector3 snapPos = gameGrid.GetCellCenterWorld(new Vector3Int(startPos.x, startPos.y, 0));
            GameObject specialPrefab = (playerID == 1) ? player1SpecialPrefab : player2SpecialPrefab;
            GameObject specialPiece = Instantiate(specialPrefab, snapPos, Quaternion.identity);

            // 3. Add the new Special piece back into our grid memory so it takes up space
            gridData.Add(startPos, new PieceData(playerID, specialPiece));

            return true; // Tell the game we successfully transformed a line
        }

        return false; // No line of 4 found here
    }

    // Helper method that walks cell-by-cell and returns a LIST of coordinates instead of just counting
    List<Vector2Int> GetPiecesInDirection(Vector2Int startPos, int playerID, Vector2Int direction)
    {
        List<Vector2Int> foundCells = new List<Vector2Int>();
        Vector2Int checkPos = startPos + direction;

        while (gridData.ContainsKey(checkPos) && gridData[checkPos].playerID == playerID)
        {
            foundCells.Add(checkPos);
            checkPos += direction; 
        }

        return foundCells;
    }
}