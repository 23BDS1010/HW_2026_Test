using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; } = 0;

    public System.Action<int> OnScoreChanged;

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

    public void AddPoint()
    {
        CurrentScore++;

        Debug.Log($"[Score] +1 -> Total: {CurrentScore}");

        OnScoreChanged?.Invoke(CurrentScore);

        if (CurrentScore >= 50 && GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }

    }

    public void ResetScore()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}