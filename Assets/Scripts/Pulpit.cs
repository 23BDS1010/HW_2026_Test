using UnityEngine;
using System.Collections;

public class Pulpit : MonoBehaviour
{
    private float lifetime;
    private float timer;
    private bool hasWarnedSpawn = false;
    private float spawnAheadThreshold;
    private bool fadeStarted = false;
    private bool hasBeenScored = false;

    public System.Action OnShouldSpawnNext;
    public System.Action<Pulpit> OnDestroyed;

    private Renderer rend;
    private Collider col;
    private Color originalColor;
    public float fadeDuration = 1.5f;

   private void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        if (rend == null)
        {
            Debug.LogError($"[Pulpit] No Renderer found on {gameObject.name}. Fade will be skipped.");
        }
        else
        {
            originalColor = rend.material.color;
        }

        if (col == null)
        {
            Debug.LogWarning($"[Pulpit] No Collider found on {gameObject.name}.");
        }
    }

    public void Initialize(float minLife, float maxLife, float spawnAheadTime)
    {
        lifetime = Random.Range(minLife, maxLife);
        timer = 0f;
        spawnAheadThreshold = Mathf.Clamp(spawnAheadTime, 0f, lifetime);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float remaining = lifetime - timer;

        if (!hasWarnedSpawn && remaining <= spawnAheadThreshold)
        {
            hasWarnedSpawn = true;
            OnShouldSpawnNext?.Invoke();
        }

        if (!fadeStarted && remaining <= fadeDuration && remaining > 0f)
        {
            fadeStarted = true;
            StartCoroutine(FadeOut(remaining));
        }

        if (timer >= lifetime)
        {
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeOut(float remainingAtStart)
    {
        if (col != null)
        {
            col.enabled = false; // stop blocking Doofus as soon as fade begins
        }

        float t = 0f;
        float duration = Mathf.Min(fadeDuration, remainingAtStart);

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);

            Color c = originalColor;
            c.a = 1f - progress;
            rend.material.color = c;

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenScored) return;

        if (collision.gameObject.CompareTag("Doofus"))
        {
            hasBeenScored = true;
            ScoreManager.Instance?.AddPoint();
            Debug.Log($"[Pulpit] Doofus landed - scored! id={GetInstanceID()}");
        }
    }
}