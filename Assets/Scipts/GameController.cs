using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Audio;

//class for controlling game events, statistics, menus and ui
public class GameController : MonoBehaviour {
    //gameUI
    public GameObject pauseMenu;
    public LoadingScreen loadingScreen;
    public GameObject idleMsg;

    private GameObject activeMenu; //reference to currently open menu

    //playerUI
    public Player player;
    public RectTransform compass;
    public TMPro.TextMeshProUGUI trashCounter;
    public TMPro.TextMeshProUGUI currencyCounter;
    public int trashNumber; //total number of trash world objects + already collected ones

    //statistics
    public GameObject statistics;
    public float timer; //playtime

    //options
    public wrmhlRead gyroscope; //class that connects to gyroscope and ready data
    public GameObject options;
    public Slider audioLevel; //slider for controlling total volume
    public AudioMixer mixer; //mixer for controlling total volume
    public Transform keyOptions;
    public string[] inputs; //input manager
    public Transform gyroOptions;

    void Awake() { inputs = new string[5] {"W", "S", "A", "D", "Space"}; }

    void Update() {
        //open pausemenu/close currently open menu 
        if (Input.GetKeyDown(KeyCode.Escape) && activeMenu != null) CloseMenu(activeMenu);
        else if (Input.GetKeyDown(KeyCode.Escape)) OpenMenu(pauseMenu);

        //increment play time
        timer += Time.unscaledDeltaTime;

        //playerUI
        compass.rotation = Quaternion.Euler(0, 0, (-Mathf.Atan2(player.transform.position.x, player.transform.position.z) * Mathf.Rad2Deg - 180));
        currencyCounter.text = "" + player.currency;
        trashCounter.text = player.inv + " / " + player.invMax;

        //statistics
        TimeSpan t = TimeSpan.FromSeconds(timer);
        string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
        statistics.transform.GetChild(1).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = elapsedTime;
        statistics.transform.GetChild(2).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = player.collected.Count + " / " + trashNumber;

        //options
        if(options.activeSelf) {
            String status;
            switch (gyroscope.deviceStatus) {
                case -1: status = "Ein Fehler ist aufgetreten"; break;
                case 0: status = "Gyroskop nicht verbunden"; break;
                case 1: status = "Gyroskop verbunden"; break;
                default: status = ""; break;
            }
            gyroOptions.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Status: " + status;
            if (gyroscope.deviceStatus == -1) gyroOptions.GetChild(5).gameObject.SetActive(true); else gyroOptions.GetChild(5).gameObject.SetActive(false);
            gyroOptions.GetChild(7).GetComponent<TMPro.TextMeshProUGUI>().text = "X: " + gyroscope.gyroX;
            gyroOptions.GetChild(8).GetComponent<TMPro.TextMeshProUGUI>().text = "Y: " + gyroscope.gyroY;
            gyroOptions.GetChild(9).GetComponent<TMPro.TextMeshProUGUI>().text = "Z: " + gyroscope.gyroZ;
        }

        //if idle for more than 10 seconds show idle message
        if (player.idle >= 10) idleMsg.SetActive(true);
        else idleMsg.SetActive(false);
    }

    //close all currently open menus, if any, and open specified one
    public void OpenMenu(GameObject menu) {
        if (activeMenu != null) CloseMenu(activeMenu);

        menu.SetActive(true);
        activeMenu = menu;
        Time.timeScale = 0;
        player.idle = 0;
    }

    //close specified menu
    public void CloseMenu(GameObject menu) {
        menu.SetActive(false);
        activeMenu = null;
        Time.timeScale = 1;
    }

    //load or end loading screen animation
    public void StartLoadingScreen() { loadingScreen.StartLoadingScreen(); }
    public void EndLoadingScreen() { loadingScreen.EndLoadingScreen(); }

    //-----------------------------OptionsMenu---------------------------------

