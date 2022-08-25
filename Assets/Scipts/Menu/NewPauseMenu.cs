using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewPauseMenu : MonoBehaviour {
    public Player player;
    public GameController gameController;

    public GameObject saveMsg;

    public void Continue() { gameController.CloseMenu(transform.GetChild(0).gameObject); }

    //save savegame and start save message coroutine
    public void Save() {
        GameObject.Find("SaveSystem").GetComponent<NewSaveSystem>().SaveGame();
        IEnumerator coroutine = SaveFeedback();
        StartCoroutine(coroutine);
    }

    //coroutine to show save message for 1.5 seconds
    private IEnumerator SaveFeedback() {
        saveMsg.SetActive(true);
        for (float i = 0; i < 1.5f; i += Time.unscaledDeltaTime) yield return null;
        saveMsg.SetActive(false);
    }

    //open reset window
    public void Reset() {
        gameController.CloseMenu(transform.GetChild(0).gameObject);
        gameController.OpenMenu(transform.GetChild(1).gameObject);
    }

    //reset player to base and remove glued trash
    public void ContinueReset() {
        gameController.StartLoadingScreen();

        //remove and destroy glued trash
        GameObject trash;
        for (int i = player.glued.Count - 1; i >= 0; i--) {
            trash = player.glued[i];
            player.inv -= trash.GetComponent<TrashBehaviour>().size;
            player.glued.RemoveAt(i);
            Destroy(trash);
        }

        int resets = int.Parse(gameController.statistics.transform.GetChild(7).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
        gameController.statistics.transform.GetChild(7).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + (resets + 1);

        //reset player position and rigidbody values
        player.transform.position = new Vector3(0, 32.5f, 0);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        player.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
        player.GetComponent<Rigidbody>().inertiaTensor = new Vector3(16, 16, 16);
        player.GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;

        gameController.CloseMenu(transform.GetChild(1).gameObject);

        gameController.EndLoadingScreen();
    }

    //close reset menu
    public void BackReset() {
        gameController.CloseMenu(transform.GetChild(1).gameObject);
        gameController.OpenMenu(transform.GetChild(0).gameObject);
    }

    //open statistics menu
    public void Stats() {
        gameController.CloseMenu(transform.GetChild(0).gameObject);
        gameController.OpenMenu(transform.GetChild(2).gameObject);
    }

    //close statistics menu
    public void BackStats() {
        gameController.CloseMenu(transform.GetChild(2).gameObject);
        gameController.OpenMenu(transform.GetChild(0).gameObject);
    }

    //open options menu
    public void Options() {
        gameController.CloseMenu(transform.GetChild(0).gameObject);
        gameController.OpenMenu(transform.GetChild(3).gameObject);
    }

    //close options menu and save options
    public void BackOptions() {
        gameController.SaveOptions();
        gameController.CloseMenu(transform.GetChild(3).gameObject);
        gameController.OpenMenu(transform.GetChild(0).gameObject);
    }

    //return to game menu
    public void Close() {
        SceneManager.LoadScene("GameMenu");
    }
}
