
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
    [SerializeField] private GameObject unrestrainedGO;
    [SerializeField] private GameObject restrainedIcon;
    [SerializeField] private GameObject coverGO;

    public void SetCharacter(Character character, UIMenu parentMenu) {
        this.character = character;
        this.parentMenu = parentMenu;
        UpdateInfo();
        UpdateLocationIcons();
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }
    private void UpdateInfo() {
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = character.raceClassName;
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
            if(character.GetTraitOr("Abducted", "Restrained") != null) {
                restrainedIcon.SetActive(true);
                unrestrainedGO.SetActive(false);
            } else {
                restrainedIcon.SetActive(false);
                unrestrainedGO.SetActive(true);
            }
            if (character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine != null) {
                travellingIcon.SetActive(true);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(true);
            } else if ((parentMenu as AreaInfoUI).activeArea.areaResidents.Contains(character)) { //only check for arrival icon if the character is a resident of the showing area
                if (character.specificLocation.id == character.homeArea.id) {
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
        Area showingArea = UIManager.Instance.GetCurrentlyShowingSmallInfoLocation();
        if (showingArea == null || showingArea.id != character.currentParty.icon.targetLocation.id) {
            if (character.currentParty.icon.targetLocation == null) {
                return;
            }
            float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
            UIManager.Instance.ShowSmallLocationInfo(character.currentParty.icon.targetLocation, new Vector3(x, thisTrans.position.y - 15f, 0f), "Travelling to:");
        }
    }
    public void ShowArrivedTooltip() {
        //UIManager.Instance.ShowSmallInfo("Arrived at " + character.currentParty.specificLocation.name);
        Area showingArea = UIManager.Instance.GetCurrentlyShowingSmallInfoLocation();
        if (showingArea == null || showingArea.id != character.currentParty.icon.targetLocation.id) {
            if (character.currentParty.icon.targetLocation == null) {
                return;
            }
            float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
            UIManager.Instance.ShowSmallLocationInfo(character.currentParty.icon.targetLocation, new Vector3(x, thisTrans.position.y - 15f, 0f), "Arrived at:");
        }
    }
    public void ShowRestrainedTooltip() {
        string info = string.Empty;
        Trait abductedTrait = character.GetTrait("Abducted");
        Trait restrainedTrait = character.GetTrait("Restrained");
        if (abductedTrait != null) {
            info += abductedTrait.GetToolTipText();
        }
        if (restrainedTrait != null) {
            if(info != string.Empty) {
                info += "\n";
            }
            info += restrainedTrait.GetToolTipText();
        }
        if(info != string.Empty) {
            UIManager.Instance.ShowSmallInfo(info);
        }
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
    private void OnCharacterChangedRace(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnTraitAdded(Character character, Trait trait) {
        if(character.id == this.character.id) {
            if(trait.name == "Abducted" || trait.name == "Restrained") {
                restrainedIcon.SetActive(true);
                unrestrainedGO.SetActive(false);
            }
        }
    }
    private void OnTraitRemoved(Character character, Trait trait) {
        if (character.id == this.character.id) {
            if (trait.name == "Abducted" || trait.name == "Restrained") {
                if(character.GetTraitOr("Abducted", "Restrained") == null) {
                    restrainedIcon.SetActive(false);
                    unrestrainedGO.SetActive(true);
                }
            }
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
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnTraitAdded);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnTraitRemoved);
    }

    private void OnDisable() {
        if (Messenger.eventTable.ContainsKey(Signals.PARTY_STARTED_TRAVELLING)) {
            Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        }
        if (Messenger.eventTable.ContainsKey(Signals.PARTY_DONE_TRAVELLING)) {
            Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
        }
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_CHANGED_RACE)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        }
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnTraitAdded);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnTraitRemoved);
    }
}
