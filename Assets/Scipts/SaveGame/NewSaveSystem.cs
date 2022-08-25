using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

//class for controlling load and save behaviour when starting a new savegame
public class NewSaveSystem : MonoBehaviour {
    public Player player;
    public GameController gameController;
    public Transform tutorial;

    public UpgradeMenu upgradesMenu;

    private string path;

    void Start() {
        gameController.StartLoadingScreen();

        //load json with trash data
        NewTrashCollection trash;
        var json = Resources.Load<TextAsset>("Trash/TrashData");
        trash = JsonUtility.FromJson<NewTrashCollection>(json.ToString());

        //instantiate all trash world-objects
        foreach (Trash t in trash.trash) {
            GameObject clone = Instantiate(Resources.Load<GameObject>("Trash/" + t.mesh)) as GameObject;
            clone.name = t.name;
            clone.transform.position = new Vector3(t.posX, t.posY, t.posZ);
            clone.transform.rotation = Quaternion.Euler(t.rotX, t.rotY, t.rotZ);
            clone.AddComponent(typeof(TrashBehaviour));
            clone.GetComponent<TrashBehaviour>().setValues(t.id, t.name, t.info, t.size, t.container, t.mesh);
            gameController.trashNumber++;
        }

        //load all possible upgrades
        upgradesMenu.upgrades = new List<Upgrade>();
        upgradesMenu.upgrades.Add(new Upgrade(0, 1, "Springen Stufe 1", "Schaltet die Sprungfähigkeit frei", 500, 6));
        upgradesMenu.upgrades.Add(new Upgrade(1, 1, "Inventar Stufe 1", "Erhöht die Inventargröße", 500, 15));
        upgradesMenu.upgrades.Add(new Upgrade(2, 1, "Beschleunigung Stufe 1", "Erhöht die Beschleunigung", 500, 80));
        upgradesMenu.upgrades.Add(new Upgrade(3, 1, "Max. Geschwindigkeit Stufe 1", "Erhöht die maximale Geschwindigkeit", 500, 10));
        upgradesMenu.upgrades.Add(new Upgrade(0, 2, "Springen Stufe 2", "Erhöht die Sprungkraft", 1000, 13));
        upgradesMenu.upgrades.Add(new Upgrade(1, 2, "Inventar Stufe 2", "Erhöht die Inventargröße", 1000, 20));
        upgradesMenu.upgrades.Add(new Upgrade(2, 2, "Beschleunigung Stufe 2", "Erhöht die Beschleunigung", 1000, 100));
        upgradesMenu.upgrades.Add(new Upgrade(3, 2, "Max. Geschwindigkeit Stufe 2", "Erhöht die maxmiale Geschwindigkeit", 1000, 13));
        upgradesMenu.upgrades.Add(new Upgrade(0, 3, "Springen Stufe 3", "Erhöht die Sprungkraft", 1500, 22));
        upgradesMenu.upgrades.Add(new Upgrade(1, 3, "Inventar Stufe 3", "Erhöht die Inventargröße", 1500, 25));
        upgradesMenu.upgrades.Add(new Upgrade(2, 3, "Beschleunigung Stufe 3", "Erhöht die Beschleunigung", 1500, 120));
        upgradesMenu.upgrades.Add(new Upgrade(3, 3, "Max. Geschwindigkeit Stufe 3", "Erhöht die maxmiale Geschwindigkeit", 1500, 15));
        upgradesMenu.SetCurrentUpgrades();

        //create new savegame
        path = Path.Combine(Application.persistentDataPath, "muellsammler.savegame");
        SaveGame();
        gameController.LoadOptions();

        gameController.EndLoadingScreen();
        Time.timeScale = 0;
    }

    //create savegame at path and save all values
    public void SaveGame() {
        Savegame save = new Savegame();
        save.SetSaveData(player, gameController);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, save);
        stream.Close();
    }

    //continue to tutorial controls
    public void TutorialContiue() {
        gameController.CloseMenu(tutorial.GetChild(0).gameObject);
        gameController.OpenMenu(tutorial.GetChild(1).gameObject);
    }

    //end tutorial
    public void TutorialEnd() {
        gameController.CloseMenu(tutorial.transform.GetChild(1).gameObject);
        tutorial.gameObject.SetActive(false);
    }
}

//container class for json object
[System.Serializable]
public class NewTrashCollection { public Trash[] trash; }