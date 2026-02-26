using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        BootstrapScene(SceneManager.GetActiveScene().name);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BootstrapScene(scene.name);
    }

    private static void BootstrapScene(string sceneName)
    {

        if (sceneName == "StartScene")
        {
            EnsureController<StartSceneController>("StartSceneController");
            return;
        }

        if (sceneName == "EndScene")
        {
            EnsureController<EndSceneController>("EndSceneController");
            return;
        }

        if (sceneName.StartsWith("Level"))
        {
            EnsureController<LevelSceneController>("LevelSceneController");
        }
    }

    private static void EnsureController<T>(string objectName) where T : Component
    {
        T existing = Object.FindFirstObjectByType<T>();
        if (existing != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject(objectName);
        controllerObject.AddComponent<T>();
    }
}
