using System;
using UnityEngine;

public class BoardLogic : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    public GameObject tilePrefab;
    
    [Header("Board Dimensions")]
    public int columns = 8;
    public int rows = 8;

    void Start()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        // Optional: Create an empty parent object to keep your Unity Hierarchy clean
        GameObject boardHolder = new GameObject("BoardContainer");
        boardHolder.transform.SetParent(this.transform);

        // Loop through the x (columns) and y (rows) to spawn tiles
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                // Calculate grid position and convert to world space
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector3 worldPosition = _grid.GetCellCenterWorld(cellPosition);
                
                // Instantiate the tile and set its parent to the boardHolder
                GameObject newTile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, boardHolder.transform);
                
                // Name the tile for easy debugging in the Hierarchy (e.g., "Tile_2_3")
                newTile.name = $"Tile_{x}_{y}";
            }
        }
    }
}