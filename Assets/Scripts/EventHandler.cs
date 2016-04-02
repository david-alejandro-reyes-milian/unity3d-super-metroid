using UnityEngine;
using System.Collections;

public class EventHandler : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip spinClip;
    public AudioClip bombClip;
    void Awake()
    {
        audio = Camera.main.GetComponent<AudioSource>();
    }

    public void SpinSound()
    {
        audio.PlayOneShot(spinClip);
    }
    public void BombSound()
    {
        audio.PlayOneShot(bombClip, .5f);
    }
}
