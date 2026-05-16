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

    // A custom container to hold both the Player ID and the actual piece on the board
    private class PieceData
    {
        public int playerID;
        public GameObject pieceObject;
        public bool isSpecialPiece;

        public PieceData(int id, GameObject obj, bool isSpecial = false) // added special pieces for easier checking later
        {
            playerID = id;
            pieceObject = obj;
            isSpecialPiece = isSpecial;
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

    // function to place a piece on the board
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

        // Figure out whose turn it is BEFORE placing the piece
        int currentPlayerID = isPlayer1Turn ? 1 : 2;

        // ==========================================
        // --- NEW RULE: BLOCK LINES LONGER THAN 4 ---
        // ==========================================
        if (WouldCreateLineTooLong(cellCoordinate2D, currentPlayerID))
        {
            Debug.Log("Placement blocked! You cannot make a line longer than 4.");
            return; // This completely stops the placement. The turn doesn't pass.
        }

        // Spawn the standard piece
        Vector3 snapPosition = gameGrid.GetCellCenterWorld(cellCoordinate3D);
        GameObject currentPrefabToPlace = isPlayer1Turn ? player1Prefab : player2Prefab;
        GameObject spawnedPiece = Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);

        // Save BOTH the Player ID and the spawned piece into our Dictionary
        gridData.Add(cellCoordinate2D, new PieceData(currentPlayerID, spawnedPiece, false));

        // Check for 4 in a row!
        CheckMigoYogo(cellCoordinate2D, currentPlayerID);

        isPlayer1Turn = !isPlayer1Turn;
    }

    // function to check if a cell is within the board
    bool IsCellInBounds(Vector3Int cellPos)
    {
        return cellPos.x >= minBounds.x && cellPos.x <= maxBounds.x &&
               cellPos.y >= minBounds.y && cellPos.y <= maxBounds.y;
    }

    // function to check if a line is more than 4 long
    bool WouldCreateLineTooLong(Vector2Int testPos, int playerID)
    {
        // Check all 4 main axes (Horizontal, Vertical, Diagonal 1, Diagonal 2)
        // If any of them result in a line greater than 4, return true (which means YES, it's too long)
        if (GetPotentialLineLength(testPos, playerID, Vector2Int.right) > 4) return true;
        if (GetPotentialLineLength(testPos, playerID, Vector2Int.up) > 4) return true;
        if (GetPotentialLineLength(testPos, playerID, new Vector2Int(1, 1)) > 4) return true;
        if (GetPotentialLineLength(testPos, playerID, new Vector2Int(1, -1)) > 4) return true;

        return false; // Safe to place!
    }

    // function to check how long a line would be
    int GetPotentialLineLength(Vector2Int startPos, int playerID, Vector2Int direction)
    {
        // Start at 1 to count the piece we are *about* to place
        int totalLength = 1;

        // Count existing pieces in the forward direction
        totalLength += CountPiecesInDirection(startPos, playerID, direction);
        
        // Count existing pieces in the backward direction
        totalLength += CountPiecesInDirection(startPos, playerID, -direction);

        return totalLength;
    }

    // Helper function to count pieces in a specific direction
    int CountPiecesInDirection(Vector2Int startPos, int playerID, Vector2Int direction)
    {
        int count = 0;
        Vector2Int checkPos = startPos + direction;

        // Keep walking in the direction as long as we find a matching piece
        while (gridData.ContainsKey(checkPos) && gridData[checkPos].playerID == playerID)
        {
            count++;
            checkPos += direction; 
        }

        return count;
    }

    // ==========================================
    // --- THE MIGO YOGO TRANSFORMATION LOGIC ---
    // ==========================================

    // function for checking if a piece can be transformed into a yugo
    void CheckMigoYogo(Vector2Int placedPos, int playerID)
    {
        // 1. Remove the "if (...) return;" part from these lines! 
        // This forces the game to check EVERY direction, allowing crosses/combos.
        CheckAndTransformLine(placedPos, playerID, Vector2Int.right);
        CheckAndTransformLine(placedPos, playerID, Vector2Int.up);
        CheckAndTransformLine(placedPos, playerID, new Vector2Int(1, 1));
        CheckAndTransformLine(placedPos, playerID, new Vector2Int(1, -1));
    }

    // function to check a line of migo and transform it into a yugo
    bool CheckAndTransformLine(Vector2Int startPos, int playerID, Vector2Int direction)
    {
        List<Vector2Int> matchingCells = new List<Vector2Int>();
        
        matchingCells.Add(startPos);
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, direction));
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, -direction));

        if (matchingCells.Count >= 4)
        {
            Debug.Log(playerID == 1 ? "⭐⭐⭐ RED MIGO! ⭐⭐⭐" : "⭐⭐⭐ YUGO! ⭐⭐⭐");

            foreach (Vector2Int pos in matchingCells)
            {
                if (gridData.ContainsKey(pos))
                {
                    if (gridData[pos].isSpecialPiece == false) 
                    {
                        Destroy(gridData[pos].pieceObject); 
                        gridData.Remove(pos);               
                    }          
                }
            }

            // --- NEW: THE DOUBLE-SPAWN FIX ---
            // If a previous combo line already spawned a special piece here, we skip this step!
            if (!gridData.ContainsKey(startPos) || gridData[startPos].isSpecialPiece == false)
            {
                Vector3 snapPos = gameGrid.GetCellCenterWorld(new Vector3Int(startPos.x, startPos.y, 0));
                GameObject specialPrefab = (playerID == 1) ? player1SpecialPrefab : player2SpecialPrefab;
                GameObject specialPiece = Instantiate(specialPrefab, snapPos, Quaternion.identity);

                // Use brackets [] instead of .Add() to safely update the memory without errors
                gridData[startPos] = new PieceData(playerID, specialPiece, true);
            }

            return true; 
        }

        return false; 
    }

    // helper function to get all pieces in a direction
    List<Vector2Int> GetPiecesInDirection(Vector2Int startPos, int playerID, Vector2Int direction)
    {
        List<Vector2Int> foundCells = new List<Vector2Int>();
        Vector2Int checkPos = startPos + direction;

        while (gridData.ContainsKey(checkPos) && 
               gridData[checkPos].playerID == playerID)
        {
            foundCells.Add(checkPos);
            checkPos += direction; 
        }

        return foundCells;
    }
}