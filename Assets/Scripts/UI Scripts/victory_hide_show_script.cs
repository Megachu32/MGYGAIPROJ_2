using UnityEngine;
using TMPro; 

public class Victory_Hide_Show : MonoBehaviour
{

    
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI reasonText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayVictoryScreen(bool redWon, string winReason)
    {
        // 1. Check the boolean: If redWon is true, set the winner text to "Red". Else, "Black".
        string message = "";
        if (redWon)
        {
            message = "Red Won!";
        } else
        {
            message = "Black Won!";
        }

        // FIXED: You access the '.text' property to change what the component says
        winnerText.text = message;
        
        // 2. Set the reason text to match the 'winReason' string (e.g., "Win by Time").
        reasonText.text = winReason;
        
        // 3. Show the panel: gameObject.SetActive(true);
        gameObject.SetActive(true);
    }
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}