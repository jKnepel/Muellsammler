using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeMenu : MonoBehaviour {
    public GameObject menu;
    public GameObject confirmation;
    public GameController gameController;
    public Player player;
    public wrmhlRead gyroscope;

    public Transform upgradeRow;

    //currently highlighted upgrade
    private Upgrade upgrade;

    public List<Upgrade> upgrades; //all upgrades that havent been purchased yet
    private List<Upgrade> currentUpgrades = new List<Upgrade>(); //all currently available upgrades on their respective level

    //current position within menu
    private int upgradePos; //currently highlighted upgrade
    private int verticalPos; //-1 == close menu, 0 == highlight close indicator, 1 == upgradeRow, 2 == highlight purchase indicator, 3 == continue to purchase menu
    private int confirmPos; //-1 == return to upgrade menu, 0 == highlight abort indicator, 1 == normal position, 2 == highlight confirm indicator, 3 == confirm purchase

    private Vector2 gyroValues; //Vector that determines if gyro is used for menu navigation
    private bool hori = false; //check if cooldown is active
    private bool vert = false; //check if cooldown is active

    //open upgrade menu on collider trigger
    void OnTriggerEnter(Collider other) {
        if (upgrades != null) {
            if (other.name == "Player" && upgrades.Count > 0) {
                gameController.OpenMenu(menu);
                upgradePos = 0;
                verticalPos = 1;
            }
        }
    }

    //set currently available upgrade level for each of the 4 types
    public void SetCurrentUpgrades() {
        currentUpgrades = new List<Upgrade>();
        for (int i = 0; i < 4; i++) 
            foreach (Upgrade _upgrade in upgrades) 
                if (_upgrade.type == i) { currentUpgrades.Add(_upgrade); break; }

        currentUpgrades.Sort((p1, p2) => p1.type.CompareTo(p2.type));
    }

    void Update() {
        if (menu.activeSelf) {
            //check for inputs from gyroscope and start cooldown if threshold is reached
            if (Mathf.Abs(gyroscope.gyroX) >= 30 && !hori) {
                gyroValues = new Vector2(1 * Mathf.Sign(gyroscope.gyroX), gyroValues.y);
                IEnumerator horiCooldown = HorizontalCooldown();
                StartCoroutine(horiCooldown);
            }
            if (Mathf.Abs(gyroscope.gyroY) >= 55 && !vert) {
                gyroValues = new Vector2(gyroValues.x, 1 * Mathf.Sign(gyroscope.gyroY));
                IEnumerator vertCooldown = VerticalCooldown();
                StartCoroutine(vertCooldown);
            }

            //move position within menu
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[0])) || gyroValues.y == -1) verticalPos = (int)Mathf.Clamp(verticalPos + 1, -1, 3);
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[1])) || gyroValues.y == 1) verticalPos = (int)Mathf.Clamp(verticalPos - 1, -1, 3);
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[2])) && verticalPos == 1 || gyroValues.x == 1 && verticalPos == 1) upgradePos = (int)Mathf.Clamp(upgradePos - 1, 0, currentUpgrades.Count - 1);
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[3])) && verticalPos == 1 || gyroValues.x == -1 && verticalPos == 1) upgradePos = (int)Mathf.Clamp(upgradePos + 1, 0, currentUpgrades.Count - 1);
            gyroValues = new Vector2(0, 0);


            //close upgrade menu
            if (verticalPos == -1) gameController.CloseMenu(menu);

            //highlight close menu indicator
            if (verticalPos == 0) {
                menu.transform.GetChild(1).GetChild(3).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched_highlighted_green", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(1).GetChild(4).GetComponent<TMP_Text>().font = Resources.Load("Fonts/GreenFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            } else {
                menu.transform.GetChild(1).GetChild(3).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(1).GetChild(4).GetComponent<TMP_Text>().font = Resources.Load("Fonts/WhiteUIFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            }

            //highlight purchase indicator depending on wether or the player has enough currency
            if (verticalPos == 2 || verticalPos == 3) {
                upgrade = currentUpgrades[upgradePos];
                if (player.currency >= upgrade.price) {
                    menu.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched_highlighted_green", typeof(Sprite)) as Sprite;
                    menu.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().font = Resources.Load("Fonts/GreenFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
                } else {
                    menu.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched_highlighted_red", typeof(Sprite)) as Sprite;
                    menu.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().font = Resources.Load("Fonts/RedFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
                }
            } else {
                menu.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().font = Resources.Load("Fonts/WhiteUIFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            }

            //purchase highlighted upgrade if player has enough currency
            if (verticalPos == 3) {
                upgrade = currentUpgrades[upgradePos];

                if (player.currency >= upgrade.price) {
                    confirmation.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Willst du wirklich das Upgrade " + upgrade.name + " kaufen? Das Upgrade kostet " + upgrade.price + ".";
                    gameController.CloseMenu(menu);
                    gameController.OpenMenu(confirmation);
                    confirmPos = 1;
                } else verticalPos = 2;
            }

            //set left upgrade
            if (upgradePos > 0) SetUpgrade(upgradePos - 1, upgradeRow.GetChild(1));
            else upgradeRow.GetChild(1).gameObject.SetActive(false);

            //set center upgrade
            SetUpgrade(upgradePos, upgradeRow.GetChild(0));
            if(verticalPos == 1) {
                if (player.currency >= currentUpgrades[upgradePos].price) upgradeRow.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color(0, 255, 0);
                else upgradeRow.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color(255, 0, 0);
            } else upgradeRow.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255);

            //set right upgrade
            if (upgradePos < currentUpgrades.Count - 1) SetUpgrade(upgradePos + 1, upgradeRow.GetChild(2));
            else upgradeRow.GetChild(2).gameObject.SetActive(false);


            //close menu when every upgrade has been purchased
            if (upgrades.Count == 0) {
                gameController.CloseMenu(menu);
                transform.GetChild(3).gameObject.SetActive(false);
            }
        } else if(confirmation.activeSelf) {
            //check for inputs from gyroscope and start cooldown if over threshold, or other specified vertical inputs
            if (Mathf.Abs(gyroscope.gyroY) >= 55 && !vert) {
                confirmPos -= (int)(Mathf.Sign(gyroscope.gyroY));
                IEnumerator vertCooldown = VerticalCooldown();
                StartCoroutine(vertCooldown);
            } else if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[0]))) confirmPos += 1;
            else if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[1]))) confirmPos -= 1;

            //check for position within confirmation menu
            switch (confirmPos) {
                case -1: Abort(); break;
                case 0: confirmation.transform.GetChild(0).GetChild(2).GetComponent<Button>().Select(); break;
                case 1: EventSystem.current.SetSelectedGameObject(null); break;
                case 2: confirmation.transform.GetChild(0).GetChild(1).GetComponent<Button>().Select(); break;
                case 3: Confirm(); break;
                default: confirmPos = 1; break;
            }
        }
    }

    //set information of visible upgrades within menu
    private void SetUpgrade(int pos, Transform destination) {
        destination.gameObject.SetActive(true);

        upgrade = currentUpgrades[pos];

        //set image
        switch (upgrade.type) {
            case 0: destination.GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/upgrade_jump", typeof(Sprite)) as Sprite; break;
            case 1: destination.GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/upgrade_inventory", typeof(Sprite)) as Sprite; break;
            case 2: destination.GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/upgrade_acceleration", typeof(Sprite)) as Sprite; break;
            case 3: destination.GetChild(1).GetComponent<Image>().sprite = Resources.Load("UI/upgrade_velocity", typeof(Sprite)) as Sprite; break;
            default: break;
        }

        //set info
        destination.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = upgrade.name;
        destination.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = upgrade.info;
        destination.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().text = "" + upgrade.price;
        if (player.currency >= upgrade.price) {
            destination.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().font = Resources.Load("Fonts/GreenFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            destination.GetChild(5).GetComponent<Image>().sprite = Resources.Load("UI/money_green", typeof(Sprite)) as Sprite;
        } else {
            destination.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().font = Resources.Load("Fonts/RedFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            destination.GetChild(5).GetComponent<Image>().sprite = Resources.Load("UI/money_red", typeof(Sprite)) as Sprite;
        }
    }

    //purchase upgrade and add it and its values to player
    public void Confirm() {
        upgrade = currentUpgrades[upgradePos];
        player.upgrades.Add(upgrade);
        player.currency -= upgrade.price;


        //set player attribute to upgrades value
        switch (upgrade.type) {
            case 0: player.jump = upgrade.value; break;
            case 1: player.invMax = upgrade.value; break;
            case 2: player.acceleration = upgrade.value; break;
            case 3: player.speedMax = upgrade.value; break;
        }

        gameController.statistics.transform.GetChild(3).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + player.upgrades.Count; //set statistic of bought upgrades once on purchase, to prevent nesseccary update on every frame
        //reset upgrade menu
        upgrades.Remove(upgrade);
        SetCurrentUpgrades();
        verticalPos = 1;
        upgradePos = (int) Mathf.Clamp(upgradePos, 0, currentUpgrades.Count - 1);
        gameController.CloseMenu(confirmation);
        gameController.OpenMenu(menu);
    }

    //abort purchase and go back to upgrade menu
    public void Abort() {
        verticalPos = 1;
        gameController.CloseMenu(confirmation);
        gameController.OpenMenu(menu);
    }

    //cooldown coroutine for menu navigation via gyroscope to prevent skipping through elements
    private IEnumerator HorizontalCooldown() {
        hori = true;
        for (float i = 0; i <= 1f; i += Time.unscaledDeltaTime) yield return null;
        hori = false;
    }

    private IEnumerator VerticalCooldown() {
        vert = true;
        for (float i = 0; i <= 1f; i += Time.unscaledDeltaTime) yield return null;
        vert = false;
    }
}
