using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    [Header("Grid Setup")]
    public Grid gameGrid; 
    
    // --- NEW: SCENE ORGANIZATION ---
    [Header("Scene Organization")]
    [Tooltip("Drag an empty GameObject here to hold all the spawned pieces.")]
    public Transform piecesGroup; 
    
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

    [Header("Outside Scripts")]
    public timer_script timerScript;


    private bool isPlayer1Turn = true;
    private bool isAIThinking = false;

    public bool canPlacePieces = false;

    

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

    public void EnablePlacing()
    {
        canPlacePieces = true;
        Debug.Log("GridPlacer is now active!");
    }

    // You can also add a function to stop it again if needed
    public void DisablePlacing()
    {
        canPlacePieces = false;
    }

    public void ClearBoard()
    {
        // 1. Loop through all child objects inside the group and destroy them
        if (piecesGroup != null)
        {
            foreach (Transform childPiece in piecesGroup)
            {
                Destroy(childPiece.gameObject);
            }
        }

        // 2. Wipe the internal memory so the game knows the spaces are empty again
        gridData.Clear();

        // 3. Reset the turn back to Player 1
        isPlayer1Turn = true;

        Debug.Log("The board has been cleared!");
    }

    void Update()
    {
        if (!canPlacePieces) 
        {
            return;
        }
            
        if (isPlayer1Turn)
        {
            // --- HUMAN TURN ---
            // Only check for mouse clicks if it's Player 1's turn
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlacePieceAtMouse();
            }
        }
        else
        {
            // --- AI TURN ---
            // No mouse click required! Just make sure it isn't already thinking.
            if (!isAIThinking)
            {
                StartCoroutine(WaitAndMakeAIMove());
            }
        }
    }

    //adding delay and calling the ai move function
    System.Collections.IEnumerator WaitAndMakeAIMove()
    {
        isAIThinking = true; // Lock the AI so it doesn't trigger again
        
        yield return new WaitForSeconds(0.75f); // Wait for a fraction of a second
        
        aiMove(); // Execute the move
        
        isAIThinking = false; // Unlock for the next turn
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
        timerScript.SwitchTurn(); // Start the timer for the current player as soon as they place a piece

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

        // --- NEW: PARENT THE STANDARD PIECE ---
        // If we assigned a group in the inspector, put the piece inside it
        if (piecesGroup != null)
        {
            spawnedPiece.transform.SetParent(piecesGroup);
        }

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

                // --- NEW: PARENT THE SPECIAL PIECE ---
                if (piecesGroup != null)
                {
                    specialPiece.transform.SetParent(piecesGroup);
                }

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

    void aiMove()
    {
        // It is the AI's turn, so the ID is automatically 2.
        int currentPlayerID = 2;

        //making the list for avalable move
        List<Vector2Int> validMoves = new List<Vector2Int>();

        // Loop through all cells in the grid to find valid moves
        for (int x = minBounds.x; x <= maxBounds.x; x++) 
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                Vector2Int potentialCell2D = new Vector2Int(x, y);
                Vector3Int potentialCell3D = new Vector3Int(x, y, 0);

                // Must be in bounds
                if (!IsCellInBounds(potentialCell3D)) continue;

                // Must NOT be taken
                if (gridData.ContainsKey(potentialCell2D)) continue;

                // Must NOT break the "lines longer than 4" rule
                if (WouldCreateLineTooLong(potentialCell2D, currentPlayerID)) continue;

                // If it passes all checks, add it to our list of possible moves
                validMoves.Add(potentialCell2D);
            }
        }

        //check if no avalible move
        if (validMoves.Count == 0)
        {
            Debug.Log("AI has no valid moves left! The board is full or blocked.");
            return; 
        }

        // Pick a random valid cell
        int randomIndex = UnityEngine.Random.Range(0, validMoves.Count);
        Vector2Int chosenCell2D = validMoves[randomIndex];
        Vector3Int chosenCell3D = new Vector3Int(chosenCell2D.x, chosenCell2D.y, 0);

        // Place the piece in that cell
        timerScript.SwitchTurn();

        Vector3 snapPosition = gameGrid.GetCellCenterWorld(chosenCell3D);
        GameObject currentPrefabToPlace = player2Prefab; // It's the AI, so always use Player 2's prefab
        GameObject spawnedPiece = Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);

        if (piecesGroup != null)
        {
            spawnedPiece.transform.SetParent(piecesGroup);
        }

        gridData.Add(chosenCell2D, new PieceData(currentPlayerID, spawnedPiece, false));

        CheckMigoYogo(chosenCell2D, currentPlayerID);

        // Give the turn back to Player 1
        isPlayer1Turn = true;
    }
}