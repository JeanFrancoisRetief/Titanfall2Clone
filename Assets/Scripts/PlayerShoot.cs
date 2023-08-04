using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject HitMarker;

    [Header("Other")]
    float fireTimer;
    GameObject mainCam;
    BulletHoleScript holeMaker;
    Controller cntrl;
    bool didhit = false;

    Text ammoCounter;


    private void Awake() {
        mainCam = GameObject.FindWithTag("Camera");
        holeMaker = GameObject.FindWithTag("BulletHoler").GetComponent<BulletHoleScript>();

        ADSelement = GameObject.Find("ADSElement");
        Hipelement = GameObject.Find("HipElement");
        Reticleelement = GameObject.Find("Reticle");
        HitMarker = GameObject.Find("hitmarker");

        HitMarker.GetComponent<Image>().enabled = false;


        ammocount = maxAmmo;
        ammoCounter = GameObject.Find("Ammo").GetComponent<Text>();

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

        ammoCounter.text = "Ammo: " + ammocount.ToString();

        if(Input.GetMouseButton(1)){ 
            ADSactive = true;
        }else{
            ADSactive = false;
        }

        if(reloading){
            ADSelement.GetComponent<Image>().enabled = false;
            ADSelement.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
            Hipelement.GetComponent<Image>().enabled = false;
            Reticleelement.GetComponent<Image>().enabled = false;
        }
        else if(ADSactive){
            ADSelement.GetComponent<Image>().enabled = true;
            ADSelement.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = true;
            Hipelement.GetComponent<Image>().enabled = false;
            Reticleelement.GetComponent<Image>().enabled = false;
        }else{
            ADSelement.GetComponent<Image>().enabled = false;
            ADSelement.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
            Hipelement.GetComponent<Image>().enabled = true;
            Reticleelement.GetComponent<Image>().enabled = true;
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
            StartCoroutine(hitTick());

            didhit = true;
        }
    }


    void hipFire(){
        float xspread = Random.Range(-horizontalSpread, horizontalSpread);
        float yspread = Random.Range(-verticalSpread, verticalSpread);

        Vector3 shotDirection = mainCam.transform.forward + new Vector3(xspread, yspread, 0);
        
        
        ShootServer(damage, mainCam.transform.position, shotDirection);
        if(!didhit){
            holeMaker.makeHole(shotDirection);
        }
        didhit = false;
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
        if(!didhit){
            holeMaker.makeHole(mainCam.transform.forward);
        }
        didhit = false;
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

    IEnumerator hitTick(){
        HitMarker.GetComponent<Image>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        HitMarker.GetComponent<Image>().enabled = false;
    }
}
