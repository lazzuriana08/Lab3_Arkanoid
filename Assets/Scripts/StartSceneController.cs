using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    void Start()
    {
        GameSession.ResetForNewGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(0, 170, Screen.width, 120), "Destrua todos os blocos coloridos com a bola.\nColete power-ups e sobreviva com suas vidas.", textStyle);

        Rect buttonRect = new Rect((Screen.width - 220) / 2f, 320, 220, 50);
        if (GUI.Button(buttonRect, "Iniciar Jogo"))
        {
            StartGame();
        }

        GUI.Label(new Rect(0, 390, Screen.width, 40), "Pressione ESPAÇO para lançar a bola durante a fase.", textStyle);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }
}
