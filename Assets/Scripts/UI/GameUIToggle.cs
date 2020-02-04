using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class GameUIToggleEvent : UnityEvent { }

[System.Serializable]
public class GameUIToggle : Toggle {

    [SerializeField] private TextMeshProUGUI targetText;

    //public ActionEvent onClickAction;

    private Color onColor;
    private Color offColor;

    public override void OnPointerClick(PointerEventData eventData) {

    }
    public override void OnPointerEnter(PointerEventData eventData) {
        
    }
    public override void OnPointerExit(PointerEventData eventData) {
        
    }

}


