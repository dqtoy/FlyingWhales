using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ApplyTooltipUIMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public UIMenu uiParent;

    public void OnPointerEnter(PointerEventData eventData) {
        uiParent.ShowTooltip(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.Instance.HideSmallInfo();
    }
}
