using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashMenu : MonoBehaviour {
    public GameObject menu;
    public GameController gameController;
    public Player player;
    public wrmhlRead gyroscope;

    public Transform trashRow;
    public Transform containerRow;
    public Transform infoContainer;

    //position within menu
    private int trashPos; //currently highlighted trash
    private int containerPos; //currently highlighted container
    private int verticalPos; //-1 == close menu, 0 == highlight close menu indicator, 1 == trashRow, 2 == containerRow, 3 == highlight sort indicator, 4 == sort trash

    private Vector2 gyroValues; //Vector that determines if gyro is used for menu navigation
    private bool hori = false; //check if cooldown is active
    private bool vert = false; //check if cooldown is active

    //open trash menu on collider trigger
    void OnTriggerEnter(Collider other) {
        if (other.name == "Player" && player.glued.Count > 0) {
            gameController.OpenMenu(menu);
            trashPos = 0;
            containerPos = 0;
            verticalPos = 1;
        }
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

            //vertical navigation within menu
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[0])) || gyroValues.y == -1) verticalPos = (int)Mathf.Clamp(verticalPos + 1, -1, 4);
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[1])) || gyroValues.y == 1) verticalPos = (int)Mathf.Clamp(verticalPos - 1, -1, 4);
            gyroValues = new Vector2(gyroValues.x, 0);

            //close the menu
            if (verticalPos == -1) {
                player.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
                player.GetComponent<Rigidbody>().inertiaTensor = new Vector3(16, 16, 16);
                player.GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;
                gameController.CloseMenu(menu);
                return;
            }

            //highlight leave indicator
            if (verticalPos == 0) {
                menu.transform.GetChild(0).GetChild(1).GetChild(5).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched_highlighted_green", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(0).GetChild(1).GetChild(6).GetComponent<TMP_Text>().font = Resources.Load("Fonts/GreenFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            } else {
                menu.transform.GetChild(0).GetChild(1).GetChild(5).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(0).GetChild(1).GetChild(6).GetComponent<TMP_Text>().font = Resources.Load("Fonts/WhiteUIFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            }

            //horizontal navigation for trash and container row
            if (verticalPos == 1) {
                if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[3])) || gyroValues.x == -1) trashPos = (int)Mathf.Clamp(trashPos + 1, 0, player.glued.Count - 1);
                if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[2])) || gyroValues.x == 1) trashPos = (int)Mathf.Clamp(trashPos - 1, 0, player.glued.Count - 1);
            } else if (verticalPos == 2) {
                if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[3])) || gyroValues.x == -1) containerPos = (int)Mathf.Clamp(containerPos + 1, 0, 6);
                if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[2])) || gyroValues.x == 1) containerPos = (int)Mathf.Clamp(containerPos - 1, 0, 6);
            }
            gyroValues = new Vector2(0, gyroValues.y);

            //highlight confirm indicator
            if (verticalPos == 3) {
                menu.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched_highlighted_green", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<TMP_Text>().font = Resources.Load("Fonts/GreenFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            } else {
                menu.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<Image>().sprite = Resources.Load("UI/arrow_stretched", typeof(Sprite)) as Sprite;
                menu.transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<TMP_Text>().font = Resources.Load("Fonts/WhiteUIFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
            }

            //sort trash
            if (verticalPos == 4) {
                //check if sort was successful or not and update statistics
                TrashBehaviour sorted = GetTrash(trashPos);
                if (sorted.container == containerPos) {
                    gameController.transform.GetChild(0).GetComponent<AudioSource>().Play();
                    player.currency += sorted.size * 100;
                    int currency = int.Parse(gameController.statistics.transform.GetChild(4).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
                    gameController.statistics.transform.GetChild(4).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + (currency + sorted.size * 100);
                    int sortedCorrect = int.Parse(gameController.statistics.transform.GetChild(5).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
                    gameController.statistics.transform.GetChild(5).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + (sortedCorrect + 1);
                } else {
                    gameController.transform.GetChild(1).GetComponent<AudioSource>().Play();
                    int sortedWrong = int.Parse(gameController.statistics.transform.GetChild(6).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
                    gameController.statistics.transform.GetChild(6).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + (sortedWrong + 1);
                }

                //remove sorted trash from player and return to menu / close menu if no glued trash remains
                GameObject d_trash;
                foreach (GameObject _trash in player.glued) {
                    if (_trash.GetComponent<TrashBehaviour>().id == sorted.id) {
                        d_trash = _trash;
                        player.glued.Remove(d_trash);
                        player.inv -= _trash.GetComponent<TrashBehaviour>().size;
                        Destroy(d_trash);
                        if (player.glued.Count == 0) {
                            player.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
                            player.GetComponent<Rigidbody>().inertiaTensor = new Vector3(16, 16, 16);
                            player.GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;
                            gameController.CloseMenu(menu);
                        } else {
                            trashPos = Mathf.Clamp(trashPos, 0, player.glued.Count - 1);
                            verticalPos = 2;
                        }
                        return;
                    }
                }
            }

            TrashBehaviour trash;

            //left trash
            if (trashPos > 0) {
                trash = GetTrash(trashPos - 1);
                SetTrash(trash, Resources.Load<GameObject>("Trash/" + trash.mesh), trashRow.GetChild(1), false);
            } else trashRow.GetChild(1).gameObject.SetActive(false);

            //center trash
            trash = GetTrash(trashPos);
            SetTrash(trash, Resources.Load<GameObject>("Trash/" + trash.mesh), trashRow.GetChild(0), true);
            if (verticalPos == 1) trashRow.GetChild(0).GetChild(1).gameObject.SetActive(true); else trashRow.GetChild(0).GetChild(1).gameObject.SetActive(false);

            //right trash
            if (trashPos < player.glued.Count - 1) {
                trash = GetTrash(trashPos + 1);
                SetTrash(trash, Resources.Load<GameObject>("Trash/" + trash.mesh), trashRow.GetChild(2), false);
            } else trashRow.GetChild(2).gameObject.SetActive(false);


            //left container
            if (containerPos > 0) SetContainer(containerPos - 1, containerRow.GetChild(1), false);
            else containerRow.GetChild(1).gameObject.SetActive(false);

            //center container
            SetContainer(containerPos, containerRow.GetChild(0), true);
            if (verticalPos == 2) containerRow.GetChild(0).GetChild(1).gameObject.SetActive(true); else containerRow.GetChild(0).GetChild(1).gameObject.SetActive(false);

            //right container
            if (containerPos < 6) SetContainer(containerPos + 1, containerRow.GetChild(2), false);
            else containerRow.GetChild(2).gameObject.SetActive(false);
        }
    }

    //set values and visuals for trash objects
    private void SetTrash(TrashBehaviour trash, GameObject component, Transform destination, bool highlighted) {
        destination.gameObject.SetActive(true);

        //set mesh of trash prefab instance in menu
        destination.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh = component.GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = new Material[component.GetComponent<MeshRenderer>().sharedMaterials.Length];
        for (int i = 0; i < materials.Length; i++)
            materials[i] = component.GetComponent<MeshRenderer>().sharedMaterials[i];
        destination.GetChild(0).GetComponent<MeshRenderer>().sharedMaterials = materials;

        //set info in container
        if (highlighted) {
            infoContainer.GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = trash.name;
            infoContainer.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = trash.info;
        }
    }

    //return TrashBehaviour of specified trash object
    private TrashBehaviour GetTrash(int i) { return player.glued[i].GetComponent<TrashBehaviour>(); }

    //set values and visuals for container objects
    private void SetContainer(int container, Transform destination, bool highlighted) {
        destination.gameObject.SetActive(true);

        switch (container) {
            case 0: SetContainerMesh(Resources.Load<GameObject>("Container/Yellow_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Gelbe Tonne";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Hier hinein gehören Verpackungen aus Kunststoff, Weißblech und Aluminium," +
                                                                                                           "ebenso Tuben, Konservendosen und Plastiktüten. Verbundverpackungen," +
                                                                                                           "wie Getränkekartons, oder Styroporhaltige Stoffe zählen auch darunter.";
                    } break;
            case 1: SetContainerMesh(Resources.Load<GameObject>("Container/Blue_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Blaue Tonne";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Hier hinein gehören Abfälle aus Pappe oder Papier darunter Zeitungen, Zeitschriften, " +
                                                                                                           "Schreibpapier, Verpackungen oder Geschenkpapier.Ausgeschlossen davon sind" +
                                                                                                           "imprägnierte oder beschichtete Papiere und verschmiertes Papier.";
                    } break;
            case 2: SetContainerMesh(Resources.Load<GameObject>("Container/Brown_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Biomüll";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Hier hinein gehören alle zur Kompostierung geeigneten organischen Abfälle, " +
                                                                                                           "wie Pflanzenreste, Gartenabfälle, Obst-und Gemüseabfälle, aber auch Kaffee- und Teefilter." +
                                                                                                           "Jegliche Form von Plastik ist in dieser Tonne nicht erwünscht.";
                    } break;
            case 3: SetContainerMesh(Resources.Load<GameObject>("Container/Grey_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Restmüll";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Hier hinein gehören Zigaretten und Asche, Tierkot und Streu, Windeln, " +
                                                                                                           "zerbrochenes Porzellan oder Glas, benutztes Papier, kaputte Haushaltsgegenstände und CDs oder DVDs.";
                    } break;
            case 4: SetContainerMesh(Resources.Load<GameObject>("Container/Bulky_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Sperrmüll";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Für Mülltonnen unpassende Abfälle gelten als \"Sperrmüll\" und werden gesondert abgeholt" +
                                                                                                           "oder auf Wertstoffhöfe gebracht. Hier hinein gehören ausrangierte Möbel, Tapetenreste, und alte Teppiche.";
                    } break;
            case 5: SetContainerMesh(Resources.Load<GameObject>("Container/Glass_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Glasmüll";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Hier hinein gehören alle Einwegglasflaschen oder -gläser, unter anderem Getränkeflaschen, " +
                                                                                                           "Einmachgläser, Konservengläser und weitere.Hierbei gibt es auch eine Unterscheidung" +
                                                                                                           "zwischen Weißglas, Buntglas und Braunglas.";
                    } break;
            case 6: SetContainerMesh(Resources.Load<GameObject>("Container/Electro_Trash_Container"), destination.GetChild(0).gameObject);
                    if (highlighted) {
                        infoContainer.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Elektromüll";
                        infoContainer.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Hier hinein gehören Elektronik-Geräte wie Toaster, Bügeleisen, Laptops, Handys" +
                                                                                                           "Haushaltsmaschienen und sämtliche elektronische Kabel.";
                    }  break;
        }
    }

    //set mesh of container prefab instance in menu
    private void SetContainerMesh(GameObject component, GameObject destination) {
        destination.GetComponent<MeshFilter>().mesh = component.GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = new Material[component.GetComponent<MeshRenderer>().sharedMaterials.Length];
        for (int i = 0; i < materials.Length; i++)
            materials[i] = component.GetComponent<MeshRenderer>().sharedMaterials[i];
        destination.GetComponent<MeshRenderer>().sharedMaterials = materials;
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
