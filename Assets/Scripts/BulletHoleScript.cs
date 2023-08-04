using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleScript : MonoBehaviour
{
    public GameObject[] bulletHolePrefabs;

    // Update is called once per frame
    // void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.Mouse0)){
            
    //     }
    // }

    public void makeHole(Vector3 angle){
        RaycastHit hitInfo;
        if(Physics.Raycast(transform.position, angle, out hitInfo)){
            // Debug.Log(hitInfo);
            int a = Random.Range(0, 2);
            GameObject obj = Instantiate(bulletHolePrefabs[a], hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            obj.transform.position += obj.transform.forward/1000;
        }
    }
}
