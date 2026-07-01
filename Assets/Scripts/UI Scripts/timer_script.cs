using UnityEngine;
using TMPro; 
using System.Threading.Tasks; // <--- ADD THIS EXACT LINE
public class timer_script : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Set the starting time in MINUTES (e.g., 10 for 10:00)")]

    // set startiing time in minutes, can be changed in the inspector
    public float startingTimeMinutes = 10f;
    
    // Internal counters for seconds
    private float p1TimeRemaining;
    private float p2TimeRemaining;
    
    private bool isRunning = false;
    
    // 0 = Not Started, 1 = Timer 1 Active, 2 = Timer 2 Active
    public int activeTimer { get; private set; } = 0;

    // imorting the ui
    [Header("UI References")]
    public TextMeshProUGUI timer1Text;
    public TextMeshProUGUI timer2Text;

    [Header("Debug & Manual Testing")]
    [Tooltip("Check this box while playing to manually force a turn switch!")]
    public bool forceSwitchTurn = false;

    [Header("Debug & Manual Testing")]
    public victory_hide_show_script vicScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p1TimeRemaining = startingTimeMinutes * 60f;
        p2TimeRemaining = startingTimeMinutes * 60f;

        UpdateDisplay(timer1Text, p1TimeRemaining);
        UpdateDisplay(timer2Text, p2TimeRemaining);
    }

    // check for input before starting a timer or switching and ending timers
    void Update()
    {
        // Debug toggle check
        if (forceSwitchTurn)
        {
            forceSwitchTurn = false; 
            SwitchTurn();
        }

        if (!isRunning) return;

        if (activeTimer == 1)
        {
            ProcessTimer(ref p1TimeRemaining, timer1Text, 1);
        }
        else if (activeTimer == 2)
        {
            ProcessTimer(ref p2TimeRemaining, timer2Text, 2);
        }
    }

    // check if there's time left, if yes change the display and reduce the time  
    // else stop the timer and update the visual
    private void ProcessTimer(ref float timeRemaining, TextMeshProUGUI textUI, int timerNum)
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateDisplay(textUI, timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            UpdateDisplay(textUI, timeRemaining);
            isRunning = false; 
            OnTimerZero(timerNum);

            bool redWon = (timerNum == 1)? false : true;
            vicScript.DisplayVictoryScreen(redWon, "Ran Out of time");
        }
    }

    // updates the timer visual 
    private void UpdateDisplay(TextMeshProUGUI textUI, float timeInSeconds)
    {
        float minutes = Mathf.FloorToInt(timeInSeconds / 60);
        float seconds = Mathf.FloorToInt(timeInSeconds % 60);

        textUI.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // function to start the timer
    public void StartTimers()
    {
        isRunning = true;
        activeTimer = 1; 
        Debug.Log("Timers Started! Timer 1 is now ticking.");
    }

    // function to switch the active timer
    [ContextMenu("Switch Turn")]
    public void SwitchTurn()
    {
        if (activeTimer == 1)
        {
            activeTimer = 2;
            Debug.Log("Switched to Timer 2");
        }
        else if (activeTimer == 2)
        {
            activeTimer = 1;
            Debug.Log("Switched to Timer 1");
        }
        // else
        // {
        //     StartTimers(); 
        // }
    }

    // function to pause the timer
    public void PauseTimers()
    {
        isRunning = false;
        Debug.Log("Timers Paused.");
    }

    // function to reset the timer
    public void ResetTimers()
    {
        // 1. Stop the clock and set status back to Not Started
        isRunning = false;
        activeTimer = 0;

        // 2. Refill the time variables based on your starting time
        p1TimeRemaining = startingTimeMinutes * 60f;
        p2TimeRemaining = startingTimeMinutes * 60f;

        // 3. Force the UI to update immediately so the players see the full time again
        UpdateDisplay(timer1Text, p1TimeRemaining);
        UpdateDisplay(timer2Text, p2TimeRemaining);

        Debug.Log("Timers have been reset back to " + startingTimeMinutes + " minutes!");
    }

    // function to display in the logs when a timer hits zero
    private void OnTimerZero(int timerNumber)
    {
        Debug.Log("Timer " + timerNumber + " hit zero!");
    }
}