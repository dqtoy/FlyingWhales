using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackPickerItem : ObjectPickerItem<Character>, IDragParentItem {
    private Character character;

    public CharacterPortrait portrait;
    public GameObject portraitCover;
    public Image jobIcon;

    #region getters/setters
    public object associatedObj {
        get { return character; }
    }
    public override Character obj { get { return character; } }
    #endregion

    public void SetCharacter(Character character) {
        this.character = character;
        UpdateVisuals();
    }

    public override void SetDraggableState(bool state) {
        base.SetDraggableState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.GeneratePortrait(character);
        mainLbl.text = character.name;
        subLbl.text = character.raceClassName;
        //JOB charactersJob = PlayerManager.Instance.player.GetCharactersCurrentJob(character);
        //if(charactersJob != JOB.NONE) {
        //    jobIcon.sprite = CharacterManager.Instance.GetJobSprite(charactersJob);
        //    jobIcon.gameObject.SetActive(true);
        //} else {
        //    jobIcon.gameObject.SetActive(false);
        //}
    }
}
