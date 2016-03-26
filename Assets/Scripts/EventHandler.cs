using UnityEngine;
using System.Collections;

public class EventHandler : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip spinClip;
    void Awake()
    {
        audio = Camera.main.GetComponent<AudioSource>();
    }

    public void SpinSound()
    {
        audio.PlayOneShot(spinClip);
    }
}
