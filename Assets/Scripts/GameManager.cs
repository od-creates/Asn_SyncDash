using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void StartGame() => GameSceneManager.Instance.StartGame();
    public void QuitGame() => GameSceneManager.Instance.QuitGame();
    public void Restart() => GameSceneManager.Instance.Restart();
    public void BackToMainMenu() => GameSceneManager.Instance.BackToMainMenu();
}