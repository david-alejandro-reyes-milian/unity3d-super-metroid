using UnityEngine;
using System.Collections;

public class EventHandler : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip spinClip;
    public AudioClip bombClip;
    public AudioClip openDoorClip, stepClip, burstClip;
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
        doorCollider.SetActive(false);
    }
    public void OpenDoorSound()
    {
        audio.PlayOneShot(openDoorClip, .5f);
    }
    public void StepSound()
    {
        audio.PlayOneShot(stepClip, .6f);
    }
    public void BurstSound()
    {
        audio.PlayOneShot(burstClip, .6f);
    }

}
