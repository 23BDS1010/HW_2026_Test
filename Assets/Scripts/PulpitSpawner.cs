using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PulpitSpawner : MonoBehaviour
{
    public GameObject pulpitPrefab;
    public float pulpitSize = 9f;

    private List<GameObject> activePulpits = new List<GameObject>();
    private Vector3 latestPulpitPosition = Vector3.zero;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing && activePulpits.Count == 0)
        {
            SpawnPulpit(Vector3.zero);
        }
    }

    private void SpawnPulpit(Vector3 position)
    {
        GameObject newPulpitObj = Instantiate(pulpitPrefab, position, Quaternion.identity);
        Pulpit pulpitScript = newPulpitObj.GetComponent<Pulpit>();

        var config = ConfigLoader.Instance.Config.pulpit_data;
        pulpitScript.Initialize(config.min_pulpit_destroy_time, config.max_pulpit_destroy_time, config.pulpit_spawn_time);

        pulpitScript.OnShouldSpawnNext += TrySpawnNext;
        pulpitScript.OnDestroyed += HandlePulpitDestroyed;

        activePulpits.Add(newPulpitObj);
        latestPulpitPosition = position;

        Debug.Log($"[Spawner] Spawned at {position}. Active count: {activePulpits.Count}");
    }

    private void HandlePulpitDestroyed(Pulpit destroyedPulpit)
    {
        activePulpits.Remove(destroyedPulpit.gameObject);
        Debug.Log($"[Spawner] Pulpit removed. Active count now: {activePulpits.Count}");
        TrySpawnNext();
    }

    private void TrySpawnNext()
    {
        if (activePulpits.Count >= 2)
        {
            Debug.Log("[Spawner] Skipped - already 2 active.");
            return;
        }

        Vector3[] allDirections = new Vector3[]
        {
            new Vector3(pulpitSize, 0, 0),
            new Vector3(-pulpitSize, 0, 0),
            new Vector3(0, 0, pulpitSize),
            new Vector3(0, 0, -pulpitSize)
        };

        List<Vector3> validDirections = new List<Vector3>();
        foreach (var dir in allDirections)
        {
            Vector3 candidatePosition = latestPulpitPosition + dir;
            bool overlaps = activePulpits.Any(p =>
                p != null && Vector3.Distance(p.transform.position, candidatePosition) < 0.5f);

            if (!overlaps)
            {
                validDirections.Add(dir);
            }
        }

        if (validDirections.Count == 0)
        {
            validDirections = allDirections.ToList();
        }

        Vector3 chosenOffset = validDirections[Random.Range(0, validDirections.Count)];
        Vector3 newPosition = latestPulpitPosition + chosenOffset;

        SpawnPulpit(newPosition);
    }
}