using UnityEngine;
using System.Collections;

public class EventHandler : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip spinClip, bombClip;
    public AudioClip openDoorClip, stepClip, burstClip, enemyHurt3Clip, enemyDieClip, injureClip, dyingClip;
    public GameObject doorCollider;
    void Awake()
    {
        audio = Camera.main.GetComponent<AudioSource>();
    }

    public void SpinSound()
    {
        audio.PlayOneShot(spinClip, .2f);
    }
    public void BombSound()
    {
        audio.PlayOneShot(bombClip, 1f);
    }
    public void OpenDoor()
    {
        doorCollider.SetActive(false);
    }
    public void OpenDoorSound()
    {
        audio.PlayOneShot(openDoorClip, 1f);
    }
    public void StepSound()
    {
        audio.PlayOneShot(stepClip, 1f);
    }
    public void BurstSound()
    {
        audio.PlayOneShot(burstClip, 1f);
    }
    public void EnemyHurt3Sound()
    {
        audio.PlayOneShot(enemyHurt3Clip, 1f);
    }
    public void EnemyDieSound()
    {
        audio.PlayOneShot(enemyDieClip, 1f);
    }
    public void InjureSound()
    {
        audio.PlayOneShot(injureClip, 1f);
    }
    public void DyingSound()
    {
        audio.PlayOneShot(dyingClip, 1f);
    }
    public void ActivateBombExplotion()
    {

        GameObject bomb = gameObject.transform.parent.gameObject;
        BoxCollider collider = bomb.GetComponent<BoxCollider>();
        bomb.GetComponent<Rigidbody>().useGravity = false;
        //collider.size = new Vector2(.2f, .2f);
        bomb.tag = "Weapon";

        collider.isTrigger = true;
    }
    public void RestartLevel()
    {
        Application.LoadLevel(0);
    }

}
