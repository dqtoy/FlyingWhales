using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryItem : MonoBehaviour {

    private Memory memory;

    //public delegate void OnClickAction(Intel intel);
    //private OnClickAction onClickAction;

    //private List<System.Action> otherClickActions;

    [SerializeField] private TextMeshProUGUI dateLbl;
    [SerializeField] private TextMeshProUGUI infoLbl;

    //[SerializeField] private Button mainBtn;
    //[SerializeField] private Image iconImg;
    //[SerializeField] private Image clickedImg;

    //[SerializeField] private Sprite eventIntelIcon;
    //[SerializeField] private Sprite objectIntelIcon;

    public void SetMemory(Memory memory) {
        this.memory = memory;
        //otherClickActions = new List<System.Action>();
        //ClearClickActions();
        //SetClickedState(false);
        if (this.memory != null) {
            //string preText = "TIP: ";
            //iconImg.sprite = objectIntelIcon;
            //if (intel is EventIntel) {
            //    //preText = "EVENT: ";
            //    iconImg.sprite = eventIntelIcon;
            //} else if (intel is PlanIntel) {
            //    //preText = "PLAN: ";
            //}
            dateLbl.text = this.memory.date.ConvertToContinuousDaysWithTime(true);
            infoLbl.text = this.memory.text;
            gameObject.SetActive(true);
            //mainBtn.interactable = true;
            //iconImg.gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
            //infoLbl.text = "";
            //mainBtn.interactable = false;
            //iconImg.gameObject.SetActive(false);
        }
    }
    //public void ShowLogDebugInfo() {
    //    if (intel != null) {
    //        //string text = intel.GetDebugInfo();
    //        //UIManager.Instance.ShowSmallInfo(text);
    //    }
    //}
    //public void HideLogDebugInfo() {
    //    UIManager.Instance.HideSmallInfo();
    //}
    //public void SetClickAction(OnClickAction clickAction) {
    //    onClickAction = clickAction;
    //}
    //public void AddOtherClickAction(System.Action clickAction) {
    //    if (otherClickActions != null) {
    //        otherClickActions.Add(clickAction);
    //    }
    //}

    //public void OnClick() {
    //    onClickAction?.Invoke(intel);
    //    for (int i = 0; i < otherClickActions.Count; i++) {
    //        otherClickActions[i]();
    //    }
    //}

    //public void ClearClickActions() {
    //    onClickAction = null;
    //    otherClickActions.Clear();
    //}

    //public void SetClickedState(bool isClicked) {
    //    clickedImg.gameObject.SetActive(isClicked);
    //}
}
