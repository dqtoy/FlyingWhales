using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelItem : MonoBehaviour {

    private Intel intel;

    public void SetIntel(Intel intel) {
        this.intel = intel;
    }

    public void ShowIntelInfo() {
        UIManager.Instance.ShowSmallInfo(intel.description);
    }
    public void HideIntelInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
