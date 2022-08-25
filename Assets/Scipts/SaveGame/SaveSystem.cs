using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

//class for controlling load and save behaviour when continuing to play on existing savegame   
public class SaveSystem : MonoBehaviour {
    public Player player;
    public GameController gameController;

    public UpgradeMenu upgradesMenu;

    private string path;

    void Start() {
        path = Path.Combine(Application.persistentDataPath, "muellsammler.savegame");

        gameController.StartLoadingScreen();

        LoadGame();

        gameController.EndLoadingScreen();
    }

    //load deserialized savegame
    private void LoadGame() {
        FileStream stream = null;
        Savegame save = null;
        try {
            stream = new FileStream(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            save = formatter.Deserialize(stream) as Savegame;
        } catch (FileNotFoundException) {
            Debug.LogError("Save file not found in " + path);
        } finally {
            if (stream != null) stream.Close();
        }

        LoadPlayer(save);
        LoadTrash(save);
        LoadUpgrades(save);
        gameController.LoadOptions();
    }

    //load saved player attributes and statistics
    private void LoadPlayer(Savegame data) {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.position = new Vector3(data.pos[0], data.pos[1], data.pos[2]);
        rb.rotation = Quaternion.Euler(data.rot[0], data.rot[1], data.rot[2]);
        player.inv = data.inv;
        player.currency = data.currency;

        //statistics
        gameController.timer = data.timer;
        gameController.statistics.transform.GetChild(3).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + data.upgrades.Count;
        gameController.statistics.transform.GetChild(4).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + data.currencyTotal;
        gameController.statistics.transform.GetChild(5).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + data.correct;
        gameController.statistics.transform.GetChild(6).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + data.wrong;
        gameController.statistics.transform.GetChild(7).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + data.resets;
    }

    //load glued trash onto player object and load all, not-collected trash objects from json
    private void LoadTrash(Savegame data) {
        //position all glued trash on player
        foreach (Trash trash in data.glued) {
            GameObject clone = Instantiate(Resources.Load<GameObject>("Trash/" + trash.mesh), Vector3.zero, Quaternion.identity) as GameObject;
            clone.transform.parent = player.transform;
            clone.transform.localPosition = new Vector3(trash.posX, trash.posY, trash.posZ);
            clone.transform.localRotation = Quaternion.Euler(trash.rotX, trash.rotY, trash.rotZ);
            clone.AddComponent(typeof(TrashBehaviour));
            clone.GetComponent<TrashBehaviour>().setValues(trash.id, trash.name, trash.info, trash.size, trash.container, trash.mesh);
            player.glued.Add(clone);
        }

        //load json with trash data
        TrashCollection trashCollection;

        var json = Resources.Load<TextAsset>("Trash/TrashData");
        trashCollection = JsonUtility.FromJson<TrashCollection>(json.ToString());

        //instantiate all trash world-objects except already collected ones 
        player.collected = data.collected;
        foreach (Trash trash in trashCollection.trash) {
            if (!player.collected.Contains(trash.id)) {
                GameObject clone = Instantiate(Resources.Load<GameObject>("Trash/" + trash.mesh)) as GameObject;
                clone.transform.position = new Vector3(trash.posX, trash.posY, trash.posZ);
                clone.transform.rotation = Quaternion.Euler(trash.rotX, trash.rotY, trash.rotZ);
                clone.AddComponent(typeof(TrashBehaviour));
                clone.GetComponent<TrashBehaviour>().setValues(trash.id, trash.name, trash.info, trash.size, trash.container, trash.mesh);
            }
            gameController.trashNumber++;
        }
    }

    //load all possible upgrades, remove already bought ones
    private void LoadUpgrades(Savegame data) {
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

        player.upgrades = data.upgrades;
        Upgrade upgrade;
        for (int i = upgradesMenu.upgrades.Count - 1; i >= 0; i--) {
            upgrade = upgradesMenu.upgrades[i];
            if (ContainsUpgrade(player.upgrades, upgrade)) {
                switch (upgrade.type) {
                    case 0: if (player.jump < upgrade.value) player.jump = upgrade.value; break;
                    case 1: if (player.invMax < upgrade.value) player.invMax = upgrade.value; break;
                    case 2: if (player.acceleration < upgrade.value) player.acceleration = upgrade.value; break;
                    case 3: if (player.speedMax < upgrade.value) player.speedMax = upgrade.value; break;
                }
                upgradesMenu.upgrades.RemoveAt(i);
            }
        }

        upgradesMenu.SetCurrentUpgrades();
    }

    //check if upgrade or higher level of it was bought
    private bool ContainsUpgrade(List<Upgrade> list, Upgrade upgrade) {
        foreach (Upgrade _upgrade in list) {
            if (upgrade.type == _upgrade.type && upgrade.level <= _upgrade.level) return true;
        }
        return false;
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
}

//container class for json object
[System.Serializable]
public class TrashCollection { public Trash[] trash; }