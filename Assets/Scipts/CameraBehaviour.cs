using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {
    public Transform target;
    public Vector3 offset = new Vector3(0, 7, -15);

    void Update() {
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}
