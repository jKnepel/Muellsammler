using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Savegame {
    //player data
    public float[] pos;
    public float[] rot;
    [SerializeField] public List<int> collected;
    [SerializeField] public List<Trash> glued;
    [SerializeField] public List<Upgrade> upgrades;
    public int inv;
    public int currency;

    //statistics data
    public float timer;
    public int currencyTotal;
    public int correct;
    public int wrong;
    public int resets;

    public void SetSaveData(Player player, GameController gameController) {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        pos = new float[3] { rb.position.x, rb.position.y, rb.position.z };
        rot = new float[3] { rb.transform.eulerAngles.x, rb.transform.eulerAngles.y, rb.transform.eulerAngles.z };
        collected = player.collected;
        glued = player.GetTrash();

        upgrades = player.upgrades;
        inv = player.inv;
        currency = player.currency;

        timer = gameController.timer;
        currencyTotal = int.Parse(gameController.statistics.transform.GetChild(4).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
        correct = int.Parse(gameController.statistics.transform.GetChild(5).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
        wrong = int.Parse(gameController.statistics.transform.GetChild(6).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
        resets = int.Parse(gameController.statistics.transform.GetChild(7).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
    }
}
