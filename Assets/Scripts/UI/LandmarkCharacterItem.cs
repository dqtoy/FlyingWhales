using ECS;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public ICharacter character { get; private set; }
    //private BaseLandmark _landmark;
    private bool isDefender;

    //public CharacterPortrait portrait;
    [SerializeField] private Image bgImage;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;
    public SlotItem slotItem;


    //private void Awake() {
    //    if (slotItem != null) {
            
    //    }
    //}

    public void SetCharacter(ICharacter character, BaseLandmark landmark, bool isDefender = false) {
        this.character = character;
        this.isDefender = isDefender;
        //_landmark = landmark;
        if (character != null) {
            slotItem.PlaceObject(character);
        } else {
            slotItem.ClearSlot(true);
        }
        //if (_landmark.tileLocation.areaOfTile == null 
        //    || _landmark.tileLocation.areaOfTile.id != PlayerManager.Instance.player.playerArea.id) {
        //    slotItem.draggable.SetDraggable(false);
        //} else {
        //    slotItem.draggable.SetDraggable(true);
        //}
        
    }
    public void OnEnableSlotAction() {
        if (bgImage != null) {
            bgImage.sprite = unlockedSprite;
            bgImage.SetNativeSize();
        }
    }
    public void OnDisableSlotAction() {
        if (bgImage != null) {
            bgImage.sprite = lockedSprite;
            bgImage.SetNativeSize();
        }
    }

    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        if (slotItem.portrait.isLocked) {
            return;
        }
        if (isDefender) {
            UIManager.Instance.ShowSmallInfo(character.name);
        } else {
            if (character.currentParty.icharacters.Count > 1) {
                UIManager.Instance.ShowSmallInfo(character.currentParty.name);
            } else {
                UIManager.Instance.ShowSmallInfo(character.name);
            }
        }
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
