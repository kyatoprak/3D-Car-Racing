using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class awakeManager : MonoBehaviour
{
    [SerializeField] public GameObject LoadingScreen;
    [SerializeField] Slider slider;
    [SerializeField] public GameObject GameModeSelectScreen;

    public GameObject toRotate;
    public float rotateSpeed = 10;

    public vehicleList listOfVehicles;
    public int vehiclePointer = 0;

    private string selectedGameMode = "";

    private void Awake()
    {
        //Selected car
        PlayerPrefs.SetInt("pointer", vehiclePointer);
        vehiclePointer = PlayerPrefs.GetInt("pointer");

        GameObject childObject = Instantiate(listOfVehicles.Vehicles[vehiclePointer], Vector3.zero, Quaternion.identity);
        childObject.transform.parent = toRotate.transform;
    }

    private void FixedUpdate()
    {
        toRotate.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        LoadingScreen.SetActive(true);
        slider.value = 0f;

        float fakeProgress = 0f;
        float duration = 10f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fakeProgress = timer / duration;
            slider.value = Mathf.Clamp01(fakeProgress);
            yield return null;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
    }

    public void ShowGameModeScreen()
    {
        GameModeSelectScreen.SetActive(true);
    }

    public void SelectFreeRide()
    {
        selectedGameMode = "freeRideScene";
        StartGame();
    }

    public void SelectRaceMode()
    {
        selectedGameMode = "raceScene";
        StartGame();
    }

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(selectedGameMode))
        {
            StartCoroutine(LoadSceneAsync(selectedGameMode));
        }
        else
        {
            Debug.LogWarning("Game Mode is not selected");
        }
    }

    public void leftButton()
    {
        if (vehiclePointer > 0)
        {
            ClearSpawnedVehicle();
            vehiclePointer--;
            PlayerPrefs.SetInt("pointer", vehiclePointer);

            GameObject childObject = Instantiate(listOfVehicles.Vehicles[vehiclePointer], Vector3.zero, Quaternion.identity);
            childObject.transform.parent = toRotate.transform;
        }
    }

    public void rightButton()
    {
        if (vehiclePointer < listOfVehicles.Vehicles.Length - 1)
        {
            ClearSpawnedVehicle();
            vehiclePointer++;
            PlayerPrefs.SetInt("pointer", vehiclePointer);

            GameObject childObject = Instantiate(listOfVehicles.Vehicles[vehiclePointer], Vector3.zero, Quaternion.identity);
            childObject.transform.parent = toRotate.transform;
        }
    }

    private void ClearSpawnedVehicle()
    {
        foreach (Transform child in toRotate.transform)
        {
            Destroy(child.gameObject);
        }
    }

  
}
