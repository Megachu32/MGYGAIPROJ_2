using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Threading.Tasks; // <--- ADD THIS EXACT LINE

public class GridPlacer : MonoBehaviour
{
    [Header("Grid Setup")]
    public Grid gameGrid; 
    
    // --- NEW: SCENE ORGANIZATION ---
    [Header("Scene Organization")]
    [Tooltip("Drag an empty GameObject here to hold all the spawned pieces.")]
    public Transform piecesGroup;

    //setting the player peices
    [Header("Standard Player Prefabs")]
    public GameObject player1Prefab; 
    public GameObject player2Prefab; 

    // setting special migo yugo pieces
    [Header("Special 'Migo Yogo' Prefabs")]
    [Tooltip("The special piece Player 1 gets for connecting 4")]
    public GameObject player1SpecialPrefab;
    public GameObject player1SpecialPrefabO;
    public GameObject player1SpecialPrefabT;
    public GameObject player1SpecialPrefabS;

    //code for putting the prefab
    [Tooltip("The special piece Player 2 gets for connecting 4")]
    public GameObject player2SpecialPrefab;
    public GameObject player2SpecialPrefabO;
    public GameObject player2SpecialPrefabT;
    public GameObject player2SpecialPrefabS;



    //setting the board boundries
    [Header("Grid Boundaries")]
    public Vector2Int minBounds = new Vector2Int(-5, -5);
    public Vector2Int maxBounds = new Vector2Int(5, 5);

    // importing other scripts
    [Header("Outside Scripts")]
    public timer_script timerScript;
    public victory_hide_show_script victoryScript;
    public history_script historyScript;

    //audio source?
    [Header("Audio Settings")]
    [Tooltip("Drag the AudioSource component here. This acts as our speaker.")]
    public AudioSource gameAudioSource; 
    
    //sound for placing a piece?
    [Tooltip("Drag your sound effect file here.")]
    public AudioClip placePieceSound;
    public AudioClip yogoTransformationSound; // You can add as many as you need!



    private bool isPlayer1Turn = true;
    private bool isAIThinking = false;

    //this is boolean to set the aponent type
    private bool humanIsPlaying = false; 
    private bool easyAIIsPlaying = false; 
    private bool normalAIIsPlaying = false; 

    private bool playAsRed = true; 

    public bool canPlacePieces = false;

    

    // A custom container to hold both the Player ID and the actual piece on the board
    public class PieceData
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











    public class MoveInfo
    {
        public Vector2Int position; // Where the piece was placed
        public int player;          // Who placed it
        public bool resultsInWin;   // NEW: Tells the AI if this move triggers an AIGO!

        // A list to track pieces that were captured, removed, or flipped during this move.
        public Dictionary<Vector2Int, PieceData> alteredPieces = new Dictionary<Vector2Int, PieceData>();
    }










    // Our Dictionary now stores our custom PieceData instead of just an integer
    private Dictionary<Vector2Int, PieceData> gridData = new Dictionary<Vector2Int, PieceData>();

    //grid data for simulating moves
    private Dictionary<Vector2Int, PieceData> gridDataSimulate = new Dictionary<Vector2Int, PieceData>();









    // for enabling grid placer
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
        // gridDataSimulate.Clear();

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

        // check if it's human vs ai easy
        else if (easyAIIsPlaying) 
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
        // check if it's human vs ai normal
        else if (normalAIIsPlaying) 
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
                        aiMoveAdvance();
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
                        aiMoveAdvance();
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













    System.Collections.IEnumerator WaitAndMakeAIMoveAdvance()
    {
        isAIThinking = true; // Lock the AI so it doesn't trigger again
        
        yield return new WaitForSeconds(0.75f); // Wait for a fraction of a second
        
        aiMoveAdvance(); // Execute the move    
        
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
        if (WouldCreateLineTooLong(cellCoordinate2D, currentPlayerID, gridData))
        {
            Debug.Log("Placement blocked! You cannot make a line longer than 4.");
            return; // This completely stops the placement. The turn doesn't pass.
        }

        // Spawn the standard piece
        Vector3 snapPosition = gameGrid.GetCellCenterWorld(cellCoordinate3D);
        GameObject currentPrefabToPlace = isPlayer1Turn ? player1Prefab : player2Prefab;
        GameObject spawnedPiece = Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);


        PlaySound(placePieceSound);
        // --- NEW: PARENT THE STANDARD PIECE ---
        // If we assigned a group in the inspector, put the piece inside it
        if (piecesGroup != null)
        {
            spawnedPiece.transform.SetParent(piecesGroup);
        }

