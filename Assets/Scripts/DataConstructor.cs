using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataConstructor : MonoBehaviour {
    public static DataConstructor Instance;

    void Awake() {
        Instance = this;
    }

    public void InitializeData() {
        LocalizationManager.Instance.Initialize();
        CharacterManager.Instance.Initialize();
        RaceManager.Instance.Initialize();
        TraitManager.Instance.Initialize();
        SecretManager.Instance.Initialize();
        LandmarkManager.Instance.Initialize();
    }
}
