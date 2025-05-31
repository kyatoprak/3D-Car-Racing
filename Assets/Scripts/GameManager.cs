using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject racePanel;
    public TMP_Dropdown DropdownGearBox;
    public TMP_Dropdown DropdownDriveType;
    public TMP_Dropdown DropdownDriveMode;
    public GameObject EscapeMenu;
    
    
    public GameObject needle;
    public Text kph, gearNum;
    public vehicleList list;

    public GameObject resultsPanel;
    public Transform resultListContainer;
    public GameObject resultRowPrefab;
    
    public carControlScript _carControlScript;
    public GameObject[] presentVehicle;

    private float startPosition = 22f, endPosition = -202f, desiredPosition;
    private float currentNeedleRotation = 0f;
    private float needleVelocity = 0f;

    public bool raceStarts = false;
    private Vector3 vehicleStartPos;
    public TMP_Text countdownText;

    public List<GameObject> Players;
    
    
    private void Awake()
    {
        
        //start pos for the vehicle
        if (SceneManager.GetActiveScene().name == "freeRideScene")
        {
            vehicleStartPos = new Vector3(-35.6f, 0.76f, -0.05f);
        }
        else if (SceneManager.GetActiveScene().name == "raceScene")
        {
            vehicleStartPos = new Vector3(249.3f, 5.7f, 17);
            presentVehicle = GameObject.FindGameObjectsWithTag("AI");
        }

        int pointer = PlayerPrefs.GetInt("pointer", 0);
        if (pointer < 0 || pointer >= list.Vehicles.Length)
        {
            pointer = 0;
        }

        GameObject vehicle = Instantiate(list.Vehicles[pointer], vehicleStartPos, Quaternion.identity);

        if (SceneManager.GetActiveScene().name == "raceScene")
            vehicle.transform.rotation = Quaternion.Euler(0, -90, 0);

        _carControlScript = vehicle.GetComponent<carControlScript>();
        if (_carControlScript == null)
        {
            Debug.LogError("Araç prefab'inde carControlScript component'i eksik");
        }
    }

    private void Start()
    {
        
        
        if(SceneManager.GetActiveScene().name=="raceScene")
            StartCoroutine(RaceStartCountdown());
        
        if (_carControlScript == null) return;

        // Dropdown connections
        DropdownDriveType.onValueChanged.AddListener(OnDriveTypeChanged);
        DropdownGearBox.onValueChanged.AddListener(OnGearboxChanged);
        DropdownDriveMode.onValueChanged.AddListener(OnDriveModeChanged);
        
        DropdownDriveType.value = (int)_carControlScript.drive;
        DropdownGearBox.value = (int)_carControlScript.gearChangeType;
        OnDriveModeChanged(DropdownDriveMode.value);

        //Adding all vehicles to Players List
        foreach (var VARIABLE in GameObject.FindGameObjectsWithTag("AI"))
        {
            Players.Add(VARIABLE);
        }
        Players.Add(GameObject.FindGameObjectWithTag("Player"));
        
        
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (EscapeMenu.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_carControlScript == null) return;

        kph.text = _carControlScript.KPH.ToString("0");
        UpdateNeedle();
    }

    public void UpdateNeedle()
    {
        desiredPosition = startPosition - endPosition;
        float targetValue = _carControlScript.engineRPM / 10000f;
        float targetRotation = startPosition - targetValue * desiredPosition;

        currentNeedleRotation = Mathf.SmoothDamp(currentNeedleRotation, targetRotation, ref needleVelocity, 0.1f);
        needle.transform.eulerAngles = new Vector3(0, 0, currentNeedleRotation);
    }

    public void changeGear()
    {
        gearNum.text = (!_carControlScript.reverse) ? (_carControlScript.gearNum + 1).ToString() : "R";
    }

    public void OnDriveTypeChanged(int index)
    {
        _carControlScript.drive = (carControlScript.driveType)index;
    }
    

   public void ShowResults()
   {
       racePanel.SetActive(true);
    
    resultsPanel.SetActive(true);
    
    VerticalLayoutGroup layout = resultListContainer.GetComponent<VerticalLayoutGroup>();
    if (layout == null)
        layout = resultListContainer.gameObject.AddComponent<VerticalLayoutGroup>();

    layout.childAlignment = TextAnchor.MiddleCenter;
    layout.spacing = 15;
    layout.childControlHeight = true;
    layout.childControlWidth = true;
    layout.childForceExpandHeight = false;
    layout.childForceExpandWidth = false;

    RectTransform rt = resultListContainer.GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(0.5f, 0.5f);
    rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.pivot = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = Vector2.zero;
    rt.sizeDelta = new Vector2(600, 500); 

    foreach (Transform child in resultListContainer)
    {
        Destroy(child.gameObject);
    }

    var results = new List<(string name, float time)>();

    foreach (var player in Players)
    {
        var raceInfo = player.GetComponent<RaceInfo>();
        if (raceInfo != null)
        {
            results.Add((raceInfo.playerName, raceInfo.endTime));
        }
    }

    results = results.OrderBy(r => r.time < 0 ? float.MaxValue : r.time).ToList();

    foreach (var (name, time) in results)
    {
        GameObject row = Instantiate(resultRowPrefab, resultListContainer);
        
        RectTransform rowRT = row.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(400, 120);
        rowRT.localScale = Vector3.one;

        TMP_Text txt = row.GetComponentInChildren<TMP_Text>();
        txt.text = $"{name} - {(time >= 0 ? time.ToString("F2") + "s" : "--")}";
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableAutoSizing = false;
        txt.fontSize = 8;

        RectTransform textRT = txt.GetComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(400, 120);
        textRT.localScale = Vector3.one;
    }
}



    
    public void OnGearboxChanged(int index)
    {
        _carControlScript.gearChangeType = (carControlScript.gearBox)index;
    }

    IEnumerator RaceStartCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        _carControlScript.startTime = Time.time;
        raceStarts = true;
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1);
        countdownText.text = "";
    }

    public void OnDriveModeChanged(int index)
    {
        if (_carControlScript == null) return;
        
        for (int i = 0; i < _carControlScript.wheels.Length; i++)
        {
            WheelFrictionCurve forwardFriction = _carControlScript.wheels[i].forwardFriction;
            WheelFrictionCurve sidewaysFriction = _carControlScript.wheels[i].sidewaysFriction;

            if (index == 0) // Casual
            {
                forwardFriction.extremumSlip = 0.1f;
                forwardFriction.stiffness = 2.5f;

                sidewaysFriction.extremumSlip = 0.1f;
                sidewaysFriction.stiffness = 2.5f;
            }
            else if (index == 1) // Drift
            {
                forwardFriction.extremumSlip = 0.65f;
                forwardFriction.stiffness = 1.5f;

                sidewaysFriction.extremumSlip = 1f;
                sidewaysFriction.stiffness = 1.5f;
            }

            _carControlScript.wheels[i].forwardFriction = forwardFriction;
            _carControlScript.wheels[i].sidewaysFriction = sidewaysFriction;
        }
    }

    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("AwakeScene");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; 
        #else
            Application.Quit(); 
        #endif
    }


    public void PauseGame()
    {
        EscapeMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        EscapeMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void RestartRace()
    {
        Time.timeScale = 1f; 
        racePanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }


}
