
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public Character character { get; private set; }
    //private BaseLandmark _landmark;
    //private bool isDefender;

    public CharacterPortrait portrait;

    public void SetCharacter(Character character, BaseLandmark landmark) {
        this.character = character;
        //this.isDefender = isDefender;
        //_landmark = landmark;
        //if (character != null) {
        //    slotItem.PlaceObject(character);
        //} else {
        //    slotItem.ClearSlot(true);
        //}
        portrait.GeneratePortrait(character);
        portrait.SwitchBGToLocked();
    }
    //public void OnEnableSlotAction() {
    //    if (bgImage != null) {
    //        bgImage.sprite = unlockedSprite;
    //        bgImage.SetNativeSize();
    //    }
    //}
    //public void OnDisableSlotAction() {
    //    if (bgImage != null) {
    //        bgImage.sprite = lockedSprite;
    //        bgImage.SetNativeSize();
    //    }
    //}

    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        if (portrait.isLocked) {
            return;
        }
        //if (isDefender) {
        //    UIManager.Instance.ShowSmallInfo(character.name);
        //} else {
        if (character.currentParty.characters.Count > 1) {
            UIManager.Instance.ShowSmallInfo(character.currentParty.name);
        } else {
            UIManager.Instance.ShowSmallInfo(character.name);
        }
        //}
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
