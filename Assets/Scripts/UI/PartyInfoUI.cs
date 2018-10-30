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

    internal Party currentlyShowingParty {
        get { return _data as Party; }
    }

    internal override void Initialize() {
        base.Initialize();
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            currSlot.SetNeededType(typeof(IUnit));
            currSlot.SetSlotIndex(i);
        }
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
            confirmBtn.gameObject.SetActive(false);
        }
    }
    private void UpdateGeneralInfo() {
        partyField.text = currentlyShowingParty.partyName;
    }
    private void UpdatePartyCharacters() {
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currItem = partySlots[i];
            ICharacter character = currentlyShowingParty.icharacters.ElementAtOrDefault(i);
            if (character == null) {
                currItem.ClearSlot(true);
            } else {
                currItem.PlaceObject(character);
            }
        }
    }

    private void UpdateEditableItems() {
        if (currentlyShowingParty is CharacterParty) {
            if ((currentlyShowingParty as CharacterParty).owner.minion != null) {
                //the owner of the party is a minion, allow editing
                partyField.interactable = true;
                for (int i = 0; i < partySlots.Length; i++) {
                    SlotItem currSlot = partySlots[i];
                    currSlot.dropZone.SetEnabledState(true);
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
        }
    }

    #region Create/Edit Party
    private PartyHolder partyHolder;
    public void ShowCreatePartyUI() {
        base.OpenMenu();
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            currSlot.ClearSlot();
            currSlot.SetNeededType(typeof(IUnit));
            currSlot.itemDroppedCallback = new ItemDroppedCallback();
            currSlot.itemDroppedCallback.AddListener(OnItemDroppedOnSlot);
            currSlot.itemDroppedOutCallback = new ItemDroppedOutCallback();
            currSlot.itemDroppedOutCallback.AddListener(OnItemDroppedOutOfSlot);
        }
        
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
            } else {
                confirmBtn.gameObject.SetActive(true);
                confirmBtn.interactable = true;
            }
        } else {
            if (!partyHolder.name.Equals(currentlyShowingParty.partyName) || !HasSameCharacters(currentlyShowingParty, partyHolder)) {
                //if the currently showing party has a different name or has different characters, changes were made, check if changes are valid
                if (string.IsNullOrEmpty(partyField.text) || partyHolder.characters.Count <= 1) { //must have party name and more than 2 characters
                    confirmBtn.interactable = false;
                    confirmBtn.gameObject.SetActive(false);
                } else {
                    confirmBtn.gameObject.SetActive(true);
                    confirmBtn.interactable = true;
                }
            } else {
                //hide confirm btn
                confirmBtn.gameObject.SetActive(false);
                confirmBtn.interactable = false;
            }
        }
        
    }
    private void OnItemDroppedOnSlot(object obj, int slotIndex) {
        Debug.Log(obj.ToString() + " dropped at slot " + slotIndex);
        ICharacter characterToAdd = null;
        SlotItem item = partySlots[slotIndex];
        if (obj is Minion) {
            characterToAdd = (obj as Minion).icharacter;
        } else if (obj is ICharacter) {
            characterToAdd = obj as ICharacter;
        } else {
            //dragged item was invalid
            item.ClearSlot(true);
        }
        if (characterToAdd != null) {
            if (characterToAdd.IsInParty()) {
                //if the character is not in his/her own party (means he/she is already in another party)
                item.ClearSlot(true);
                Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, characterToAdd.name + " is already part of another party!", true);
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
            partyHolder.RemoveCharacter((obj as Minion).icharacter);
        } else if (obj is ICharacter) {
            partyHolder.RemoveCharacter(obj as ICharacter);
        }
        item.ClearSlot(true);
        OnChangesMade();
    }
    public void OnClickSave() {
        if (currentlyShowingParty != null) {
            List<ICharacter> charactersToRemove = new List<ICharacter>();
            for (int i = 0; i < currentlyShowingParty.icharacters.Count; i++) {
                ICharacter currCharacter = currentlyShowingParty.icharacters[i];
                if (!partyHolder.characters.Contains(currCharacter)) {
                    charactersToRemove.Add(currCharacter);
                }
            }
            if (charactersToRemove.Contains(currentlyShowingParty.owner)) {
                //the party owner was removed from the party
                //remove all other characters instead
                charactersToRemove = new List<ICharacter>(currentlyShowingParty.icharacters);
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
            ICharacter currCharacter = partyHolder.characters[i];
            partyToUse.AddCharacter(currCharacter);
        }
        base.SetData(partyToUse);
        OnChangesMade();
        Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "Saved party!", true);
    }
    public void SetPartyHolderName(string name) {
        partyHolder.SetName(name);
    }
    private bool HasSameCharacters(Party party, PartyHolder other) {
        for (int i = 0; i < party.icharacters.Count; i++) {
            ICharacter partyCharacter = party.icharacters[i];
            if (!other.characters.Contains(partyCharacter)) {
                return false;
            }
        }
        for (int i = 0; i < other.characters.Count; i++) {
            ICharacter otherPartyCharacter = other.characters[i];
            if (!party.icharacters.Contains(otherPartyCharacter)) {
                return false;
            }
        }
        return true;
    }
    private void CopyValues(Party party) {
        partyHolder = new PartyHolder();
        partyHolder.SetName(party.partyName);
        for (int i = 0; i < party.icharacters.Count; i++) {
            partyHolder.AddCharacter(party.icharacters[i]);
        }
    }
    #endregion
}


public class PartyHolder {

    public string name { get; private set; }
    public List<ICharacter> characters { get; private set; }
    public EmblemBG emblemBG { get; private set; }
    public Sprite emblem { get; private set; }
    public Color partyColor { get; private set; }

    public PartyHolder() {
        name = string.Empty;
        characters = new List<ICharacter>();
        emblemBG = CharacterManager.Instance.GetRandomEmblemBG();
        emblem = CharacterManager.Instance.GetRandomEmblem();
        partyColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    public void SetName(string name) {
        this.name = name;
    }
    public void AddCharacter(ICharacter character) {
        if (!characters.Contains(character)) {
            characters.Add(character);
        }
    }
    public void RemoveCharacter(ICharacter character) {
        characters.Remove(character);
    }
}