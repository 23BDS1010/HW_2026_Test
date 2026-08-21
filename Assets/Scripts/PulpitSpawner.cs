using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PulpitSpawner : MonoBehaviour
{
    public GameObject pulpitPrefab;
    public float pulpitSize = 9f;

    private List<GameObject> activePulpits = new List<GameObject>();
    private Vector3 latestPulpitPosition = Vector3.zero;
    private Vector3 lastDirection = Vector3.zero;
    private bool gameActive = false;

    private Color[] palette = new Color[] {
        new Color(0f, 0.6f, 0f),
        new Color(0.13f, 0.8f, 0.13f),
        new Color(0.4f, 1f, 0.4f)
    };
    private Color lastUsedColor = Color.green;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            gameActive = true;
            if (activePulpits.Count == 0)
            {
                SpawnPulpit(Vector3.zero);
            }
        }
        else
        {
            gameActive = false;

            foreach (var p in activePulpits)
            {
                if (p != null) Destroy(p);
            }
            activePulpits.Clear();
        }
    }

    private void SpawnPulpit(Vector3 position)
    {
        if (!gameActive) return;

        if (ConfigLoader.Instance == null || ConfigLoader.Instance.Config == null)
        {
            Debug.LogError("[Spawner] Config not ready — cannot spawn pulpit yet.");
            return;
        }

        GameObject newPulpitObj = Instantiate(pulpitPrefab, position, Quaternion.identity);
        Pulpit pulpitScript = newPulpitObj.GetComponent<Pulpit>();

        var config = ConfigLoader.Instance.Config.pulpit_data;
        pulpitScript.Initialize(config.min_pulpit_destroy_time, config.max_pulpit_destroy_time, config.pulpit_spawn_time);

        Color chosenColor;
        do
        {
            chosenColor = palette[Random.Range(0, palette.Length)];
        } while (chosenColor == lastUsedColor);

        pulpitScript.SetColor(chosenColor);
        lastUsedColor = chosenColor;

        pulpitScript.OnShouldSpawnNext += TrySpawnNext;
        pulpitScript.OnDestroyed += HandlePulpitDestroyed;

        activePulpits.Add(newPulpitObj);
        latestPulpitPosition = position;

        Debug.Log($"[Spawner] Spawned at {position}. Active count: {activePulpits.Count}");
    }

    private void HandlePulpitDestroyed(Pulpit destroyedPulpit)
    {
        if (!gameActive) return;

        activePulpits.Remove(destroyedPulpit.gameObject);

        Debug.Log($"[Spawner] Pulpit removed. Active count now: {activePulpits.Count}");

        TrySpawnNext();
    }

    [SerializeField] private float platformSpeedMultiplier = 1.5f;

    public void MovePulpitsTowardPlayer(Transform player, Vector3 playerMovement)
    {
        if (!gameActive) return;

        Vector3 scaledMovement = playerMovement * platformSpeedMultiplier;

        foreach (var pulpit in activePulpits)
        {
            if (pulpit != null)
            {
                var pulpitScript = pulpit.GetComponent<Pulpit>();
                pulpitScript?.MoveBy(scaledMovement);
            }
        }

        latestPulpitPosition -= scaledMovement;
    }

    private void TrySpawnNext()
    {
        if (!gameActive) return;

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
        lastDirection = chosenOffset;

        Vector3 newPosition = latestPulpitPosition + chosenOffset;

        SpawnPulpit(newPosition);
    }
}