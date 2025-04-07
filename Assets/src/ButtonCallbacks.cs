using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonCallbacks : MonoBehaviour
{
    public void StartButtonPressed(SceneAsset scene)
    {
        SceneManager.LoadScene($"Scenes/{scene.name}");
    }

    public void OptionsButtonPressed()
    {
        
    }

    public void QuitButtonPressed()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}
