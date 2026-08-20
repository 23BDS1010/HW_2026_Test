using UnityEngine;

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
        Debug.Log($"[GameManager] State changed to: {newState}");
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

    private bool isRestarting = false;

    public void RestartGame()
    {
        if (isRestarting) return;
        isRestarting = true;
        StartCoroutine(ReloadAndStart());
    }

    private System.Collections.IEnumerator ReloadAndStart()
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var loadOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        yield return loadOp;
        yield return null;

        StartGame();
        isRestarting = false;
    }
}