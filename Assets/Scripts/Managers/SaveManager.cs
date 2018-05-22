using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    public Save currentSave;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(Instance);
    }
}
