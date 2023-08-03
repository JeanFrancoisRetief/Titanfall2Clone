using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;




public class PlayerShoot : NetworkBehaviour
{
    [Header("Gun parameters")]
    public int damage;
    public float shotCooldown;
    [SerializeField] float horizontalSpread, verticalSpread;
    public float horizontalRecoil, verticalRecoil;
    [SerializeField] private int ammocount;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float reloadTime;
    [SerializeField] private float TacreloadTime;
    [SerializeField] bool reloading;


    public float[] recoilvalues; // these are stored in sets of two for each gun

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
    Controller cntrl;


    private void Awake() {
        mainCam = GameObject.FindWithTag("Camera");
        holeMaker = GameObject.FindWithTag("BulletHoler").GetComponent<BulletHoleScript>();

        ADSelement = GameObject.Find("ADSElement");
        Hipelement = GameObject.Find("HipElement");

        ammocount = maxAmmo;
    }

    public override void OnStartClient(){
        if(base.IsOwner){
            cntrl = gameObject.GetComponent<Controller>();
        }
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

        if(reloading){
            ADSelement.SetActive(false);
            Hipelement.SetActive(false);
        }
        else if(ADSactive){
            ADSelement.SetActive(true);
            Hipelement.SetActive(false);
        }else{
            ADSelement.SetActive(false);
            Hipelement.SetActive(true);
        }

        if(Input.GetButton("Fire1")){
            if(fireTimer <= 0 && ammocount > 0 && !reloading){
                ammocount -= 1;
                if(ADSactive){
                    ADSFire();
                }else{
                    hipFire();
                }

            }else if(ammocount <= 0 && !reloading){
                StartCoroutine(reload(true));
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if(ammocount > 0){
                StartCoroutine(reload(false));
            }else{
                StartCoroutine(reload(true));
            }
        }

        if(fireTimer > 0){
            fireTimer -= Time.deltaTime;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void ShootServer(int DMG, Vector3 position, Vector3 direction){
        Debug.DrawRay(position, direction, Color.green, 2f);
        if(Physics.Raycast(position, direction, out RaycastHit hitter) && hitter.transform.TryGetComponent(out Playerhealth enemHealth)){
            Debug.Log(enemHealth);
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
        // Debug.Log("firing");
        
        // float xRecoil = Random.Range(-horizontalRecoil, horizontalRecoil) / 2;
        // float yRecoil = Random.Range(-verticalRecoil, verticalRecoil) / 2;

        // float xRotation = mainCam.transform.rotation.x + xRecoil;
        // float yRotation = mainCam.transform.rotation.x + yRecoil;
        
        // mainCam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        cntrl.recoil = true;

        ShootServer(damage, mainCam.transform.position, mainCam.transform.forward);
        holeMaker.makeHole(mainCam.transform.forward);
        fireTimer = shotCooldown;


    }

    public void respawnreload(){
        ammocount = maxAmmo;
        reloading = false;
    }

    IEnumerator reload(bool isTac){
        if(isTac){
            reloading = true;

            yield return new WaitForSeconds(TacreloadTime);
            ammocount = maxAmmo;
            reloading = false;
        }else{
            reloading = true;

            yield return new WaitForSeconds(reloadTime);
            ammocount = maxAmmo;
            reloading = false;
        }
    }
}
