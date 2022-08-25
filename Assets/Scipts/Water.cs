using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    private GameController gameController;
    private Player player;

    void Awake() {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Update() {
        //calculate closest Point and outer Collider to Player and Lerp Sound Volume for their Distance
        Vector3 closestPoint = gameObject.GetComponent<Collider>().ClosestPointOnBounds(player.transform.position);
        transform.GetChild(0).GetComponent<AudioSource>().volume = Mathf.Lerp(0, 0.35f, Mathf.InverseLerp(40, 0, Vector3.Distance(player.transform.position, closestPoint)));
    }

    //remove all glued objects and reset player
    void OnTriggerEnter(Collider other) {
        if (other.name == "Player") {
            transform.GetChild(1).GetComponent<AudioSource>().Play();

            gameController.StartLoadingScreen();

            //remove and destroy glued trash
            GameObject trash;
            for (int i = player.glued.Count - 1; i >= 0; i--) {
                trash = player.glued[i];
                player.inv -= trash.GetComponent<TrashBehaviour>().size;
                player.glued.RemoveAt(i);
                Destroy(trash);
            }

            //change reset statistic
            int resets = int.Parse(gameController.statistics.transform.GetChild(7).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text);
            gameController.statistics.transform.GetChild(7).GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + (resets + 1);

            //reset player
            other.transform.transform.position = new Vector3(0, 32.5f, 0);
            other.transform.transform.rotation = Quaternion.Euler(0, 0, 0);
            other.transform.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
            other.transform.GetComponent<Rigidbody>().inertiaTensor = new Vector3(16, 16, 16);
            other.transform.GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;

            gameController.EndLoadingScreen();
        }
    }
}
