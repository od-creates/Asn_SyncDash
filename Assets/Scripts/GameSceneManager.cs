using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] string _MainMenuScene = "Start";
    [SerializeField] string _GameScene = "Game";
    [SerializeField] string _GameOverScene = "End";

    float mScore, mObjectSpeed;
    bool mIsGameRunning;

    TextMeshProUGUI mScoreTMP, mFinalScoreTMP;

    void Awake()
    {
        // Singleton + persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == _MainMenuScene)
        {
            mIsGameRunning = false;
            Time.timeScale = 0f;
        }
        else if (scene.name == _GameScene)
        {
            mScore = 0f;
            mObjectSpeed = 0f;
            mIsGameRunning = true;
            Time.timeScale = 1f;
            //fetch the score text
            var scoreGO = GameObject.FindWithTag("ScoreText");
            if (scoreGO != null)
                mScoreTMP = scoreGO.GetComponent<TextMeshProUGUI>();
        }
        else if (scene.name == _GameOverScene)
        {
            mIsGameRunning = false;
            Time.timeScale = 0f;
            //fetch the final score text
            var finalGO = GameObject.FindWithTag("FinalScoreText");
            if (finalGO != null)
            {
                mFinalScoreTMP = finalGO.GetComponent<TextMeshProUGUI>();
                mFinalScoreTMP.text = Mathf.FloorToInt(mScore).ToString();
            }
        }
    }

    void Update()
    {
        if (mIsGameRunning && mScoreTMP != null)
        {
            mScore += Time.deltaTime / 2; //increment score with time
            mObjectSpeed += Time.deltaTime; //increment objectSpeed with time
            mScoreTMP.text = Mathf.FloorToInt(mScore).ToString();//update score on Game UI
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(_GameScene);
    }

    public void GameOver()
    {
        SceneManager.LoadScene(_GameOverScene);
    }

    public void Restart()
    {
        StartGame();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(_MainMenuScene);
    }

    public void UpdateScore(int orbCollectionScore)
    {
        mScore += orbCollectionScore;
    }

    public float GetObjectSpeed()
    {
        return mObjectSpeed;
    }

    public void SetObjectSpeed(float speed)
    {
        mObjectSpeed += speed;
    }

    /// <summary>
    /// Quits the application. In the Editor it stops play mode.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stop play mode in the Editor
        EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }
}