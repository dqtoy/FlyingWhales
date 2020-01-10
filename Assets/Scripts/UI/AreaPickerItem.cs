using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AreaPickerItem : ObjectPickerItem<Settlement>, IPointerClickHandler {

    public Action<Settlement> onClickAction;

    private Settlement _settlement;

    [SerializeField] private LocationPortrait portrait;
    public GameObject portraitCover;

    public override Settlement obj { get { return _settlement; } }

    public void SetArea(Settlement settlement) {
        this._settlement = settlement;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.SetLocation(_settlement.region);
        mainLbl.text = _settlement.name;
        //subLbl.text = Utilities.GetNormalizedSingularRace(settlement.race) + " " + settlement.characterClass.className;
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(_settlement);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            //portrait.OnClick();
            UIManager.Instance.ShowRegionInfo(_settlement.region);
        } else {
            OnClick();
        }
    }
}