        // Save BOTH the Player ID and the spawned piece into our Dictionary
        gridData.Add(cellCoordinate2D, new PieceData(currentPlayerID, spawnedPiece, false, 1));
        // gridDataSimulate.Add(cellCoordinate2D, new PieceData(currentPlayerID, spawnedPiece, false, 1));

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
    bool WouldCreateLineTooLong(Vector2Int testPos, int playerID, Dictionary<Vector2Int, PieceData> gridToUse)
    {
        // Check all 4 main axes (Horizontal, Vertical, Diagonal 1, Diagonal 2)
        // If any of them result in a line greater than 4, return true (which means YES, it's too long)
        if (GetPotentialLineLength(testPos, playerID, Vector2Int.right, gridToUse) > 4) return true;
        if (GetPotentialLineLength(testPos, playerID, Vector2Int.up, gridToUse) > 4) return true;
        if (GetPotentialLineLength(testPos, playerID, new Vector2Int(1, 1), gridToUse) > 4) return true;
        if (GetPotentialLineLength(testPos, playerID, new Vector2Int(1, -1), gridToUse) > 4) return true;

        return false; // Safe to place!
    }













    // function to check how long a line would be
    int GetPotentialLineLength(Vector2Int startPos, int playerID, Vector2Int direction, Dictionary<Vector2Int, PieceData> gridToUse)
    {
        // Start at 1 to count the piece we are *about* to place
        int totalLength = 1;

        // Count existing pieces in the forward direction
        totalLength += CountPiecesInDirection(startPos, playerID, direction, gridToUse);
        
        // Count existing pieces in the backward direction
        totalLength += CountPiecesInDirection(startPos, playerID, -direction, gridToUse);

        return totalLength;
    }















    // Helper function to count pieces in a specific direction
    int CountPiecesInDirection(Vector2Int startPos, int playerID, Vector2Int direction, Dictionary<Vector2Int, PieceData> gridToUse)
    {
        int count = 0;
        Vector2Int checkPos = startPos + direction;

        // Keep walking in the direction as long as we find a matching piece
        while (gridToUse.ContainsKey(checkPos) && gridToUse[checkPos].playerID == playerID)
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
        GatherPiecesInLine(placedPos, playerID, Vector2Int.right, cellsToTransform, gridData);
        GatherPiecesInLine(placedPos, playerID, Vector2Int.up, cellsToTransform, gridData);
        GatherPiecesInLine(placedPos, playerID, new Vector2Int(1, 1), cellsToTransform, gridData);
        GatherPiecesInLine(placedPos, playerID, new Vector2Int(1, -1), cellsToTransform, gridData);

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

            Debug.Log("Total Yugos connected: " + yugoCount);

            // 5. Check if they connected 4 Yugos to win!
            if (yugoCount + 1 >= 4)
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
    void GatherPiecesInLine(Vector2Int startPos, int playerID, Vector2Int direction, HashSet<Vector2Int> cellsToTransform, Dictionary<Vector2Int, PieceData> gridToUse)
    {
        // Create a temporary list to hold the pieces we find in this specific line
        List<Vector2Int> matchingCells = new List<Vector2Int>();
        
        // 1. Add the piece we just placed down
        matchingCells.Add(startPos);
        
        // 2. Look forward and add any matching pieces we find
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, direction, gridToUse));
        
        // 3. Look backward and add any matching pieces we find
        matchingCells.AddRange(GetPiecesInDirection(startPos, playerID, -direction, gridToUse));

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
    List<Vector2Int> GetPiecesInDirection(Vector2Int startPos, int playerID, Vector2Int direction, Dictionary<Vector2Int, PieceData> gridToUse)
    {
        List<Vector2Int> foundCells = new List<Vector2Int>();
        Vector2Int checkPos = startPos + direction;

        while (gridToUse.ContainsKey(checkPos) && 
               gridToUse[checkPos].playerID == playerID)
        {
            foundCells.Add(checkPos);
            checkPos += direction; 
        }

        return foundCells;
    }
















