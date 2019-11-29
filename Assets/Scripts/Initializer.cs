using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour {
    public void InitializeDataBeforeWorldCreation() {
        LocalizationManager.Instance.Initialize();
        CharacterManager.Instance.Initialize();
        RaceManager.Instance.Initialize();
        TraitManager.Instance.Initialize();
        SecretManager.Instance.Initialize();
        LandmarkManager.Instance.Initialize();
        //CombatManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
        TimerHubUI.Instance.Initialize();

        CameraMove.Instance.Initialize();
        InteriorMapManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();

        UIManager.Instance.InitializeUI();

        InteractionManager.Instance.Initialize();
        WorldEventsManager.Instance.Initialize();

        TokenManager.Instance.Initialize();
        JobManager.Instance.Initialize();
    }

    public void InitializeDataAfterWorldCreation() {
        PlayerUI.Instance.Initialize();
        PlayerUI.Instance.InitializeAfterGameLoaded();
    }
}
