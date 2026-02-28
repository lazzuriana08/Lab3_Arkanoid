using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
        }
    }

    void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter
        };

        string title = GameSession.DidWin ? "VOCÊ VENCEU!" : "";
        GUI.Label(new Rect(0, 110, Screen.width, 70), title, titleStyle);
        GUI.Label(new Rect(0, 265, Screen.width, 50), $"Pontuação Final: {GameSession.Score}", textStyle);

        Rect buttonRect = new Rect((Screen.width - 260) / 2f, 310, 260, 50);
        if (GUI.Button(buttonRect, "Reiniciar"))
        {
            Restart();
        }

        GUI.Label(new Rect(0, 380, Screen.width, 40), "Pressione ENTER ou ESPAÇO para reiniciar.", textStyle);
    }

    private void Restart()
    {
        GameSession.ResetForNewGame();
        SceneManager.LoadScene("StartScene");
    }
}
