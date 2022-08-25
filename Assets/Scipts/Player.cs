using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour {
    public GameController gameController;
    private Rigidbody rb;
    private Vector2 keyboardData;

    private wrmhlRead gyroscope; //class which holds gyro and accelerometer data 
        private float dirXValue; //x | mapped gyro x value for left/right direction
        private float dirZValue; //z | mapped gyro y value for forward/backward direction
        public float sensitivity = 0.1f; //treshhold for rigidbody reaction

    public int acceleration;
    public int speedMax;

    public int currency = 0;

    public int inv = 0;
    public int invMax = 10;

    public int jump;
    private float fallMult = 2.7f; //multiplier for quicker fall
    private float jumpMult = 1.8f; //multiplier for slower ascend

    public List<int> collected = new List<int>(); //every ever collected trash object
    public List<GameObject> glued = new List<GameObject>(); //currently picked up trash objects

    public List<Upgrade> upgrades = new List<Upgrade>();

    public float idle; //idle timer

    void Awake() { 
        rb = GetComponent<Rigidbody>();
        gyroscope = transform.GetChild(0).gameObject.GetComponent<wrmhlRead>();
    }

    void Update() {
        //directional keyboard input
        int x = 0;
        if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[0]))) x = 1;
        else if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[1]))) x = -1;

        int z = 0;
        if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[2]))) z = -1;
        else if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[3]))) z = 1;
        keyboardData = new Vector2(x, z);
        
        //map gyro angle to number between -1 and 1
        if (gyroscope.deviceStatus == 1) {
            dirXValue = Mathf.Lerp(-1, 1, Mathf.InverseLerp(90, -90, gyroscope.gyroX));
            dirZValue = Mathf.Lerp(-1, 1, Mathf.InverseLerp(90, -90, gyroscope.gyroY));
        } else dirXValue = dirZValue = 0;
    }

    void FixedUpdate() {
        //add and remove velocity
        if (Mathf.Abs(dirXValue) >= sensitivity || Mathf.Abs(dirZValue) >= sensitivity) rb.AddForce(new Vector3(dirXValue, 0, dirZValue) * acceleration);
        else if (keyboardData.x != 0 || keyboardData.y != 0) rb.AddForce(new Vector3(keyboardData.y, 0, keyboardData.x) * acceleration);
        else {
            rb.velocity = new Vector3(LerpVelocity(rb.velocity.x), rb.velocity.y, LerpVelocity(rb.velocity.z));
            rb.angularVelocity = new Vector3(LerpVelocity(rb.angularVelocity.x), LerpVelocity(rb.angularVelocity.y), LerpVelocity(rb.angularVelocity.z));
        }

        //jump input
        if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), "" + gameController.inputs[4])) || gyroscope.jump == 1)
            if (IsGrounded() && jump != 0)
                rb.velocity = new Vector3(rb.velocity.x, jump, rb.velocity.z);

        //restrict speed
        if (Mathf.Abs(rb.velocity.x) > speedMax) rb.velocity = new Vector3(speedMax * Mathf.Sign(rb.velocity.x), rb.velocity.y, rb.velocity.z);
        if (Mathf.Abs(rb.velocity.z) > speedMax) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, speedMax * Mathf.Sign(rb.velocity.z));
        if (rb.velocity.y < 0)
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMult - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0)
            rb.velocity += Vector3.up * Physics.gravity.y * (jumpMult - 1) * Time.deltaTime;

        //check idle time
        if (Mathf.Abs(rb.velocity.x) <= 0.2f && Mathf.Abs(rb.velocity.y) <= 0.2f && Mathf.Abs(rb.velocity.z) <= 0.2f) idle += Time.deltaTime;
        else idle = 0;
    }

    //try to pickup trashBehaviour Object on collision
    public void OnCollisionEnter(Collision other) {
        TrashBehaviour obj = other.gameObject.GetComponent<TrashBehaviour>();
        if (obj != null) {
            if (inv + obj.size <= invMax) {
                Trash trash = obj.GetTrash();
                gameController.transform.GetChild(2).GetComponent<AudioSource>().Play();

                //position new object at collision point and make player parent
                Vector3 gluePoint = 2.3f * Vector3.Normalize(other.transform.position - transform.position) + transform.position;
                GameObject clone = (GameObject)  Instantiate(other.gameObject, gluePoint, Quaternion.Euler(trash.rotX, trash.rotY, trash.rotZ));
                clone.transform.SetParent(gameObject.transform);
                clone.AddComponent(typeof(TrashBehaviour));
                clone.GetComponent<TrashBehaviour>().setValues(trash.id, trash.name, trash.info, trash.size, trash.container, trash.mesh);

                //add trash to list for later use and destroy trash world object
                glued.Add(clone);
                collected.Add(trash.id);
                inv += trash.size;
                Destroy(other.gameObject);
            } else {
                IEnumerator coroutine = FailedPickupFeedback();
                StartCoroutine(coroutine);
            }
        }
    }

    //turn gameUI inventory red on failed pickup
    private IEnumerator FailedPickupFeedback() {
        gameController.trashCounter.GetComponent<TMPro.TextMeshProUGUI>().font = Resources.Load("Fonts/RedFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
        gameController.trashCounter.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("UI/trash_red", typeof(Sprite)) as Sprite;
        for (float i = 0; i <= 0.5f; i += Time.deltaTime) yield return null;
        gameController.trashCounter.GetComponent<TMPro.TextMeshProUGUI>().font = Resources.Load("Fonts/WhiteUIFont", typeof(TMP_FontAsset)) as TMP_FontAsset;
        gameController.trashCounter.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("UI/trash", typeof(Sprite)) as Sprite;
    }

    //slow down velocity by 2%
    private float LerpVelocity(float velocity) { return Mathf.Lerp(0, velocity, 0.98f); }

    //raycast check if on terrain
    private bool IsGrounded() {
        LayerMask mask = LayerMask.GetMask("Terrain");
        return Physics.Raycast(transform.position, Vector3.down, 2.1f, mask);
    }

    //get list of glued Trash Objects
    public List<Trash> GetTrash() {
        List<Trash> trash = new List<Trash>();
        foreach(GameObject _trash in glued) { trash.Add(_trash.GetComponent<TrashBehaviour>().GetTrash()); }
        return trash;
    }
}
