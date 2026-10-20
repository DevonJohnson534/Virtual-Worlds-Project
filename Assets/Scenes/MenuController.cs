using UnityEngine;
using UnityEngine.SceneManagement; //we use this one to load scenes.

public class MenuController : MonoBehaviour
{
    // This is called by our select/buttons to bring us to a new scene.
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        // This is for testing in the Unity editor, which will stop play mode.
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
