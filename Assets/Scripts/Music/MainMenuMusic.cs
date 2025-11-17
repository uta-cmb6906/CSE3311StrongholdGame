using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip backgroundMusic;

    void Start()
    {
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