    void CheckMigoYogoSimulation(MoveInfo info)
    {
        HashSet<Vector2Int> cellsToTransform = new HashSet<Vector2Int>();

        // 1. Gather pieces using the SIMULATED grid
        GatherPiecesInLine(info.position, info.player, Vector2Int.right, cellsToTransform, gridDataSimulate);
        GatherPiecesInLine(info.position, info.player, Vector2Int.up, cellsToTransform, gridDataSimulate);
        GatherPiecesInLine(info.position, info.player, new Vector2Int(1, 1), cellsToTransform, gridDataSimulate);
        GatherPiecesInLine(info.position, info.player, new Vector2Int(1, -1), cellsToTransform, gridDataSimulate);

        // 2. Check if a transformation happens
        if (cellsToTransform.Count >= 4)
        {
            int totalMigoCount = 0;
            int yugoCount = 0;

            foreach (Vector2Int pos in cellsToTransform)
            {
                if (gridDataSimulate.ContainsKey(pos))
                {
                    // RECORD-KEEPING: If this piece existed BEFORE this turn, log it on the receipt
                    // We skip info.position because that piece was just placed this turn
                    if (pos != info.position) 
                    {
                        info.alteredPieces.Add(pos, gridDataSimulate[pos]);
                    }

                    // Count types
                    if (gridDataSimulate[pos].isSpecialPiece == true) 
                    {
                        yugoCount++;             
                    }
                    else 
                    {
                        totalMigoCount++;
                    }

                    // Destroy the simulated piece
                    gridDataSimulate.Remove(pos);
                }
            }

            // 3. Did they connect 4 Yugos? Flag a win for the AI!
            if (yugoCount >= 4)
            {
                info.resultsInWin = true; 
                return; // Stop here, the game is over
            }

            // 4. Place the simulated Special Yugo piece
            int totalConnectedPieces = totalMigoCount + yugoCount;
            
            // Notice we pass 'null' for the GameObject, but 'true' for isSpecialPiece
            gridDataSimulate[info.position] = new PieceData(info.player, null, true, totalConnectedPieces);
        }
    }













    // ==========================================
    // --- THE AI LOGIC ---
    // ==========================================

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
                if (WouldCreateLineTooLong(potentialCell2D, currentPlayerID, gridData)) continue;

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

        PlaySound(placePieceSound);

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
        // gridDataSimulate.Add(chosenCell2D, new PieceData(currentPlayerID, spawnedPiece, false));

        CheckMigoYogo(chosenCell2D, currentPlayerID);

