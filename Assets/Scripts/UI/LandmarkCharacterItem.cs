﻿using ECS;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public ICharacter character { get; private set; }
    private BaseLandmark _landmark;

    [SerializeField] private CharacterPortrait portrait;

    public void SetParty(ICharacter character, BaseLandmark landmark) {
        this.character = character;
        _landmark = landmark;
        if (character != null) {
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait(character, 100);
            portrait.SetBGState(false);
        } else {
            portrait.gameObject.SetActive(false);
        }
        
        //UpdateVisuals();
        //Messenger.AddListener<ICharacter, Party>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        //Messenger.AddListener<ICharacter, Party>(Signals.CHARACTER_LEFT_PARTY, OnCharacterLeftParty);
        //actionIcon.Initialize();
        //actionIcon.SetCharacter(party.mainCharacter);
        //actionIcon.SetAction(party.currentAction);
    }

    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        //if (character.currentParty.icharacters.Count > 1) {
        //    UIManager.Instance.ShowSmallInfo(character.currentParty.name);
        //} else {
            UIManager.Instance.ShowSmallInfo(character.name);
        //}
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    //public void SetPartyData(LandmarkPartyData partyData) {
    //    for (int i = 0; i < visitorPortraits.Length; i++) {
    //        if (i < partyData.partyMembers.Count) {
    //            visitorPortraits[i].gameObject.SetActive(true);
    //            visitorPortraits[i].GeneratePortrait(partyData.partyMembers[i], 42, true);
    //        } else {
    //            visitorPortraits[i].gameObject.SetActive(false);
    //        }
    //    }
    //    actionIcon.SetCurrentDay(partyData.currentDuration);
    //    actionIcon.SetAction(partyData.action);
    //}

    //public void UpdateVisuals() {
    //    for (int i = 0; i < visitorPortraits.Length; i++) {
    //        if(i < party.icharacters.Count) {
    //            visitorPortraits[i].gameObject.SetActive(true);
    //            visitorPortraits[i].GeneratePortrait(party.icharacters[i], 42, true);
    //            visitorPortraits[i].SetBGState(false);
    //        } else {
    //            visitorPortraits[i].gameObject.SetActive(false);
    //        }
    //    }
    //    //if (_landmark.IsResident(party.owner)) {
    //    //    //resident
    //    //    if (party.icharacters.Count > 1) {
    //    //        //use party icon
    //    //        residentsParty.gameObject.SetActive(true);
    //    //        residentsPortrait.gameObject.SetActive(false);
    //    //    } else {
    //    //        //use character portrait
    //    //        residentsParty.gameObject.SetActive(false);
    //    //        residentsPortrait.gameObject.SetActive(true);
    //    //        residentsPortrait.GeneratePortrait(party.owner, 42, true);
    //    //    }
    //    //    visitorParty.gameObject.SetActive(false);
    //    //    visitorPortrait.gameObject.SetActive(false);
    //    //} else {
    //    //    //visitor
    //    //    if (party.icharacters.Count > 1) {
    //    //        //use party icon
    //    //        visitorParty.gameObject.SetActive(true);
    //    //        visitorPortrait.gameObject.SetActive(false);
    //    //    } else {
    //    //        //use character portrait
    //    //        visitorParty.gameObject.SetActive(false);
    //    //        visitorPortrait.gameObject.SetActive(true);
    //    //        visitorPortrait1.GeneratePortrait(party.owner, 42, true);
    //    //    }
    //    //    residentsParty.gameObject.SetActive(false);
    //    //    residentsPortrait.gameObject.SetActive(false);
    //    //}
    //}

    //public void OnHoverPartyIcon() {
    //    //isHovering = true;
    //    //hoveredObject = HoveredObject.Party;
    //}
    //public void OnHoverOverCharacter() {
    //    //isHovering = true;
    //    //hoveredObject = HoveredObject.Character;
    //}
    //public void OnHoverOut() {
    //    //isHovering = false;
    //    //hoveredObject = HoveredObject.None;
    //    //UIManager.Instance.HideSmallInfo();
    //    //UIManager.Instance.HideDetailedInfo();
    //}
    //public void OnClickParty() {
    //    //UIManager.Instance.ShowPartyInfo(party);
    //}


    ////private void Update() {
    ////    if (isHovering) {
    ////        if (hoveredObject == HoveredObject.Party) {
    ////            UIManager.Instance.ShowDetailedInfo(party);
    ////        } else if (hoveredObject == HoveredObject.Character) {
    ////            UIManager.Instance.ShowSmallInfo(party.owner.name);
    ////        }
    ////    }
    ////}

    //#region Listeners
    //private void OnCharacterJoinedParty(ICharacter character, Party affectedParty) {
    //    if (party.id == affectedParty.id) {
    //        UpdateVisuals();
    //    }
    //}
    //private void OnCharacterLeftParty(ICharacter character, Party affectedParty) {
    //    if (party.id == affectedParty.id) {
    //        UpdateVisuals();
    //    }
    //}
    //#endregion

    //public override void Reset() {
    //    base.Reset();
    //    if (isHovering) {
    //        UIManager.Instance.HideSmallInfo();
    //        UIManager.Instance.HideDetailedInfo();
    //    }
    //    party = null;
    //    _landmark = null;
    //    actionIcon.Reset();
    //    isHovering = false;
    //    hoveredObject = HoveredObject.None;
    //    Messenger.RemoveListener<ICharacter, Party>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
    //    Messenger.RemoveListener<ICharacter, Party>(Signals.CHARACTER_LEFT_PARTY, OnCharacterLeftParty);
    //}
}
