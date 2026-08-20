using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    StartMenu,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.StartMenu;
    public System.Action<GameState> OnStateChanged;

    private bool isRestarting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(GameState.StartMenu);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"State: {newState}");
        OnStateChanged?.Invoke(newState);
    }

    public void StartGame()
    {
        ScoreManager.Instance?.ResetScore();
        SetState(GameState.Playing);
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
    }

    public void RestartGame()
    {
        if (isRestarting) return;

        isRestarting = true;
        StartCoroutine(ReloadScene());
    }

    private System.Collections.IEnumerator ReloadScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null;

        StartGame();
        isRestarting = false;
    }
}