        // Give the turn back to Player 1
        isPlayer1Turn = !isPlayer1Turn;
    }










    async void aiMoveAdvance()
    {
        isAIThinking = true; // Lock the board so the human can't click
    
        // Wait for 0.75 seconds (Async version of WaitForSeconds)
        await Task.Delay(750); 

        // 1. --- HIRE THE BACKGROUND WORKER ---
        // Task.Run pushes the heavy FindTheBestMove() math off the Main Thread.
        // The "await" keyword tells this specific function to pause and wait for the result,
        // BUT it allows the rest of Unity (like your timers and graphics) to keep running perfectly!
        Vector2Int bestMove = await Task.Run(() => FindTheBestMove());

        // 2. Check if the AI returned our secret "no moves left" signal (-999)
        if (bestMove.x == -999)
        {
            CheckGameOverAndCalculateWinner();
            Debug.Log("AI has no valid moves left! The board is full or blocked.");
            return; 
        }

        // 3. --- WE HAVE A MOVE! EXECUTE IT VISUALLY ---
        int aiID = isPlayer1Turn ? 1 : 2;

        Vector3Int chosenCell3D = new Vector3Int(bestMove.x, bestMove.y, 0);
        historyScript.AddMove(bestMove.x, bestMove.y, 0);
        timerScript.SwitchTurn();

        Vector3 snapPosition = gameGrid.GetCellCenterWorld(chosenCell3D);
        GameObject currentPrefabToPlace = isPlayer1Turn ? player1Prefab : player2Prefab; 
        GameObject spawnedPiece = Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);

        PlaySound(placePieceSound);

        if (piecesGroup != null)
        {
            spawnedPiece.transform.SetParent(piecesGroup);
        }

        // Add it to the REAL visual board memory
        gridData.Add(bestMove, new PieceData(aiID, spawnedPiece, false));
        
        // Check if this physical placement causes a Yugo transformation
        CheckMigoYogo(bestMove, aiID);

        isPlayer1Turn = !isPlayer1Turn;
        isAIThinking = false; // Unlock the board for the human's turn
    }









    // Simulates placing a piece.
    // Nothing is instantiated.
    // Only gridData changes.
    MoveInfo MakeMove(Vector2Int move, int player)
    {
        MoveInfo info = new MoveInfo();

        // Remember where we played.
        info.position = move;

        // Remember who played.
        info.player = player;

        // Add the simulated piece.
        gridDataSimulate.Add(
            move,
            new PieceData(player, null)
        );

        // Simulate any transformations
        // (Migo -> Yogo, removing pieces, etc.)
        CheckMigoYogoSimulation(info);

        return info;
    }









    // Reverses a move using the exact "receipt" generated by MakeMove.
    void UndoMove(MoveInfo info)
    {
        // 1. Remove whatever piece is currently sitting at the move position.
        // (This automatically deletes either the standard piece OR the Special Yugo piece if a transformation happened)
        if (gridDataSimulate.ContainsKey(info.position))
        {
            gridDataSimulate.Remove(info.position);
        }

        // 2. Put back all the pieces that were destroyed by the Migo Yogo transformation
        foreach (KeyValuePair<Vector2Int, PieceData> altered in info.alteredPieces)
        {
            Vector2Int oldPos = altered.Key;
            PieceData oldData = altered.Value;

            // Restore the original piece to the simulated grid
            if (gridDataSimulate.ContainsKey(oldPos))
            {
                gridDataSimulate[oldPos] = oldData;
            }
            else
            {
                gridDataSimulate.Add(oldPos, oldData);
            }
        }
    }







    
    int EvaluateBoardState(int aiID, int playerID)
    {
        int score = 0;

        // Loop through every piece currently on the SIMULATED board
        foreach (KeyValuePair<Vector2Int, PieceData> entry in gridDataSimulate)
        {
            Vector2Int pos = entry.Key;
            PieceData piece = entry.Value;

            // Is this piece owned by the AI or the Player?
            // If it's the AI, we ADD points (+1). If it's the player, we SUBTRACT points (-1).
            int pointMultiplier = (piece.playerID == aiID) ? 1 : -1;

            // --- 1. SCORE SPECIAL PIECES (YUGOS) ---
            if (piece.isSpecialPiece)
            {
                // Yugos are the key to winning, so they are worth massive points
                score += 1000 * pointMultiplier;
                
                // You can even factor in your 'pieceValue' (number of connected Migos)
                score += (piece.pieceValue * 10) * pointMultiplier; 
            }
            
            // --- 2. SCORE STANDARD PIECES ---
            else
            {
                // Base value for just having a piece on the board
                score += 5 * pointMultiplier;

                // --- 3. CENTER CONTROL BONUS ---
                // Pieces closer to the center (0,0) are generally stronger.
                // We calculate how far the piece is from the center (0,0).
                int distanceFromCenter = Mathf.Abs(pos.x) + Mathf.Abs(pos.y);
                
                // We subtract the distance from 10. 
                // A piece at (0,0) gets +10 points. A piece at (5,5) gets +0 points.
                int centerBonus = Mathf.Max(0, 10 - distanceFromCenter); 
                
                score += centerBonus * pointMultiplier;
            }
        }

        return score;
    }








    // Returns a list of all legal moves currently available on the SIMULATED board
    List<Vector2Int> GetValidVirtualMoves(int playerID)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();

        // Loop through the entire grid bounds
        for (int x = minBounds.x; x <= maxBounds.x; x++) 
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                Vector2Int potentialCell2D = new Vector2Int(x, y);

                // 1. Must NOT be taken on the simulated board
                if (gridDataSimulate.ContainsKey(potentialCell2D)) continue;

                // 2. Must NOT break the "lines longer than 4" rule on the simulated board
                // Notice we pass 'gridDataSimulate' here!
                if (WouldCreateLineTooLong(potentialCell2D, playerID, gridDataSimulate)) continue;

                // If it passes all checks, it is a valid future move
                validMoves.Add(potentialCell2D);
            }
        }

        // --- NEW: MOVE ORDERING (ALPHA-BETA ACCELERATOR) ---
        // Sort the list so moves closest to the center (0,0) are at the very top.
        // The AI will test these first, allowing Alpha-Beta to prune millions of useless branches!
        validMoves.Sort((a, b) =>
        {
            int distA = Mathf.Abs(a.x) + Mathf.Abs(a.y);
            int distB = Mathf.Abs(b.x) + Mathf.Abs(b.y);
            return distA.CompareTo(distB); 
        });

        return validMoves;
    }






    void SyncSimulatedBoard()
    {
        gridDataSimulate.Clear();

        foreach (KeyValuePair<Vector2Int, PieceData> entry in gridData)
        {
            Vector2Int pos = entry.Key;
            PieceData realPiece = entry.Value;

            // Create a BRAND NEW PieceData object with no GameObject attached
            PieceData clonedPiece = new PieceData(
                realPiece.playerID, 
                null, // No visual object for the simulation
                realPiece.isSpecialPiece, 
                realPiece.pieceValue
            );

            gridDataSimulate.Add(pos, clonedPiece);
        }
    }






    int Minimax(int depth, int alpha, int beta, bool isMaximizing, int aiID, int humanID)
    {
        // 1. Whose turn is it in this simulation?
        int currentPlayerInSimulation = isMaximizing ? aiID : humanID;

        // 2. Get all legal moves for this specific board state
        List<Vector2Int> validMoves = GetValidVirtualMoves(currentPlayerInSimulation);

        // 3. BASE CASE: Stop if we hit the depth limit or if the board is completely full
        if (depth == 0 || validMoves.Count == 0)
        {
            return EvaluateBoardState(aiID, humanID);
        }

        if (isMaximizing) // --- THE AI'S TURN (Trying to get the highest score) ---
        {
            int maxScore = int.MinValue;

            foreach (Vector2Int move in validMoves)
            {
                // A. Simulate the move
                MoveInfo receipt = MakeMove(move, aiID);

                int score;
                // B. If this move instantly wins the game (AIGO!), give it a massive score and stop looking
                if (receipt.resultsInWin)
                {
                    score = 100000 + depth; // Add depth so faster wins score slightly higher
                }
                else
                {
                    // C. Look deeper into the future (Switch turns to the Human)
                    score = Minimax(depth - 1, alpha, beta, false, aiID, humanID);
                }

                // D. Clean up the board
                UndoMove(receipt);

                // E. Track the best score
                maxScore = Mathf.Max(maxScore, score);
                alpha = Mathf.Max(alpha, score);

                // F. Alpha-Beta Pruning: If the human already found a better path elsewhere, skip the rest of these bad moves
                if (beta <= alpha) break; 
            }
            return maxScore;
        }
        else // --- THE HUMAN'S TURN (Trying to get the lowest score for the AI) ---
        {
            int minScore = int.MaxValue;

            foreach (Vector2Int move in validMoves)
            {
                MoveInfo receipt = MakeMove(move, humanID);

                int score;
                // If the human wins here, it's terrible for the AI
                if (receipt.resultsInWin)
                {
                    score = -100000 - depth; 
                }
                else
                {
                    // Look deeper into the future (Switch turns back to the AI)
                    score = Minimax(depth - 1, alpha, beta, true, aiID, humanID);
                }

                UndoMove(receipt);

                minScore = Mathf.Min(minScore, score);
                beta = Mathf.Min(beta, score);

                if (beta <= alpha) break;
            }
            return minScore;
        }
    }







    // This function strictly "thinks" and returns a coordinate. It does not touch the visual game.
    Vector2Int FindTheBestMove()
    {
        int aiID = isPlayer1Turn ? 1 : 2;
        int humanID = isPlayer1Turn ? 2 : 1;

        // 1. Prepare the sandbox so the AI can think
        SyncSimulatedBoard();

        // 2. Get immediate valid moves
        List<Vector2Int> validMoves = GetValidVirtualMoves(aiID);

        // 3. If there are no moves left, return a "dummy" coordinate as an error signal
        if (validMoves.Count == 0)
        {
            return new Vector2Int(-999, -999); 
        }

        int bestScore = int.MinValue;
        Vector2Int bestMove = validMoves[0]; // Fallback to the first available move
        
        // Set how many turns ahead the AI should think. 
        int searchDepth = 4; 

        // 4. Test every immediate move using Minimax
        foreach (Vector2Int move in validMoves)
        {
            MoveInfo receipt = MakeMove(move, aiID);
            
            int moveScore;
            if (receipt.resultsInWin)
            {
                moveScore = 100000; // If it can win right now, take the win immediately!
            }
            else
            {
                // Call Minimax, handing the next turn to the human (false)
                moveScore = Minimax(searchDepth - 1, int.MinValue, int.MaxValue, false, aiID, humanID);
            }

            UndoMove(receipt);

            // Keep the move that returned the highest score
            if (moveScore > bestScore)
            {
                bestScore = moveScore;
                bestMove = move;
            }
        }

        // 5. Return the winning coordinate back to the main game
        return bestMove;
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
                    if (!WouldCreateLineTooLong(potentialCell2D, 1, gridData) || !WouldCreateLineTooLong(potentialCell2D, 2, gridData))
                    {
                        return true; // Early return: We found a valid move, stop checking!
                    }
                }
            }
        }
        Debug.Log("No valid moves found for either player.");
        return false; // Checked every single tile and found 0 valid moves
    }







    // A reusable function to play any sound effect
    void PlaySound(AudioClip clipToPlay)
    {
        // Safety check: Only try to play if we actually have a speaker and a sound file
        if (gameAudioSource != null && clipToPlay != null)
        {
            gameAudioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning("Tried to play a sound, but the AudioSource or AudioClip is missing!");
        }
    }
}