    //change keybord key used for different actions
    public void ChangeKey() {
        try {
            //get chosen input option
            GameObject button;
            switch (EventSystem.current.currentSelectedGameObject.name) {
                case "Up": button = keyOptions.GetChild(1).GetChild(1).gameObject; break;
                case "Down": button = keyOptions.GetChild(2).GetChild(1).gameObject; break;
                case "Left": button = keyOptions.GetChild(3).GetChild(1).gameObject; break;
                case "Right": button = keyOptions.GetChild(4).GetChild(1).gameObject; break;
                case "Jump": button = keyOptions.GetChild(5).GetChild(1).gameObject; break;
                default: return;
            }

            //start pressed input routine
            button.GetComponent<Button>().interactable = false;
            IEnumerator coroutine = GetInput(button);
            StartCoroutine(coroutine);
        } catch(NullReferenceException) { return; }
    }

    //coroutine to get the next input and check if its a letter or space
    private IEnumerator GetInput(GameObject button) {
        while (!Input.anyKeyDown) yield return null;

        KeyCode key = KeyCode.None;
        foreach (KeyCode _key in System.Enum.GetValues(typeof(KeyCode)))
            if (Input.GetKey(_key)) key = _key;

        String keyString = key.ToString();
        if (keyString[0] >= 65 && keyString[0] <= 90 && keyString.Length == 1 && !CheckDouble(key) || keyString == "Space" && !CheckDouble(key))
            button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + key;
        button.GetComponent<Button>().interactable = true;

        inputs[0] = keyOptions.GetChild(1).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        inputs[1] = keyOptions.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        inputs[2] = keyOptions.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        inputs[3] = keyOptions.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        inputs[4] = keyOptions.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
    }

    //check if key is already in use
    private bool CheckDouble(KeyCode key) {
        bool doubble = false;
        if (keyOptions.GetChild(1).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) doubble = true;
        else if (keyOptions.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) doubble = true;
        else if (keyOptions.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) doubble = true;
        else if (keyOptions.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) doubble = true;
        else if (keyOptions.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) doubble = true;
        return doubble;
    }

    //reconnect gyroscope
    public void Reconnect() { gyroscope.Start(); }

    //set current gyro Values as norm
    public void SetPivot() {
        gyroscope.gyroXNorm = gyroscope.gyroX;
        gyroscope.gyroYNorm = gyroscope.gyroY;
        gyroscope.gyroZNorm = gyroscope.gyroZ;
    }

    //set norm gyro values to 0
    public void ResetPivot() { gyroscope.gyroXNorm = gyroscope.gyroYNorm = gyroscope.gyroZNorm = 0; }

    //save option values
    public void SaveOptions() {
        Options save = new Options();

        save.audioLevel = audioLevel.value;
        save.up = keyOptions.GetChild(1).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        save.down = keyOptions.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        save.left = keyOptions.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        save.right = keyOptions.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        save.jump = keyOptions.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
        save.gyroXNorm = gyroscope.gyroXNorm;
        save.gyroYNorm = gyroscope.gyroYNorm;
        save.gyroZNorm = gyroscope.gyroZNorm;
        save.gyroPort = gyroscope.portName;

        String path = Path.Combine(Application.persistentDataPath, "muellsammler.options");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, save);
        stream.Close();
    }

    //load option values
    public void LoadOptions() {
        string path = Path.Combine(Application.persistentDataPath, "muellsammler.options");

        if (!File.Exists(path)) SaveOptions();
        else {
            FileStream stream = new FileStream(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            Options save = formatter.Deserialize(stream) as Options;
            stream.Close();

            //set option values
            audioLevel.value = save.audioLevel;
            keyOptions.GetChild(1).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + save.up;
            keyOptions.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + save.down;
            keyOptions.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + save.left;
            keyOptions.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + save.right;
            keyOptions.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + save.jump;
            gyroscope.gyroXNorm = save.gyroXNorm;
            gyroscope.gyroYNorm = save.gyroYNorm;
            gyroscope.gyroZNorm = save.gyroZNorm;
            gyroscope.portName = save.gyroPort;

            inputs = new string[5] { save.up, save.down, save.left, save.right, save.jump };
        }
    }

    public void SetVolume(float sliderValue) { mixer.SetFloat("TotalVolume", Mathf.Log10(sliderValue) * 20); }

    public void SetPort(string portValue) { gyroscope.portName = portValue; }
}
