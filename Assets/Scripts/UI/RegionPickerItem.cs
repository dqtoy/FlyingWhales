using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegionPickerItem : ObjectPickerItem<Region>, IPointerClickHandler {

    public Action<Region> onClickAction;

    private Region region;

    [SerializeField] private LocationPortrait portrait;
    public GameObject portraitCover;

    public override Region obj { get { return region; } }

    public void SetRegion(Region region) {
        this.region = region;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.SetLocation(region);
        mainLbl.text = region.name;
        //subLbl.text = Utilities.GetNormalizedSingularRace(area.race) + " " + area.characterClass.className;
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(region);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            //portrait.OnClick();
            UIManager.Instance.ShowHextileInfo(region.coreTile);
        } else {
            OnClick();
        }
    }
}

