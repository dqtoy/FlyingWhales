using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDesireItem : MonoBehaviour {

    private HiddenDesire hiddenDesire;

    public void SetHiddenDesire(HiddenDesire hiddenDesire) {
        this.hiddenDesire = hiddenDesire;
    }

    public void ShowHiddenDesireInfo() {
        UIManager.Instance.ShowSmallInfo(hiddenDesire.description);
    }
    public void HideHiddenDesireInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
