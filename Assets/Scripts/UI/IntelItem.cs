using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelItem : MonoBehaviour {

    private Intel intel;

    public delegate void OnClickAction(Intel intel);
    private OnClickAction onClickAction;

    private List<System.Action> otherClickActions;

    [SerializeField] private TextMeshProUGUI infoLbl;
    [SerializeField] private Button mainBtn;

    public void SetIntel(Intel intel) {
        this.intel = intel;
        otherClickActions = new List<System.Action>();
        ClearClickActions();
        if (intel != null) {
            string preText = "Tip: ";
            if (intel is EventIntel) {
                preText = "Event: ";
            } else if (intel is PlanIntel) {
                preText = "Plan: ";
            }
            infoLbl.text = preText +  Utilities.LogReplacer(intel.intelLog);
            mainBtn.interactable = true;
        } else {
            infoLbl.text = "Get some intel!";
            mainBtn.interactable = false;
        }
    }
    public void ShowLogDebugInfo() {
        if (intel != null) {
            //string text = intel.GetDebugInfo();
            //UIManager.Instance.ShowSmallInfo(text);
        }
    }
    public void HideLogDebugInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    public void SetClickAction(OnClickAction clickAction) {
        onClickAction = clickAction;
    }
    public void AddOtherClickAction(System.Action clickAction) {
        if (otherClickActions != null) {
            otherClickActions.Add(clickAction);
        }
    }

    public void OnClick() {
        onClickAction?.Invoke(intel);
        for (int i = 0; i < otherClickActions.Count; i++) {
            otherClickActions[i]();
        }
    }

    public void ClearClickActions() {
        onClickAction = null;
        otherClickActions.Clear();
    }
}
