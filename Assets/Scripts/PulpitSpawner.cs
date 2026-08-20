using System.Collections.Generic;
using UnityEngine;

public class PulpitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pulpitPrefab;
    [SerializeField] private Transform initialSpawnPoint;
    [SerializeField] private float pulpitY = 0.5f;

    private float adjacentDistance; // computed automatically from prefab size

    private float minLife;
    private float maxLife;
    private float spawnAheadTime;

    private List<Pulpit> activePulpits = new List<Pulpit>();
    private Vector3 lastSpawnPosition;

    private void Start()
    {
        LoadConfigValues();

        if (!ValidateSetup())
            return;

        ComputeAdjacentDistance();

        lastSpawnPosition = initialSpawnPoint.position;
        SpawnPulpit(lastSpawnPosition);
    }

    private void LoadConfigValues()
    {
        if (ConfigLoader.Instance == null || ConfigLoader.Instance.Config == null)
        {
            Debug.LogError("[PulpitSpawner] ConfigLoader.Instance or Config is null! Using fallback values.");
            minLife = 4f;
            maxLife = 5f;
            spawnAheadTime = 2.5f;
            return;
        }

        PulpitData data = ConfigLoader.Instance.Config.pulpit_data;
        minLife = data.min_pulpit_destroy_time;
        maxLife = data.max_pulpit_destroy_time;
        spawnAheadTime = data.pulpit_spawn_time;

        Debug.Log($"[PulpitSpawner] Config loaded — minLife={minLife}, maxLife={maxLife}, spawnAheadTime={spawnAheadTime}");
    }

    private bool ValidateSetup()
    {
        if (pulpitPrefab == null)
        {
            Debug.LogError("[PulpitSpawner] Pulpit Prefab is not assigned in the Inspector.");
            return false;
        }

        if (initialSpawnPoint == null)
        {
            Debug.LogError("[PulpitSpawner] Initial Spawn Point is not assigned in the Inspector.");
            return false;
        }

        return true;
    }

    private void ComputeAdjacentDistance()
    {
        Renderer renderer = pulpitPrefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Vector3 size = renderer.bounds.size;
            adjacentDistance = Mathf.Max(size.x, size.z);
            Debug.Log($"[PulpitSpawner] Computed adjacentDistance from prefab bounds: {adjacentDistance}");
        }
        else
        {
            Debug.LogWarning("[PulpitSpawner] No Renderer found on Pulpit Prefab. Falling back to distance = 5.");
            adjacentDistance = 5f;
        }
    }

    private Vector3 GetNextAdjacentPosition(Vector3 fromPosition)
    {
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,  // +Z
            Vector3.back,     // -Z
            Vector3.right,    // +X
            Vector3.left      // -X
        };

        Vector3 chosenDirection = directions[Random.Range(0, directions.Length)];
        Vector3 offset = chosenDirection * adjacentDistance;

        Vector3 next = fromPosition + offset;
        next.y = pulpitY;
        return next;
    }

    private void SpawnPulpit(Vector3 position)
    {
        if (activePulpits.Count >= 2)
        {
            Debug.Log($"[SPAWN BLOCKED] active count={activePulpits.Count}");
            return;
        }

        GameObject go = Instantiate(pulpitPrefab, position, Quaternion.identity);
        Pulpit pulpit = go.GetComponent<Pulpit>();

        if (pulpit == null)
        {
            Debug.LogError("[PulpitSpawner] Pulpit Prefab has no Pulpit component attached.");
            Destroy(go);
            return;
        }

        pulpit.Initialize(minLife, maxLife, spawnAheadTime);

        pulpit.OnShouldSpawnNext += () => SpawnNextAdjacentTo(position);
        pulpit.OnDestroyed += HandlePulpitDestroyed;

        activePulpits.Add(pulpit);
        lastSpawnPosition = position;
        Debug.Log($"[SPAWNED] id={pulpit.GetInstanceID()} pos={position} active count={activePulpits.Count} time={Time.time:F2}");
    }

    private void SpawnNextAdjacentTo(Vector3 previousPosition)
    {
        Vector3 nextPos = GetNextAdjacentPosition(previousPosition);
        SpawnPulpit(nextPos);
    }

    private void HandlePulpitDestroyed(Pulpit pulpit)
    {
        activePulpits.Remove(pulpit);
        pulpit.OnDestroyed -= HandlePulpitDestroyed;
        Debug.Log($"[REMOVED] id={pulpit.GetInstanceID()} active count={activePulpits.Count} time={Time.time:F2}");

        if (activePulpits.Count < 2)
        {
            Vector3 referencePosition;

            if (activePulpits.Count > 0)
            {
                // Spawn next to whichever pulpit is still alive
                referencePosition = activePulpits[activePulpits.Count - 1].transform.position;
            }
            else
            {
                // No pulpits left at all — fall back to last known position
                referencePosition = lastSpawnPosition;
            }

            SpawnNextAdjacentTo(referencePosition);
        }
    }
}