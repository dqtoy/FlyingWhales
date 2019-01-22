
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public Character character { get; private set; }

    public CharacterPortrait portrait;

    private UIMenu parentMenu;

    [SerializeField] private RectTransform thisTrans;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private GameObject travellingIcon;
    [SerializeField] private GameObject arrivedIcon;
    [SerializeField] private GameObject coverGO;

    public void SetCharacter(Character character, UIMenu parentMenu) {
        this.character = character;
        this.parentMenu = parentMenu;
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = Utilities.GetNormalizedSingularRace(character.race) + " " + character.characterClass.className;
        UpdateLocationIcons();
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }

    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        if (character.currentParty.characters.Count > 1) {
            UIManager.Instance.ShowSmallInfo(character.currentParty.name);
        } else {
            UIManager.Instance.ShowSmallInfo(character.name);
        }
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    private void UpdateLocationIcons() {
        if (parentMenu is AreaInfoUI) {
            if (character.currentParty.icon.isTravelling) {
                travellingIcon.SetActive(true);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(true);
            } else if ((parentMenu as AreaInfoUI).activeArea.areaResidents.Contains(character)) { //only check for arrival icon if the character is a resident of the showing area
                if (character.specificLocation.tileLocation.areaOfTile.id == character.homeArea.id) {
                    arrivedIcon.SetActive(false);
                    travellingIcon.SetActive(false);
                    coverGO.SetActive(false);
                } else {
                    arrivedIcon.SetActive(true);
                    travellingIcon.SetActive(false);
                    coverGO.SetActive(true);
                }
            } else {
                travellingIcon.SetActive(false);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(false);
            }
            (parentMenu as AreaInfoUI).OrderCharacterItems();
        } else {
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(false);
            coverGO.SetActive(false);
        }
    }

    public void ShowTravellingTooltip() {
        //UIManager.Instance.ShowSmallInfo("Travelling to " + character.currentParty.icon.targetLocation.tileLocation.areaOfTile.name);
        //UIManager.Instance.ShowSmallLocationInfo(character.currentParty.icon.targetLocation.tileLocation.areaOfTile, thisTrans, new Vector3(434f, 0f, 0f), "Travelling to:");
        float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
        UIManager.Instance.ShowSmallLocationInfo(character.currentParty.icon.targetLocation.tileLocation.areaOfTile, new Vector3(x, thisTrans.position.y - 15f, 0f), "Travelling to:");
    }
    public void ShowArrivedTooltip() {
        //UIManager.Instance.ShowSmallInfo("Arrived at " + character.currentParty.specificLocation.tileLocation.areaOfTile.name);
        float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
        UIManager.Instance.ShowSmallLocationInfo(character.currentParty.icon.targetLocation.tileLocation.areaOfTile, new Vector3(x, thisTrans.position.y - 15f, 0f), "Arrived at:");
    }
    public void HideToolTip() {
        UIManager.Instance.HideSmallLocationInfo();
    }

    #region Listeners
    private void OnPartyStartedTravelling(Party party) {
        if (character.currentParty == party) {
            UpdateLocationIcons();
        }
    }
    private void OnPartyDoneTravelling(Party party) {
        if (character.currentParty == party) {
            UpdateLocationIcons();
        }
    }
    #endregion


    public override void Reset() {
        base.Reset();
        //Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        //Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
    }

    private void OnEnable() {
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
    }

    private void OnDisable() {
        if (Messenger.eventTable.ContainsKey(Signals.PARTY_STARTED_TRAVELLING)) {
            Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        }
        if (Messenger.eventTable.ContainsKey(Signals.PARTY_DONE_TRAVELLING)) {
            Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
        }
    }
}
