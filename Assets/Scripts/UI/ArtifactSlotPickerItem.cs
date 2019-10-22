using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactSlotPickerItem : NameplateItem<ArtifactSlot> {

    [Header("Artifact Slot Attributes")]    
    [SerializeField] private Image portrait;
    //public GameObject portraitCover;

    private ArtifactSlot artifactSlot;
    public override ArtifactSlot obj { get { return artifactSlot; } }

    public override void SetObject(ArtifactSlot o) {
        base.SetObject(o);
        this.artifactSlot = o;
        UpdateVisuals();
    }

    //public override void SetButtonState(bool state) {
    //    base.SetButtonState(state);
    //    portraitCover.SetActive(!state);
    //}

    private void UpdateVisuals() {
        portrait.sprite = CharacterManager.Instance.GetArtifactSettings(artifactSlot.artifact.type).artifactPortrait;
        mainLbl.text = artifactSlot.artifact.name;
        subLbl.text = string.Empty;
    }
}

