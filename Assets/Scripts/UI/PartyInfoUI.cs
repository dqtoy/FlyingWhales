using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;


public class PartyInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private PartyEmblem partyEmblem;
    [SerializeField] private TMP_InputField partyField;
    [SerializeField] private SlotItem[] partySlots;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private GameObject locationGO;
    [SerializeField] private TextMeshProUGUI locationLbl;
    [SerializeField] private Image locationImg;

    [SerializeField] private TextMeshProUGUI partyNameErrorLbl;
    [SerializeField] private TextMeshProUGUI partyMembersErrorLbl;

    private PartyHolder partyHolder;

    internal Party currentlyShowingParty {
        get { return _data as Party; }
    }

    internal override void Initialize() {
        base.Initialize();
        partyHolder = new PartyHolder();
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            //currSlot.SetNeededType(typeof(IUnit));
            currSlot.SetSlotIndex(i);
        }
        Messenger.AddListener<CharacterAction, Party>(Signals.ACTION_TAKEN, OnActionTakenByParty);
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdatePartyInfo();
        UpdateEditableItems();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdatePartyInfo();
            UpdateEditableItems();
        }
    }
    public override void CloseMenu() {
        base.CloseMenu();
        _data = null;
    }

    public void UpdatePartyInfo() {
        if(currentlyShowingParty == null) {
            return;
        }
		if(!currentlyShowingParty.isDead){
            UpdateGeneralInfo();
            UpdatePartyCharacters();
            UpdateLocation();
            confirmBtn.gameObject.SetActive(false);
        }
    }
    private void UpdateGeneralInfo() {
        partyField.text = currentlyShowingParty.partyName;
        partyEmblem.SetParty(currentlyShowingParty);
    }
    private void UpdatePartyCharacters() {
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currItem = partySlots[i];
            Character character = currentlyShowingParty.characters.ElementAtOrDefault(i);
            currItem.PlaceObject(character);
        }
    }

    private void UpdateEditableItems() {
        partyNameErrorLbl.gameObject.SetActive(false);
        partyMembersErrorLbl.gameObject.SetActive(false);
        if (currentlyShowingParty is CharacterParty) {
            CharacterParty party = currentlyShowingParty as CharacterParty;
            if (party.owner.minion != null && !party.isBusy) {
                //the owner of the party is a minion, and they are not busy, allow editing
                partyField.interactable = true;
                for (int i = 0; i < partySlots.Length; i++) {
                    SlotItem currSlot = partySlots[i];
                    currSlot.dropZone.SetEnabledState(true);
                    currSlot.draggable.SetDraggable(true);
                }
                CopyValues(currentlyShowingParty);
                return;
            }
        }
        //the owner of the party is NOT a minion, disable editing
        partyField.interactable = false;
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            currSlot.dropZone.SetEnabledState(false);
            currSlot.draggable.SetDraggable(false);
        }
    }
    private void UpdateLocation() {
        locationGO.SetActive(true);
        if (currentlyShowingParty.specificLocation is BaseLandmark) {
            locationLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters((currentlyShowingParty.specificLocation as BaseLandmark).specificLandmarkType.ToString());
            locationImg.sprite = currentlyShowingParty.specificLocation.tileLocation.mainStructureSprite.sprite;
            locationImg.gameObject.SetActive(true);
        } else {
            locationLbl.text = currentlyShowingParty.specificLocation.locationName;
            locationImg.gameObject.SetActive(false);
        }
        
    }

    private void OnActionTakenByParty(CharacterAction action, Party party) {
        if (currentlyShowingParty != null && currentlyShowingParty.id == party.id) {
            SetData(party); //show the info of the party that took the action. This will result in locking the slots and undoing any changes to the party
        }
    }

    #region Create/Edit Party
    public void ShowCreatePartyUI() {
        base.OpenMenu();
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            currSlot.ClearSlot(true);
            //currSlot.SetNeededType(typeof(IUnit));
            //currSlot.itemDroppedCallback = new ItemDroppedCallback();
            currSlot.SetItemDroppedCallback(OnItemDroppedOnSlot);
            currSlot.SetItemDroppedOutCallback(OnItemDroppedOutOfSlot);
            currSlot.dropZone.SetEnabledState(true);
            currSlot.draggable.SetDraggable(true);
        }
        partyNameErrorLbl.gameObject.SetActive(true);
        partyMembersErrorLbl.gameObject.SetActive(true);
        locationGO.SetActive(false);
        confirmBtn.gameObject.SetActive(false);
        UIManager.Instance.ShowMinionsMenu();
        partyHolder = new PartyHolder();
        partyEmblem.SetVisuals(partyHolder.emblemBG, partyHolder.emblem, partyHolder.partyColor);
        partyField.text = partyHolder.name;
        partyField.interactable = true;
    }
    public void OnChangesMade() {
        if (currentlyShowingParty == null) {
            if (string.IsNullOrEmpty(partyField.text) || partyHolder.characters.Count <= 1) { //must have party name and more than 2 characters
                confirmBtn.interactable = false;
                confirmBtn.gameObject.SetActive(false);
                if (string.IsNullOrEmpty(partyField.text)) {
                    partyNameErrorLbl.gameObject.SetActive(true);
                } else {
                    partyNameErrorLbl.gameObject.SetActive(false);
                }
                if (partyHolder.characters.Count <= 1) {
                    partyMembersErrorLbl.gameObject.SetActive(true);
                } else {
                    partyMembersErrorLbl.gameObject.SetActive(false);
                }
            } else {
                confirmBtn.gameObject.SetActive(true);
                confirmBtn.interactable = true;
                partyNameErrorLbl.gameObject.SetActive(false);
                partyMembersErrorLbl.gameObject.SetActive(false);
            }
        } else {
            if (!partyHolder.name.Equals(currentlyShowingParty.partyName) || !HasSameCharacters(currentlyShowingParty, partyHolder)) {
                //if the currently showing party has a different name or has different characters, changes were made, check if changes are valid
                if (string.IsNullOrEmpty(partyField.text) || partyHolder.characters.Count <= 1) { //must have party name and more than 2 characters
                    confirmBtn.interactable = false;
                    confirmBtn.gameObject.SetActive(false);
                    if (string.IsNullOrEmpty(partyField.text)) {
                        partyNameErrorLbl.gameObject.SetActive(true);
                    } else {
                        partyNameErrorLbl.gameObject.SetActive(false);
                    }
                    if (partyHolder.characters.Count <= 1) {
                        partyMembersErrorLbl.gameObject.SetActive(true);
                    } else {
                        partyMembersErrorLbl.gameObject.SetActive(false);
                    }
                } else {
                    confirmBtn.gameObject.SetActive(true);
                    confirmBtn.interactable = true;
                    partyNameErrorLbl.gameObject.SetActive(false);
                    partyMembersErrorLbl.gameObject.SetActive(false);
                }
            } else {
                //hide confirm btn
                confirmBtn.gameObject.SetActive(false);
                confirmBtn.interactable = false;
                if (string.IsNullOrEmpty(partyField.text)) {
                    partyNameErrorLbl.gameObject.SetActive(true);
                } else {
                    partyNameErrorLbl.gameObject.SetActive(false);
                }
                if (partyHolder.characters.Count <= 1) {
                    partyMembersErrorLbl.gameObject.SetActive(true);
                } else {
                    partyMembersErrorLbl.gameObject.SetActive(false);
                }
            }
        }
        
    }
    private void OnItemDroppedOnSlot(object obj, int slotIndex) {
        //Debug.Log(obj.ToString() + " dropped at slot " + slotIndex);
        Character characterToAdd = null;
        SlotItem item = partySlots[slotIndex];
        if (obj is Minion) {
            characterToAdd = (obj as Minion).character;
        } else if (obj is Character) {
            characterToAdd = obj as Character;
        } else {
            //dragged item was invalid
            item.ClearSlot(true);
        }
        if (characterToAdd != null) {
            if (characterToAdd.IsInParty()) {
                if (currentlyShowingParty != null && characterToAdd.currentParty.id == currentlyShowingParty.id) {
                    //the character to add is already part of the current party, if he/she is not in the party holder, allow placement, 
                    //because he/she was probably dragged out, and can be returned
                    if (!partyHolder.characters.Contains(characterToAdd)) {
                        partyHolder.AddCharacter(characterToAdd);
                    } else {
                        //if the character is not in his/her own party (means he/she is already in another party)
                        item.ClearSlot(true);
                        Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, characterToAdd.name + " is already part of this party!", true);
                    }
                } else {
                    //if the character is not in his/her own party and is not part of the current party(means he/she is already in another party)
                    item.ClearSlot(true);
                    Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, characterToAdd.name + " is already part of another party!", true);
                }
            } else if (partyHolder.characters.Contains(characterToAdd)) {
                item.ClearSlot(true);
                Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, characterToAdd.name + " is already part of this party!", true);
            } else {
                partyHolder.AddCharacter(characterToAdd);
            }
        }

        OnChangesMade();
    }
    private void OnItemDroppedOutOfSlot(object obj, int slotIndex) {
        Debug.Log(obj.ToString() + " dropped out of slot " + slotIndex);
        SlotItem item = partySlots[slotIndex];
        if (obj is Minion) {
            partyHolder.RemoveCharacter((obj as Minion).character);
        } else if (obj is Character) {
            partyHolder.RemoveCharacter(obj as Character);
        }
        item.ClearSlot(true);
        OnChangesMade();
    }
    public void OnClickSave() {
        if (currentlyShowingParty != null) {
            List<Character> charactersToRemove = new List<Character>();
            for (int i = 0; i < currentlyShowingParty.characters.Count; i++) {
                Character currCharacter = currentlyShowingParty.characters[i];
                if (!partyHolder.characters.Contains(currCharacter)) {
                    charactersToRemove.Add(currCharacter);
                }
            }
            if (charactersToRemove.Contains(currentlyShowingParty.owner)) {
                //the party owner was removed from the party
                //remove all other characters instead
                charactersToRemove = new List<Character>(currentlyShowingParty.characters);
                charactersToRemove.Remove(currentlyShowingParty.owner);
            }
            for (int i = 0; i < charactersToRemove.Count; i++) {
                currentlyShowingParty.RemoveCharacter(charactersToRemove[i]);
            }
        }
        

        Party partyToUse = partyHolder.characters[0].ownParty;
        partyToUse.SetPartyName(partyHolder.name);
        partyToUse.SetEmblemSettings(partyHolder.emblemBG, partyHolder.emblem, partyHolder.partyColor);
        for (int i = 1; i < partyHolder.characters.Count; i++) {
            Character currCharacter = partyHolder.characters[i];
            partyToUse.AddCharacter(currCharacter);
        }
        SetData(partyToUse);
        OnChangesMade();
        Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "Saved party!", true);
    }
    public void SetPartyHolderName(string name) {
        partyHolder.SetName(name);
    }
    private bool HasSameCharacters(Party party, PartyHolder other) {
        for (int i = 0; i < party.characters.Count; i++) {
            Character partyCharacter = party.characters[i];
            if (!other.characters.Contains(partyCharacter)) {
                return false;
            }
        }
        for (int i = 0; i < other.characters.Count; i++) {
            Character otherPartyCharacter = other.characters[i];
            if (!party.characters.Contains(otherPartyCharacter)) {
                return false;
            }
        }
        return true;
    }
    private void CopyValues(Party party) {
        partyHolder.characters.Clear();
        partyHolder.SetEmblemSettings(party.emblemBG, party.emblem, party.partyColor);
        partyHolder.SetName(party.partyName);
        for (int i = 0; i < party.characters.Count; i++) {
            partyHolder.AddCharacter(party.characters[i]);
        }
    }
    #endregion
}


public class PartyHolder {

    public string name { get; private set; }
    public List<Character> characters { get; private set; }
    public EmblemBG emblemBG { get; private set; }
    public Sprite emblem { get; private set; }
    public Color partyColor { get; private set; }

    public PartyHolder() {
        name = string.Empty;
        characters = new List<Character>();
        SetEmblemSettings(CharacterManager.Instance.GetRandomEmblemBG(), 
            CharacterManager.Instance.GetRandomEmblem(), 
            UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
    }

    public void SetName(string name) {
        this.name = name;
    }
    public void SetEmblemSettings(EmblemBG emblemBG, Sprite emblem, Color partyColor) {
        this.emblemBG = emblemBG;
        this.emblem = emblem;
        this.partyColor = partyColor;
    }
    public void AddCharacter(Character character) {
        if (!characters.Contains(character)) {
            characters.Add(character);
        }
    }
    public void RemoveCharacter(Character character) {
        characters.Remove(character);
    }
}