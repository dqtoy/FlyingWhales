using ECS;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public NewParty party { get; private set; }
    private BaseLandmark _landmark;

    private bool isHovering = false;
    private HoveredObject hoveredObject = HoveredObject.None;
    private enum HoveredObject {
        None,
        Party,
        Character
    }

    [Header("Visitors")]
    [SerializeField] private CharacterPortrait visitorPortrait;
    [SerializeField] private Image visitorParty;

    [Header("Residents")]
    [SerializeField] private CharacterPortrait residentsPortrait;
    [SerializeField] private Image residentsParty;

    [Header("Action")]
    [SerializeField] private ActionIcon actionIcon;

    public void SetParty(NewParty party, BaseLandmark landmark) {
        this.party = party;
        _landmark = landmark;
        UpdateVisuals();
        Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_LEFT_PARTY, OnCharacterLeftParty);
        actionIcon.Initialize();
        actionIcon.SetCharacter(party.mainCharacter);
        actionIcon.SetAction(party.currentAction);
    }

    public void UpdateVisuals() {
        if (_landmark.IsResident(party.owner)) {
            //resident
            if (party.icharacters.Count > 1) {
                //use party icon
                residentsParty.gameObject.SetActive(true);
                residentsPortrait.gameObject.SetActive(false);
            } else {
                //use character portrait
                residentsParty.gameObject.SetActive(false);
                residentsPortrait.gameObject.SetActive(true);
                residentsPortrait.GeneratePortrait(party.owner, IMAGE_SIZE.X64, true, true);
            }
            visitorParty.gameObject.SetActive(false);
            visitorPortrait.gameObject.SetActive(false);
        } else {
            //visitor
            if (party.icharacters.Count > 1) {
                //use party icon
                visitorParty.gameObject.SetActive(true);
                visitorPortrait.gameObject.SetActive(false);
            } else {
                //use character portrait
                visitorParty.gameObject.SetActive(false);
                visitorPortrait.gameObject.SetActive(true);
                visitorPortrait.GeneratePortrait(party.owner, IMAGE_SIZE.X64, true, true);
            }
            residentsParty.gameObject.SetActive(false);
            residentsPortrait.gameObject.SetActive(false);
        }
    }

    public void OnHoverPartyIcon() {
        isHovering = true;
        hoveredObject = HoveredObject.Party;
    }
    public void OnHoverOverCharacter() {
        isHovering = true;
        hoveredObject = HoveredObject.Character;
    }
    public void OnHoverOut() {
        isHovering = false;
        hoveredObject = HoveredObject.None;
        UIManager.Instance.HideSmallInfo();
        UIManager.Instance.HideDetailedInfo();
    }
    public void OnClickParty() {
        UIManager.Instance.ShowPartyInfo(party);
    }


    private void Update() {
        if (isHovering) {
            if (hoveredObject == HoveredObject.Party) {
                UIManager.Instance.ShowDetailedInfo(party);
            } else if (hoveredObject == HoveredObject.Character) {
                UIManager.Instance.ShowSmallInfo(party.owner.name);
            }
        }
    }

    #region Listeners
    private void OnCharacterJoinedParty(ICharacter character, NewParty affectedParty) {
        if (party.id == affectedParty.id) {
            UpdateVisuals();
        }
    }
    private void OnCharacterLeftParty(ICharacter character, NewParty affectedParty) {
        if (party.id == affectedParty.id) {
            UpdateVisuals();
        }
    }
    #endregion

    public override void Reset() {
        base.Reset();
        if (isHovering) {
            UIManager.Instance.HideSmallInfo();
            UIManager.Instance.HideDetailedInfo();
        }
        party = null;
        _landmark = null;
        actionIcon.Reset();
        isHovering = false;
        hoveredObject = HoveredObject.None;
        Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_LEFT_PARTY, OnCharacterLeftParty);
    }
}
