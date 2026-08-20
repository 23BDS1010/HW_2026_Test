using UnityEngine;

public class ConfigLoader : MonoBehaviour
{
    public static ConfigLoader Instance { get; private set; }

    public DoofusDiary Config { get; private set; }

    private readonly DoofusDiary defaultConfig = new DoofusDiary
    {
        player_data = new PlayerData { speed = 3f },
        pulpit_data = new PulpitData
        {
            min_pulpit_destroy_time = 4f,
            max_pulpit_destroy_time = 5f,
            pulpit_spawn_time = 2.5f
        }
    };

    private void Awake()
    {
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();

        Debug.Log($"Speed: {Config.player_data.speed}, SpawnTime: {Config.pulpit_data.pulpit_spawn_time}");
    }

    private void LoadConfig()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("DoofusDiary");

        if (jsonFile == null)
        {
            Debug.LogWarning("DoofusDiary.json not found in Resources. Using default values.");
            Config = defaultConfig;
            return;
        }

        try
        {
            DoofusDiary parsed = JsonUtility.FromJson<DoofusDiary>(jsonFile.text);

            if (parsed == null || parsed.player_data == null || parsed.pulpit_data == null)
            {
                Debug.LogWarning("DoofusDiary.json is malformed. Using default values.");
                Config = defaultConfig;
                return;
            }

            if (parsed.pulpit_data.min_pulpit_destroy_time > parsed.pulpit_data.max_pulpit_destroy_time)
            {
                Debug.LogWarning("min_pulpit_destroy_time > max_pulpit_destroy_time. Swapping values.");
                (parsed.pulpit_data.min_pulpit_destroy_time, parsed.pulpit_data.max_pulpit_destroy_time) =
                    (parsed.pulpit_data.max_pulpit_destroy_time, parsed.pulpit_data.min_pulpit_destroy_time);
            }

            Config = parsed;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse DoofusDiary.json: {e.Message}. Using default values.");
            Config = defaultConfig;
        }
    }
}