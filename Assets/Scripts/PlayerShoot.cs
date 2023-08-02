using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;




public class PlayerShoot : NetworkBehaviour
{
    [Header("Gun parameters")]
    public int damage;
    public int shotCooldown;
    [SerializeField] float horizontalSpread, verticalSpread;
    [SerializeField] float horizontalRecoil, verticalRecoil;
    [SerializeField] private int ammocount;
    [SerializeField] private int reloadTime;

    public float[] recoilvalues; // the are stored in sets of two for each gun

    bool ADSactive;


    [Header("Sprite management")]
    public Sprite[] ADSsprites;
    public Sprite[] Hipsprites;
    public Sprite[] Reticlesprites;


    public GameObject ADSelement;
    public GameObject Hipelement;
    public GameObject Reticleelement;


    [Header("Other")]
    float fireTimer;
    GameObject mainCam;
    BulletHoleScript holeMaker;
    // Controller cntrl;


    private void Awake() {
        mainCam = GameObject.Find("Main Camera");
        holeMaker = GameObject.FindWithTag("BulletHoler").GetComponent<BulletHoleScript>();

        ADSelement = GameObject.Find("ADSElement");
        Hipelement = GameObject.Find("HipElement");

    }

    // Update is called once per frame
    void Update()
    {
        if(!base.IsOwner) return;

        if(Input.GetMouseButton(1)){ 
            ADSactive = true;
        }else{
            ADSactive = false;
        }

        if(ADSactive){
            ADSelement.SetActive(true);
            Hipelement.SetActive(false);
        }else{
            ADSelement.SetActive(false);
            Hipelement.SetActive(true);
        }

        if(Input.GetButton("Fire1")){
            if(fireTimer <= 0){
                if(ADSactive){
                    ADSFire();
                }else{
                    hipFire();
                }

            }
        }

        if(fireTimer > 0){
            fireTimer -= Time.deltaTime;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void ShootServer(int DMG, Vector3 position, Vector3 direction){
        if(Physics.Raycast(position, direction, out RaycastHit hitter) && hitter.transform.TryGetComponent(out Playerhealth enemHealth)){
            enemHealth.takedamage(damage);
        }
    }


    void hipFire(){
        float xspread = Random.Range(-horizontalSpread, horizontalSpread);
        float yspread = Random.Range(-verticalSpread, verticalSpread);

        Vector3 shotDirection = mainCam.transform.forward + new Vector3(xspread, yspread, 0);
        
        
        ShootServer(damage, mainCam.transform.position, shotDirection);
        holeMaker.makeHole(shotDirection);
        fireTimer = shotCooldown;
    }

    void ADSFire(){
        float xRecoil = Random.Range(-horizontalRecoil, horizontalRecoil) / 2;
        float yRecoil = Random.Range(-verticalRecoil, verticalRecoil) / 2;


        float xRotation = mainCam.transform.rotation.x + xRecoil;
        float yRotation = mainCam.transform.rotation.x + yRecoil;
        
        mainCam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        ShootServer(damage, mainCam.transform.position, mainCam.transform.position);
        holeMaker.makeHole(mainCam.transform.position);
        fireTimer = shotCooldown;


    }
}
