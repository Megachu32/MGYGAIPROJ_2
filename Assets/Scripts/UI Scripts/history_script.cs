using UnityEngine;
using System.Collections.Generic; // Required to use Lists
using TMPro; // Required to interact with TextMeshPro

public class history_script : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your TextMeshPro text object here in the inspector.")]
    public TextMeshProUGUI historyText; 

    // We use a List (a dynamic array) because we don't know how many moves the game will have.
    private List<string> moveHistory = new List<string>();

    void Start()
    {
        // Optional: Clear the text when the game starts
        if (historyText != null)
        {
            historyText.text = ""; 
        }
    }

    /// <summary>
    /// Call this function and pass a string to add it to the history array.
    /// Example: AddMove("b6");
    /// </summary>
    public void AddMove(int newMoveX , int newMoveY, int isSpecialPiece)
    {

        string moveString = "";

        // Convert the newMoveY integer to a string representation for display
        if (newMoveY == -4)
        {
            moveString = "1";
        }
        else if (newMoveY == -3)
        {
            moveString = "2";
        }
        else if (newMoveY == -2)
        {
            moveString = "3";
        }
        else if (newMoveY == -1)
        {
            moveString = "4";
        }
        else if (newMoveY == 0)
        {
            moveString = "5";
        }
        else if (newMoveY == 1)
        {
            moveString = "6";
        }
        else if (newMoveY == 2)
        {
            moveString = "7";
        }
        else if (newMoveY == 3)
        {
            moveString = "8";
        }

        // Convert the newMoveX integer to a string representation for display
        if (newMoveX == -4)
        {
            moveString += "A";
        }
        else if (newMoveX  == -3)
        {
            moveString += "B";
        }
        else if (newMoveX == -2)
        {
            moveString += "C";
        }
        else if (newMoveX == -1)
        {
            moveString += "D";
        }
        else if (newMoveX == 0)
        {
            moveString += "E";
        }
        else if (newMoveX  == 1)
        {
            moveString += "F";
        }
        else if (newMoveX == 2)
        {
            moveString += "G";
        }
        else if (newMoveX == 3)
        {
            moveString += "H";
        }

        if (isSpecialPiece == 1)
        {
            moveString += "*"; // Append an asterisk to indicate a special piece
        }
        else if (isSpecialPiece == 2)
        {
            moveString += "**"; // Append two asterisks to indicate two lines connected
        }
        else if (isSpecialPiece == 3)
        {
            moveString += "***"; // Append three asterisks to indicate three lines connected
        }
        else if (isSpecialPiece == 4)
        {
            moveString += "****"; // Append four asterisks to indicate four lines connected
        }

        // 1. Put the string inside the list (dynamic array)
        moveHistory.Add(moveString);

        // 2. Trigger the UI update
        UpdateHistoryText();
    }

    /// <summary>
    /// Formats the strings into the incremental, two-column layout shown in your image.
    /// </summary>
    private void UpdateHistoryText()
    {
        if (historyText == null)
        {
            Debug.LogWarning("History TextMeshPro is not assigned in the Inspector!");
            return;
        }

        string formattedDisplay = "";

        // Loop through the history. We step by 2 (i += 2) because each numbered row has up to 2 moves.
        for (int i = 0; i < moveHistory.Count; i += 2)
        {
            // Calculate the row number (0, 1 becomes turn 1. 2, 3 becomes turn 2).
            int rowNumber = (i / 2) + 1;
            
            // Get Player 1's move
            string leftColumnMove = moveHistory[i];
            
            // Get Player 2's move (if it exists yet)
            string rightColumnMove = "";
            if (i + 1 < moveHistory.Count)
            {
                rightColumnMove = moveHistory[i + 1];
            }

            // Format the row exactly like the image: "1. b6    f4"
            // \t adds a horizontal tab spacing. \n pushes the next entry to a new line below.
            formattedDisplay += $"{rowNumber}. {leftColumnMove}\t\t{rightColumnMove}\n";
        }

        // Apply the final formatted string to the TextMeshPro component
        historyText.text = formattedDisplay;
    }

    public void clearHistory()
    {
        moveHistory.Clear();
        UpdateHistoryText();
    }

    /// <summary>
    /// Removes the very last move recorded in the history and replaces it with a new one.
    /// </summary>
    public void ReplaceLastMove(int newMoveX, int newMoveY, int isSpecialPiece)
    {
        // 1. First, we must check if the list actually has anything in it. 
        // Trying to remove an item from an empty list will crash the game.
        if (moveHistory.Count > 0)
        {
            // 2. Remove the very last item in the dynamic array.
            moveHistory.RemoveAt(moveHistory.Count - 1);
        }
        else
        {
            Debug.LogWarning("There are no moves in the history to replace!");
            // We return early so we don't accidentally add the replacement 
            // if there was nothing to replace in the first place.
            return; 
        }

        // 3. Now that the old move is gone, we just call your existing AddMove 
        // function to process the new coordinates and update the UI.
        AddMove(newMoveX, newMoveY, isSpecialPiece);
    }
}