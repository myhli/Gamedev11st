using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class OpenMainScene
{
    static OpenMainScene()
    {
        EditorApplication.delayCall += CheckAndOpen;
    }

    private static void CheckAndOpen()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += CheckAndOpen;
            return;
        }

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        }
    }
}
