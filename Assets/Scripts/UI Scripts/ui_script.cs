using UnityEngine;
using TMPro;

public class ui_script : MonoBehaviour
{
    [Header("Dropdown")]
    public TMP_Dropdown myDropdown;
    [Header("External References")]
    public timer_script targetScript;
    public GridPlacer targetGridPlacer;

    public void resetButton()
    {
        targetScript.ResetTimers();
        targetGridPlacer.DisablePlacing();
        targetGridPlacer.ClearBoard();
        targetGridPlacer.clearPlayingModes();
        targetGridPlacer.chooseRed(); // Reset to default color
        Debug.Log("Reset button clicked!");
    }
    public void startButton()
    {
        int selectedIndex = myDropdown.value;
        targetScript.StartTimers();
        
        if(selectedIndex == 0)
        {
            targetGridPlacer.humanMove();
        } else if (selectedIndex == 1)
        {
            targetGridPlacer.easyAIMove();
        } else if (selectedIndex == 2)
        {
            targetGridPlacer.normalAIMove();
        }

        targetGridPlacer.EnablePlacing();
        Debug.Log("Start button clicked!" + "value: " + selectedIndex);
    }
    public void PlayAsBlack()
    {
        targetGridPlacer.chooseBlack();
        Debug.Log("Play as Black clicked!");
    }
    public void PlayAsRed()
    {
        targetGridPlacer.chooseRed();
        Debug.Log("Play as Red clicked!");
    }

}