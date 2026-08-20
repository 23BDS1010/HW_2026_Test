using UnityEngine;

public class Pulpit : MonoBehaviour
{
    private float lifetime;
    private float timer;
    private bool hasWarnedSpawn = false;
    private float spawnAheadThreshold;

    public System.Action OnShouldSpawnNext;
    public System.Action<Pulpit> OnDestroyed;
    public bool IsDestroyed { get; private set; } = false;

    public void Initialize(float minLife, float maxLife, float spawnAheadTime)
    {
        lifetime = Random.Range(minLife, maxLife);
        timer = 0f;
        spawnAheadThreshold = Mathf.Clamp(spawnAheadTime, 0f, lifetime);
        Debug.Log($"[Pulpit Init] Lifetime: {lifetime:F2}, SpawnThreshold: {spawnAheadThreshold:F2}, id={GetInstanceID()}");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float remaining = lifetime - timer;

        if (!hasWarnedSpawn && remaining <= spawnAheadThreshold)
        {
            Debug.Log($"[EARLY WARNING] id={GetInstanceID()} remaining={remaining:F2} time={Time.time:F2}");
            hasWarnedSpawn = true;
            OnShouldSpawnNext?.Invoke();
        }

        if (timer >= lifetime)
        {
            Debug.Log($"[LIFETIME EXPIRED] id={GetInstanceID()} time={Time.time:F2}");

            // Safety net: if early warning never fired for some reason, still trigger next
            if (!hasWarnedSpawn)
            {
                hasWarnedSpawn = true;
                OnShouldSpawnNext?.Invoke();
            }

            IsDestroyed = true;
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }
}