using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AreaPickerItem : ObjectPickerItem<Area>, IPointerClickHandler {

    public Action<Area> onClickAction;

    private Area area;

    [SerializeField] private LocationPortrait portrait;
    public GameObject portraitCover;

    public override Area obj { get { return area; } }

    public void SetArea(Area area) {
        this.area = area;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.SetLocation(area.region);
        mainLbl.text = area.name;
        //subLbl.text = Utilities.GetNormalizedSingularRace(area.race) + " " + area.characterClass.className;
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(area);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            //portrait.OnClick();
            UIManager.Instance.ShowRegionInfo(area.coreTile.region);
        } else {
            OnClick();
        }
    }
}
