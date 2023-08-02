using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine.UI;


public class Playerhealth : NetworkBehaviour
{
    [SyncVar] public int health = 10;
    [SerializeField] private int maxhealth;

    float healthtick;
    [SerializeField] private float tickTime;

    private Text healthText;

    private void Start() {
        healthText = GameObject.FindWithTag("HealthText").GetComponent<Text>();
    }

    private void Update() {
        if(!base.IsOwner){
            return;
        }
        
        healthText.text =  "Health: " + health.ToString();

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
        if(base.IsOwner) return;

        health -= dmg;
    }
}
