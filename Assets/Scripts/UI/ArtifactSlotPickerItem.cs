using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactSlotPickerItem : ObjectPickerItem<ArtifactSlot>, IPointerClickHandler {

    public Action<ArtifactSlot> onClickAction;

    private ArtifactSlot artifactSlot;

    [SerializeField] private Image portrait;
    public GameObject portraitCover;

    public override ArtifactSlot obj { get { return artifactSlot; } }

    public void SetArtifactSlot(ArtifactSlot summon) {
        this.artifactSlot = summon;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.sprite = CharacterManager.Instance.GetArtifactSettings(artifactSlot.artifact.type).artifactPortrait;
        mainLbl.text = artifactSlot.artifact.name;
        //subLbl.text = artifactSlot.summon.summonType.SummonName();
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(artifactSlot);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnClick();
    }
}

