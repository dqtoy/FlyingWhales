using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntelItem : MonoBehaviour {

    private Intel intel;

    [SerializeField] private Image intelImage;
    [SerializeField] private GameObject lockedGO;

    public void SetIntel(Intel intel) {
        this.intel = intel;
        UpdateVisuals();
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelAddedToPlayer);
    }

    private void UpdateVisuals() {
        if (PlayerManager.Instance.player.HasIntel(intel)) {
            lockedGO.SetActive(false);
        } else {
            lockedGO.SetActive(true);
        }
    }
    public void ShowIntelInfo() {
        if (PlayerManager.Instance.player.HasIntel(intel)) {
            //UIManager.Instance.ShowSmallInfo(intel.description);
        }
    }
    public void HideIntelInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    private void OnIntelAddedToPlayer(Intel intel) {
        UpdateVisuals();
    }

    public void Reset() {
        Messenger.RemoveListener<Intel>(Signals.INTEL_ADDED, OnIntelAddedToPlayer);
    }
}
