using UnityEngine;
using UnityEngine.InputSystem; 

public class GridPlacer : MonoBehaviour
{
    [Header("Grid Setup")]
    [Tooltip("Drag your Grid object here from the Hierarchy")]
    public Grid gameGrid; 
    
    [Header("Player Prefabs")]
    [Tooltip("Drag Player 1's piece prefab here")]
    public GameObject player1Prefab; 
    
    [Tooltip("Drag Player 2's piece prefab here")]
    public GameObject player2Prefab; 

    [Header("Grid Boundaries")]
    [Tooltip("The bottom-left limit of the grid (in cell coordinates)")]
    public Vector2Int minBounds = new Vector2Int(-5, -5);
    
    [Tooltip("The top-right limit of the grid (in cell coordinates)")]
    public Vector2Int maxBounds = new Vector2Int(5, 5);

    // --- NEW: A boolean to track turns. True = Player 1, False = Player 2 ---
    private bool isPlayer1Turn = true;

    void Update()
    {
        // Check if a mouse exists and if the left button was clicked this frame
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlacePieceAtMouse();
        }
    }

    void PlacePieceAtMouse()
    {
        // 1 & 2. Get the mouse position using the new Input System
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0f));
        mouseWorldPosition.z = 0f; 

        // 3. Find out which cell the mouse is hovering over
        Vector3Int cellCoordinate = gameGrid.WorldToCell(mouseWorldPosition);

        // 4. Check boundaries
        if (!IsCellInBounds(cellCoordinate))
        {
            Debug.Log($"Placement out of bounds at cell {cellCoordinate}");
            return; 
        }

        // 5. Get the exact center of that cell in world space
        Vector3 snapPosition = gameGrid.GetCellCenterWorld(cellCoordinate);

        // --- NEW: Decide which prefab to place ---
        GameObject currentPrefabToPlace;
        
        if (isPlayer1Turn)
        {
            currentPrefabToPlace = player1Prefab;
        }
        else
        {
            currentPrefabToPlace = player2Prefab;
        }

        // 6. Spawn the correct piece
        Instantiate(currentPrefabToPlace, snapPosition, Quaternion.identity);

        // --- NEW: End the turn ---
        // The "!" symbol means "NOT". So this sets isPlayer1Turn to the opposite of whatever it currently is.
        isPlayer1Turn = !isPlayer1Turn;
        
        // Print a message to the console so you can test if the turns are swapping!
        Debug.Log(isPlayer1Turn ? "Turn ended. It is now Player 1's turn." : "Turn ended. It is now Player 2's turn.");
    }

    // Helper method to check the boundaries
    bool IsCellInBounds(Vector3Int cellPos)
    {
        return cellPos.x >= minBounds.x && cellPos.x <= maxBounds.x &&
               cellPos.y >= minBounds.y && cellPos.y <= maxBounds.y;
    }
}