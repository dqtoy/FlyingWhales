using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntelItem : MonoBehaviour {

    private Intel intel;

    [SerializeField] private Image intelImage;

    public void SetIntel(Intel intel) {
        this.intel = intel;
        UpdateVisuals();
    }

    private void UpdateVisuals() {
        if (PlayerManager.Instance.player.HasIntel(intel)) {
            intelImage.color = Color.white;
        } else {
            intelImage.color = Color.gray;
        }
    }
    public void ShowIntelInfo() {
        if (PlayerManager.Instance.player.HasIntel(intel)) {
            UIManager.Instance.ShowSmallInfo(intel.description);
        }
    }
    public void HideIntelInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
