using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine.UI;


public class Playerhealth : NetworkBehaviour
{
    //Health and regen
    [SyncVar] public int health = 100;
    [SerializeField] private int maxhealth;

    float healthtick;
    [SerializeField] private float tickTime;

    private Text healthText;
    [SerializeField] private bool isTarget;

    //Respawning
    public GameObject[] respawnLocations;
    GameObject prompt;
    GameObject ADSelement;
    GameObject Hipelement;
    GameObject Reticleelement;

    bool despawned;
    bool respawnReady = false;

    private void Start() {
        //healthText = GameObject.FindWithTag("HealthText").GetComponent<Text>();

        respawnLocations = GameObject.FindGameObjectsWithTag("respawnLoc");
    }

    public override void OnStartClient(){
        // base.OnStartClient();
        if(base.IsOwner){
            
            
            prompt = GameObject.Find("Respawnprompt");
            prompt.SetActive(false);    

            ADSelement = GameObject.Find("ADSElement");
            // Debug.Log(ADSelement);
            Hipelement = GameObject.Find("HipElement"); 
            Reticleelement = GameObject.Find("Reticle");       
        }else if(!isTarget){
            // Debug.Log("gone");
            gameObject.GetComponent<Playerhealth>().enabled = false;
        }
    }


    private void Update() {
        if(isTarget){
            if(health > 0){
                return;
            }else{
                Destroy(gameObject);
            }
        }
        
        
        // if(!base.IsOwner){
        //     return;
        // }

        if(health <= 0){
            Debug.Log(health);
        }

        // if(Input.GetButton("Jump")){
        //     transform.position = respawnLocations[1].transform.position;
        // } Teleportation test

        if(despawned){
            gameObject.transform.position = new Vector3(100, 100, 100);
            if(Input.GetButton("Jump") && respawnReady){
                respawn();
            }
            return;
        }
        if(health == 0 && !despawned){
            despawn();
            return;
        }
        
        //healthText.text =  "Health: " + health.ToString();

        if(health < maxhealth && healthtick <= 0){
            health += 1;
            healthtick = tickTime;
        }else if(health > maxhealth){
            health = maxhealth;
        }

        if(healthtick > 0){
            healthtick -= Time.deltaTime;
        }

    }

    public void takedamage(int dmg){
        // if(base.IsOwner) return;

        health -= dmg;
    }


    void despawn(){
        despawned = true;
        Transform playerCam = GameObject.FindWithTag("Camera").transform;

        playerCam.parent = null;

        gameObject.GetComponent<CapsuleCollider>().enabled = false;

        gameObject.GetComponent<PlayerShoot>().enabled = false;
        ADSelement.GetComponent<Image>().enabled = false;
        ADSelement.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
        Hipelement.GetComponent<Image>().enabled = false;
        Reticleelement.GetComponent<Image>().enabled = false;
        gameObject.transform.position = new Vector3(100, 100, 100);
        
        StartCoroutine(respawnwait());
    }

    void respawn(){
        health = maxhealth;
        
        Transform playerCam = GameObject.FindWithTag("Camera").transform;
        playerCam.transform.position = new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z);
        playerCam.transform.SetParent(transform);

        gameObject.GetComponent<CapsuleCollider>().enabled = true;

        prompt.SetActive(false);

        gameObject.GetComponent<PlayerShoot>().enabled = true;
        gameObject.GetComponent<PlayerShoot>().respawnreload();

        int spawnpoint = Random.Range(0, respawnLocations.Length);

        transform.position = respawnLocations[spawnpoint].transform.position;

        respawnReady = false;
        despawned = false;

        Debug.Log("respawn");
    }

    IEnumerator respawnwait(){
        yield return new WaitForSeconds(2f);

        respawnReady = true;
        prompt.SetActive(true);
    }
}
