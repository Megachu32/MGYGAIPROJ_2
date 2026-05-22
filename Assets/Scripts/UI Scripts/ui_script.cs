using UnityEngine;

public class ui_script : MonoBehaviour
{
    public timer_script targetScript;
    public GridPlacer targetGridPlacer;

    public void resetButton()
    {
        targetScript.ResetTimers();
        targetGridPlacer.DisablePlacing();
        targetGridPlacer.ClearBoard();
        Debug.Log("Reset button clicked!");
    }
    public void startButton()
    {
        targetScript.StartTimers();
        targetGridPlacer.EnablePlacing();
        Debug.Log("Start button clicked!");
    }
    public void dropDownDifivulty()
    {
        Debug.Log("Drop down difficulty clicked!");
    }
    public void PlayAsBlack()
    {
        Debug.Log("Play as Black clicked!");
    }
    public void PlayAsRed()
    {
        Debug.Log("Play as Red clicked!");
    }

}