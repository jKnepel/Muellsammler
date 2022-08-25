using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingScreen : MonoBehaviour {
    private Animator anim;

    void Awake() { anim = gameObject.GetComponent<Animator>(); }

    public void StartLoadingScreen() {
        anim.Play("Fade_Start");
        Time.timeScale = 0;
    }

    public void EndLoadingScreen() {
        anim.Play("Fade_End");
        Time.timeScale = 1;
    }
}
