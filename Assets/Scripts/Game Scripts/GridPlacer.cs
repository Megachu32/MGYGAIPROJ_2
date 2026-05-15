using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class GridPlacer : MonoBehaviour
{
    [Tooltip("Drag your Grid object here from the Hierarchy")]
    public Grid gameGrid; 
    
    [Tooltip("Drag the prefab of the piece you want to place here")]
    public GameObject piecePrefab; 

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
        
        // Convert that Vector2 into a Vector3 for the camera conversion
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0f));
        
        // Force the Z position to 0 since we are working in 2D
        mouseWorldPosition.z = 0f; 

        // 3. Find out which cell the mouse is hovering over
        Vector3Int cellCoordinate = gameGrid.WorldToCell(mouseWorldPosition);

        // 4. Get the exact center of that cell in world space
        Vector3 snapPosition = gameGrid.GetCellCenterWorld(cellCoordinate);

        // 5. Spawn the piece exactly at that snapped position
        Instantiate(piecePrefab, snapPosition, Quaternion.identity);
    }
}