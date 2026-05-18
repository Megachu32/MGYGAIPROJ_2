using UnityEngine;

public class ui_script : MonoBehaviour
{
    public timer_script targetScript;

    public void resetButton()
    {
        Debug.Log("Reset button clicked!");
    }
    public void startButton()
    {
        targetScript.StartTimers();
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