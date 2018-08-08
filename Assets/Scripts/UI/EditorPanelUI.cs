using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPanelUI : MonoBehaviour {
    public static EditorPanelUI Instance;

    public GameObject page1GO;
    public GameObject page2GO;

    void Awake() {
        Instance = this;
    }

    public void OnClickPage1() {
        page1GO.SetActive(true);
        page2GO.SetActive(false);
    }
    public void OnClickPage2() {
        page1GO.SetActive(false);
        page2GO.SetActive(true);
    }
}
