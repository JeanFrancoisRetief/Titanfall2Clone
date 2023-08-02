using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleDespawner : MonoBehaviour
{
    private void Awake() {
        Destroy(gameObject, 8f);   
    }

}
