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

    internal Party currentlyShowingParty {
        get { return _data as Party; }
    }

    internal override void Initialize() {
        base.Initialize();
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            currSlot.SetNeededType(typeof(ICharacter));
        }
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdatePartyInfo();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdatePartyInfo();
        }
    }

    public void UpdatePartyInfo() {
        if(currentlyShowingParty == null) {
            return;
        }
		if(!currentlyShowingParty.isDead){
            UpdateGeneralInfo();
            UpdatePartyCharacters();
        }
    }
    private void UpdateGeneralInfo() {
        partyField.text = currentlyShowingParty.name;
    }
    private void UpdatePartyCharacters() {
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currItem = partySlots[i];
            ICharacter character = currentlyShowingParty.icharacters.ElementAtOrDefault(i);
            if (character == null) {
                currItem.ClearSlot();
            } else {
                currItem.PlaceObject(character);
            }
        }
    }

    public void ShowCreatePartyUI() {
        base.OpenMenu();
        for (int i = 0; i < partySlots.Length; i++) {
            SlotItem currSlot = partySlots[i];
            currSlot.ClearSlot();
        }
        partyField.text = string.Empty;
        UIManager.Instance.ShowMinionsMenu();
    }
}
