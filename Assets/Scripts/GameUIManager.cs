using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        startPanel.SetActive(state == GameState.StartMenu);
        gamePanel.SetActive(state == GameState.Playing);
        gameOverPanel.SetActive(state == GameState.GameOver);

        if (state == GameState.GameOver && ScoreManager.Instance != null)
        {
            finalScoreText.text = $"Final Score: {ScoreManager.Instance.CurrentScore}";
        }
    }

    public void OnStartButtonPressed()
    {
        GameManager.Instance?.StartGame();
    }

    public void OnRetryButtonPressed()
    {
        GameManager.Instance?.RestartGame();
    }
}