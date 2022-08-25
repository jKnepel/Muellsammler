using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashBehaviour : MonoBehaviour {
    public int id;
    public new string name;
    public string info;
    public int size;
    public int container; //0 == yellow, 1 == blue, 2 == brown/bio, 3 == grey/residual, 4 == bulky, 5 == glass, 6 == electro
    public string mesh;

    public void setValues(int id, string name, string info, int size, int container, string mesh) {
        this.id = id;
        this.name = name;
        this.info = info;
        this.size = size;
        this.container = container;
        this.mesh = mesh;
    }

    //get serializable version of this object
    public Trash GetTrash() {
        Trash trash = new Trash();
        trash.id = id;
        trash.name = name;
        trash.info = info;
        trash.size = size;
        trash.container = container;
        trash.mesh = mesh;
        trash.posX = transform.localPosition.x;
        trash.posY = transform.localPosition.y;
        trash.posZ = transform.localPosition.z;
        trash.rotX = transform.localEulerAngles.x;
        trash.rotY = transform.localEulerAngles.y;
        trash.rotZ = transform.localEulerAngles.z;

        return trash;
    }
}
