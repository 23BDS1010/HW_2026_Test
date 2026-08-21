using UnityEngine;

public class MusicToggle : MonoBehaviour
{
    public AudioSource musicSource;

    public void ToggleMute()
    {
        musicSource.mute = !musicSource.mute;
    }
}