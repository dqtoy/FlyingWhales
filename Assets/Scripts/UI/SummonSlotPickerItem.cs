using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SummonSlotPickerItem : ObjectPickerItem<SummonSlot>, IPointerClickHandler {

    public Action<SummonSlot> onClickAction;

    private SummonSlot summonSlot;

    [SerializeField] private CharacterPortrait portrait;
    public GameObject portraitCover;

    public override SummonSlot obj { get { return summonSlot; } }

    public void SetSummonSlot(SummonSlot summon) {
        this.summonSlot = summon;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.GeneratePortrait(summonSlot.summon);
        mainLbl.text = summonSlot.summon.name;
        subLbl.text = summonSlot.summon.summonType.SummonName();
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(summonSlot);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            portrait.OnClick();
        } else {
            OnClick();
        }
    }
}
