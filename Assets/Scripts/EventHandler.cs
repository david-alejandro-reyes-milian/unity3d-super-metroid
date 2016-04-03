using UnityEngine;
using System.Collections;

public class EventHandler : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip spinClip;
    public AudioClip bombClip;
    public AudioClip openDoorClip;
    public GameObject doorCollider;
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
    public void OpenDoor()
    {
        doorCollider.active = false;
    }
    public void OpenDoorSound()
    {
        audio.PlayOneShot(openDoorClip, .5f);
    }

}
