using System.Collections.Generic;
using UnityEngine;

public class PulpitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pulpitPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minLife = 4f;
    [SerializeField] private float maxLife = 6f;
    [SerializeField] private float spawnAheadTime = 2.5f;

    private List<Pulpit> activePulpits = new List<Pulpit>();
    private int nextSpawnIndex = 0;

    private void Start()
    {
        if (!ValidateSetup())
            return;

        SpawnPulpit();
    }

    private bool ValidateSetup()
    {
        if (pulpitPrefab == null)
        {
            Debug.LogError("[PulpitSpawner] Pulpit Prefab is not assigned in the Inspector.");
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[PulpitSpawner] No Spawn Points assigned! Set the array size and drag Transforms into it in the Inspector.");
            return false;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError($"[PulpitSpawner] Spawn Points element {i} is empty (null). Assign a Transform there.");
                return false;
            }
        }

        return true;
    }

    private void SpawnPulpit()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[PulpitSpawner] No spawn points assigned! Aborting spawn.");
            return;
        }

        // Hard cap: never exceed 2 simultaneous pulpits
        if (activePulpits.Count >= 2)
        {
            Debug.Log($"[SPAWN BLOCKED] active count={activePulpits.Count}");
            return;
        }

        Transform point = spawnPoints[nextSpawnIndex % spawnPoints.Length];
        nextSpawnIndex++;

        if (point == null)
        {
            Debug.LogError($"[PulpitSpawner] Spawn point at index {(nextSpawnIndex - 1) % spawnPoints.Length} is null. Aborting spawn.");
            return;
        }

        GameObject go = Instantiate(pulpitPrefab, point.position, point.rotation);
        Pulpit pulpit = go.GetComponent<Pulpit>();

        if (pulpit == null)
        {
            Debug.LogError("[PulpitSpawner] Pulpit Prefab has no Pulpit component attached.");
            Destroy(go);
            return;
        }

        pulpit.Initialize(minLife, maxLife, spawnAheadTime);

        // IMPORTANT: subscribe to BOTH events
        pulpit.OnShouldSpawnNext += SpawnPulpit;
        pulpit.OnDestroyed += HandlePulpitDestroyed;

        activePulpits.Add(pulpit);
        Debug.Log($"[SPAWNED] id={pulpit.GetInstanceID()} active count={activePulpits.Count} time={Time.time:F2}");
    }

    private void HandlePulpitDestroyed(Pulpit pulpit)
    {
        activePulpits.Remove(pulpit);
        pulpit.OnShouldSpawnNext -= SpawnPulpit;
        pulpit.OnDestroyed -= HandlePulpitDestroyed;
        Debug.Log($"[REMOVED] id={pulpit.GetInstanceID()} active count={activePulpits.Count} time={Time.time:F2}");
    }
}