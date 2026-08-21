using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public float punchScale = 1.4f;
    public float punchDuration = 0.25f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = scoreText.transform.localScale;
    }

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is null in ScoreUI.Start()");
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }

    private void UpdateScoreDisplay(int score)
    {
        scoreText.text = $"<color=white>Score:</color> <color=yellow>{score}</color>";

        StopAllCoroutines();
        StartCoroutine(PunchAnimation());
    }

    private IEnumerator PunchAnimation()
    {
        float t = 0f;
        scoreText.transform.localScale = originalScale * punchScale;

        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float progress = t / punchDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, progress);
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }
}