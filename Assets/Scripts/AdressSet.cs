using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Transporting.Tugboat;

public class AdressSet : MonoBehaviour
{
    public string address;
    [SerializeField] private Text inputField;
    [SerializeField] private Text adressDisplay;
    
    Tugboat boat;
    
    private void Awake() {
        boat = GameObject.Find("NetworkManager").GetComponent<Tugboat>();
    }

    private void Update() {
        boat.SetClientAddress(address);
        adressDisplay.text = "Address is: " + address;
    }

    public void setAdress(){
        address = inputField.text;
    }

}