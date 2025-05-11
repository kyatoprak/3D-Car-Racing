using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class awakeManager : MonoBehaviour
{

    [SerializeField] public GameObject LoadingScreen;
    [SerializeField] Slider  slider;
    
    
    
    public GameObject toRotate;
    
    public float rotateSpeed =10;
    
    public vehicleList listOfVehicles;
    
    public int vehiclePointer = 0;
    
    private void Awake()
    {
        PlayerPrefs.SetInt("pointer",vehiclePointer);
        vehiclePointer = PlayerPrefs.GetInt("pointer");
        GameObject childObject = Instantiate(listOfVehicles.Vehicles[vehiclePointer],Vector3.zero,Quaternion.identity)as GameObject;
        childObject.transform.parent = toRotate.transform;
    }

    private void FixedUpdate()
    {
        toRotate.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

     
    }
    
    
    IEnumerator LoadSceneAsync()
    {
        LoadingScreen.SetActive(true);

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync("Versatile Studio Assets/Demo City By Versatile Studio/Scenes/demo_city_night");

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;

            yield return null;
        }
    }

    
    

    public void leftButton()
    {

        if (vehiclePointer >0)
        {

            Destroy(GameObject.FindGameObjectWithTag("Player"));
            vehiclePointer--;
            PlayerPrefs.SetInt("pointer",vehiclePointer);
            GameObject childObject = Instantiate(listOfVehicles.Vehicles[vehiclePointer],Vector3.zero,Quaternion.identity)as GameObject;
            childObject.transform.parent = toRotate.transform;
        }
    }

   
    public void StartGame()
    {
        StartCoroutine(LoadSceneAsync());
    }

    
    public void rightButton()
    {

        if (vehiclePointer < listOfVehicles.Vehicles.Length - 1)
        {

            Destroy(GameObject.FindGameObjectWithTag("Player"));
            vehiclePointer++;
            PlayerPrefs.SetInt("pointer",vehiclePointer);
            GameObject childObject = Instantiate(listOfVehicles.Vehicles[vehiclePointer],Vector3.zero,Quaternion.identity)as GameObject;
            childObject.transform.parent = toRotate.transform;
        }
    }
}
