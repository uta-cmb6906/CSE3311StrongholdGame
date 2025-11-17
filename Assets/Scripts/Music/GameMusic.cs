using System.Collections;
using UnityEngine;

public class GameMusic : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] backgroundMusic;

    void Start()
    {
        PlayRandomMusic();
    }



    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }

    void PlayRandomMusic()
    {
        if (backgroundMusic.Length == 0) return;

        AudioClip randomClip = backgroundMusic[Random.Range(0, backgroundMusic.Length)];
        audioSource.clip = randomClip;
        audioSource.Play();
    }
}
