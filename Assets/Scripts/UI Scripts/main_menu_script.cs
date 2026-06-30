using UnityEngine;
using UnityEngine.SceneManagement; // You MUST include this to load scenes!

public class main_menu_script : MonoBehaviour
{
    public void playPress(){
        SceneManager.LoadScene("MainScene");
    }
    
    public void quitPress(){
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // // This is the function you will call to quit the game
    // public void QuitGame()
    // {
    //     Debug.Log("Quit Game requested!");

    //     #if UNITY_EDITOR
    //             // Stops the play mode inside the Unity Editor
    //             UnityEditor.EditorApplication.isPlaying = false;
    //     #else
    //             // Quits the actual application when it is a built game
    //             Application.Quit();
    //     #endif
    // }
}
