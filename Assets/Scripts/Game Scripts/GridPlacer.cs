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
    public GameObject player1SpecialPrefabO;
    public GameObject player1SpecialPrefabT;
    public GameObject player1SpecialPrefabS;

    [Tooltip("The special piece Player 2 gets for connecting 4")]
    public GameObject player2SpecialPrefab;
    public GameObject player2SpecialPrefabO;
    public GameObject player2SpecialPrefabT;
    public GameObject player2SpecialPrefabS;


    [Header("Grid Boundaries")]
    public Vector2Int minBounds = new Vector2Int(-5, -5);
    public Vector2Int maxBounds = new Vector2Int(5, 5);

    [Header("Outside Scripts")]
    public timer_script timerScript;
    public victory_hide_show_script victoryScript;
    public history_script historyScript;


    private bool isPlayer1Turn = true;
    private bool isAIThinking = false;

    //this is boolean to set the aponent type
    private bool humanIsPlaying = false; 
    private bool easyAIIsPlaying = false; 
    private bool normalAIIsPlaying = false; 

    private bool playAsRed = true; 

    public bool canPlacePieces = false;

    

    // A custom container to hold both the Player ID and the actual piece on the board
    private class PieceData
    {
        public int playerID;
        public GameObject pieceObject;
        public bool isSpecialPiece;
        public int pieceValue; // to check if the piece is a more valuable yugo

        public PieceData(int id, GameObject obj, bool isSpecial = false, int value = 1) // added special pieces for easier checking later
        {
            playerID = id;
            pieceObject = obj;
            isSpecialPiece = isSpecial;
            pieceValue = value;
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

    // function to set human vs human
    public void humanMove(){   
        humanIsPlaying = true;
    }

    // function to set easy ai vs human
    public void easyAIMove(){   
        easyAIIsPlaying = true;
    }

    // function to set normal ai vs human
    public void normalAIMove(){   
        normalAIIsPlaying = true;
    }

    public void clearPlayingModes(){
        humanIsPlaying = false;
        easyAIIsPlaying = false;
        normalAIIsPlaying = false;
    }

    // function to choose red for the game piece
    public void chooseRed(){
        playAsRed = true;
    }

    // function to choose black for the game piece
    public void chooseBlack(){
        timerScript.SwitchTurn(); // Start the timer for the current player as soon as they place a piece
        playAsRed = false;
    }

    void Update()
    {
        if (!canPlacePieces) 
        {
            return;
        }

        // Check if the board is full and calculate the winner if it is
        CheckGameOverAndCalculateWinner();
            
        // check if it's human vs human    
        if (humanIsPlaying)
        {
            // --- HUMAN TURN ---
            // Only check for mouse clicks if it's Player 1's turn
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlacePieceAtMouse();
            }
        }

        // check if it's human vs ai
        else if (easyAIIsPlaying || normalAIIsPlaying) 
        {
            if(playAsRed){
                // switch the turn based on playing what color
                if (isPlayer1Turn)
                {
                    // --- HUMAN TURN ---
                    // Only check for mouse clicks if it's Player 1's turn
                    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        PlacePieceAtMouse();
                    }
                }
                else{
                    // --- AI TURN ---
                    // No mouse click required! Just make sure it isn't already thinking.
                    if (!isAIThinking)
                    {
                        StartCoroutine(WaitAndMakeAIMove());
                    }
                }
            }
            else{
                if (!isPlayer1Turn)
                {
                    // --- HUMAN TURN ---
                    // Only check for mouse clicks if it's Player 2's turn
                    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        PlacePieceAtMouse();
                    }
                }
                else{
                    // --- AI TURN ---
                    // No mouse click required! Just make sure it isn't already thinking.
                    if (!isAIThinking)
                    {
                        StartCoroutine(WaitAndMakeAIMove());
                    }
                }
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

        historyScript.AddMove(cellCoordinate2D.x, cellCoordinate2D.y, 0);

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
        gridData.Add(cellCoordinate2D, new PieceData(currentPlayerID, spawnedPiece, false, 1));

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
        // 1. Use a HashSet to gather all connected pieces without duplicating the center piece
        HashSet<Vector2Int> cellsToTransform = new HashSet<Vector2Int>();

        // 2. Gather connected pieces in all 4 directions
        GatherPiecesInLine(placedPos, playerID, Vector2Int.right, cellsToTransform);
        GatherPiecesInLine(placedPos, playerID, Vector2Int.up, cellsToTransform);
        GatherPiecesInLine(placedPos, playerID, new Vector2Int(1, 1), cellsToTransform);
        GatherPiecesInLine(placedPos, playerID, new Vector2Int(1, -1), cellsToTransform);

        // 3. Now check the TOTAL accumulated Migos. Is the cluster 4 or more?
        if (cellsToTransform.Count >= 4)
        {
            Debug.Log(playerID == 1 ? "⭐⭐⭐ RED MIGO COMBO! ⭐⭐⭐" : "⭐⭐⭐ BLACK MIGO COMBO! ⭐⭐⭐");
            
            int totalMigoCount = 0;
            int yugoCount = 0;

            // 4. Loop through our gathered pieces to count and destroy them
            foreach (Vector2Int pos in cellsToTransform)
            {
                if (gridData.ContainsKey(pos))
                {
                    if (gridData[pos].isSpecialPiece == true) 
                    {
                        yugoCount++;             
                    }
                    else 
                    {
                        totalMigoCount++;
                        Destroy(gridData[pos].pieceObject); 
                        gridData.Remove(pos);
                    }          
                }
            }

            // 5. Check if they connected 4 Yugos to win!
            if (yugoCount >= 4)
            {
                Debug.Log("🔥🔥🔥 AIGO 🔥🔥🔥");
                DisablePlacing();
                timerScript.PauseTimers();
                victoryScript.DisplayVictoryScreen((playerID == 1) ? true : false, "Win by Migo Yogo Combo");
                return; // Stop here, the game is over
            }

            // 6. Spawn the special Yugo piece exactly where the player just clicked
            Vector3 snapPos = gameGrid.GetCellCenterWorld(new Vector3Int(placedPos.x, placedPos.y, 0));
            GameObject specialPrefab = (playerID == 1) ? player1SpecialPrefab : player2SpecialPrefab;

            int totalConnectedPieces = totalMigoCount + yugoCount; // Total connected pieces including Yugos

            int countLines = 1; // to count how many lines of 4 or more are connected to this piece

            // for changing the special piece look based on the number of migos connected
            if (playerID == 1)
            {
                if (totalConnectedPieces <= 16 && totalConnectedPieces > 12)
                {
                    specialPrefab = player1SpecialPrefabS;
                    countLines = 4;
                }
                else if (totalConnectedPieces <= 12 && totalConnectedPieces > 8)
                {
                    specialPrefab = player1SpecialPrefabT;
                    countLines = 3;
                }
                else if (totalConnectedPieces <= 8 && totalConnectedPieces > 4)
                {
                    specialPrefab = player1SpecialPrefabO;
                    countLines = 2;
                }
                else if (totalConnectedPieces <= 4)
                {
                    specialPrefab = player1SpecialPrefab;
                }
            }
            else
            {
                if (totalConnectedPieces <= 16 && totalConnectedPieces > 12)
                {
                    specialPrefab = player2SpecialPrefabS;
                    countLines = 4;
                }
                else if (totalConnectedPieces <= 12 && totalConnectedPieces > 8)
                {
                    specialPrefab = player2SpecialPrefabT;
                    countLines = 3;
                }
                else if (totalConnectedPieces <= 8 && totalConnectedPieces > 4)
                {
                    specialPrefab = player2SpecialPrefabO;
                    countLines = 2;
                }
                else if (totalConnectedPieces <= 4)
                {
                    specialPrefab = player2SpecialPrefab;
                }
            }

            historyScript.ReplaceLastMove(placedPos.x, placedPos.y, countLines); // Pass the number of lines to the history script

            GameObject specialPiece = Instantiate(specialPrefab, snapPos, Quaternion.identity);

            // Parent the special piece
            if (piecesGroup != null)
            {
                specialPiece.transform.SetParent(piecesGroup);
            }

            // Save the newly accumulated value into the grid memory!
            gridData[placedPos] = new PieceData(playerID, specialPiece, true, totalConnectedPieces);
        }
    }

    // Helper function to find connected pieces and add them to our HashSet
    // Looks in a specific direction (like Left/Right) and collects all matching pieces into a list
    void GatherPiecesInLine(Vector2Int startPos, int playerID, Vector2Int direction, HashSet<Vector2Int> cellsToTransform)
    {
        // Create a temporary list to hold the pieces we find in this specific line
        List<Vector2Int> matchingCells = new List<Vector2Int>();
        
        // 1. Add the piece we just placed down
        matchingCells.Add(startPos);
        
        // 2. Look forward and add any matching pieces we find
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, direction));
        
        // 3. Look backward and add any matching pieces we find
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, -direction));

        // --- THE FIX IS HERE ---
        // ONLY add these pieces to our master list IF this specific straight line has 4 or more pieces.
        // This completely prevents triangles, L-shapes, or random clusters from transforming!
        if (matchingCells.Count >= 4)
        {
            foreach (Vector2Int cell in matchingCells)
            {
                cellsToTransform.Add(cell);
            }
        }
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
        // It is the AI's turn if the player choose to play black it goes first as red
        int currentPlayerID = isPlayer1Turn ? 1 : 2;

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
            CheckGameOverAndCalculateWinner();
            Debug.Log("AI has no valid moves left! The board is full or blocked.");
            return; 
        }

        // Pick a random valid cell
        int randomIndex = UnityEngine.Random.Range(0, validMoves.Count);
        Vector2Int chosenCell2D = validMoves[randomIndex];
        Vector3Int chosenCell3D = new Vector3Int(chosenCell2D.x, chosenCell2D.y, 0);

        historyScript.AddMove(chosenCell2D.x, chosenCell2D.y, 0);

        // Place the piece in that cell
        timerScript.SwitchTurn();

        Vector3 snapPosition = gameGrid.GetCellCenterWorld(chosenCell3D);
        GameObject currentPrefabToPlace = isPlayer1Turn ? player1Prefab : player2Prefab; // the ai can play black or red based on the player's choice
        GameObject spawnedPiece = Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);

        if (piecesGroup != null)
        {
            spawnedPiece.transform.SetParent(piecesGroup);
        }

        gridData.Add(chosenCell2D, new PieceData(currentPlayerID, spawnedPiece, false));

        CheckMigoYogo(chosenCell2D, currentPlayerID);

        // Give the turn back to Player 1
        isPlayer1Turn = !isPlayer1Turn;
    }

    // Function to check if the game is over and tally special piece points
    void CheckGameOverAndCalculateWinner()
    {
        // If there is still at least one valid move anywhere on the board, 
        // we do nothing and let the game continue.
        if (IsAnyValidMoveAvailable())
        {
            return; 
        }

        // If we make it past the return statement above, it means no moves are left.
        Debug.Log("No valid moves left on the board! Calculating points...");

        int player1SpecialPoints = 0;
        int player2SpecialPoints = 0;

        foreach (KeyValuePair<Vector2Int, PieceData> entry in gridData)
        {
            PieceData piece = entry.Value;

            if (piece.isSpecialPiece)
            {
                if (piece.playerID == 1)
                {
                    player1SpecialPoints += piece.pieceValue;
                }
                else if (piece.playerID == 2)
                {
                    player2SpecialPoints += piece.pieceValue;
                }
            }
        }

        Debug.Log($"Player 1 Points: {player1SpecialPoints} | Player 2 Points: {player2SpecialPoints}");

        DisablePlacing();
        timerScript.PauseTimers();

        if (player1SpecialPoints > player2SpecialPoints)
        {
            victoryScript.DisplayVictoryScreen(true, "No Moves Left! Player 1 Wins by Points!");
        }
        else if (player2SpecialPoints > player1SpecialPoints)
        {
            victoryScript.DisplayVictoryScreen(false, "No Moves Left! Player 2 Wins by Points!");
        }
        else
        {
            victoryScript.DisplayVictoryScreen(true, "No Moves Left! It's a Tie!"); 
        }
    }

    // Helper function to check if ANY valid moves remain on the board
    bool IsAnyValidMoveAvailable()
    {
        // Loop through every possible space on the grid
        for (int x = minBounds.x; x <= maxBounds.x; x++) 
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                Vector2Int potentialCell2D = new Vector2Int(x, y);
                Vector3Int potentialCell3D = new Vector3Int(x, y, 0);

                // FIX 1: Ignore tiles that are outside the playable board area
                if (!IsCellInBounds(potentialCell3D)) continue;

                // First, check if the cell is completely empty
                if (!gridData.ContainsKey(potentialCell2D))
                {
                    // Check if Player 1 OR Player 2 can safely place a piece here
                    if (!WouldCreateLineTooLong(potentialCell2D, 1) || !WouldCreateLineTooLong(potentialCell2D, 2))
                    {
                        return true; // Early return: We found a valid move, stop checking!
                    }
                }
            }
        }
        Debug.Log("No valid moves found for either player.");
        return false; // Checked every single tile and found 0 valid moves
    }
}