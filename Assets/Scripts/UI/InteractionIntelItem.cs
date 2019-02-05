using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionIntelItem : MonoBehaviour {

    private InteractionIntel intel;

    [SerializeField] private TextMeshProUGUI infoLbl;
    [SerializeField] private Button mainBtn;

    public void SetIntel(InteractionIntel intel) {
        this.intel = intel;
        if (intel != null) {
            infoLbl.text = "On <b>" + intel.obtainedFromLog.date.ConvertToDays() + "</b>: " +  Utilities.LogReplacer(intel.obtainedFromLog);
            mainBtn.interactable = true;
        } else {
            infoLbl.text = "Get some intel!";
            mainBtn.interactable = false;
        }
        
    }
    public void ShowLogDebugInfo() {
        if (intel != null) {
            string text = intel.GetDebugInfo();
            UIManager.Instance.ShowSmallInfo(text);
        }
    }
    public void HideLogDebugInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
