using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deathfield : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<Playerhealth>() != null){
            other.GetComponent<Playerhealth>().health = -1;
        }

    }
}
