using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    Animator anim;
    public bool doorIsOpen = false;
    public GameObject doorCollider;
    void Awake()
    {
        Transform door = gameObject.transform.parent.parent;
        doorCollider = door.Find("3DCollider").gameObject;
        anim = gameObject.transform.parent.GetComponentInChildren<Animator>();
        //if (doorIsOpen)
        //    anim.SetTrigger("OpenDoor");
        //else { anim.SetTrigger("CloseDoor"); }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Shot")
        {
            doorIsOpen = !doorIsOpen;
            if (doorIsOpen) anim.SetTrigger("OpenDoor");
        }
    }
}
