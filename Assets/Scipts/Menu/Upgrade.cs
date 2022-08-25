using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Upgrade {
    public int type; //0 == jump, 1 == inv, 2 == acc, 3 == speed
    public int level; //level 1-3

    public string name;
    public string info;
    public int price;
    public int value; //actual value that the player will receive depending on type 

    public Upgrade(int type, int level, string name, string info, int price, int value) {
        this.type = type;
        this.level = level;
        this.name = name;
        this.info = info;
        this.price = price;
        this.value = value;
    }
}
