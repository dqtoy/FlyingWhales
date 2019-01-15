using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPickerItem<T> : MonoBehaviour {

    [SerializeField] protected TextMeshProUGUI mainLbl;
    [SerializeField] protected TextMeshProUGUI subLbl;
    [SerializeField] protected Button mainBtn;
    [SerializeField] protected NewMinionDraggable draggable;


    public virtual void SetButtonState(bool state) {
        mainBtn.interactable = state;
    }
    public virtual void SetDraggableState(bool state) {
        draggable.SetDraggable(state);
    }
}
