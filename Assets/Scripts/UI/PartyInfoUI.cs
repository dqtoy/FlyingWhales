using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PartyInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI partyNameLbl;
    [SerializeField] private ActionIcon actionIcon;
    [SerializeField] private GameObject questGO;
    [SerializeField] private TextMeshProUGUI questTitleLbl;
    [SerializeField] private TextMeshProUGUI questDescriptionLbl;
    [SerializeField] private ScrollRect membersScrollView;
    [SerializeField] private PartyCharacterItem[] characterItems;
    [SerializeField] private Color evenColor;
    [SerializeField] private Color oddColor;

    internal NewParty currentlyShowingParty {
        get { return _data as NewParty; }
    }

    internal override void Initialize() {
        base.Initialize();
        actionIcon.Initialize();
    }

    public override void OpenMenu() {
        base.OpenMenu();
        if (!isShowing) {
            UpdatePartyInfo();
        }
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
            UpdateQuest();
            UpdatePartyCharacters();
        }
    }
    private void UpdateGeneralInfo() {
        partyNameLbl.text = currentlyShowingParty.name;
        if (currentlyShowingParty is CharacterParty) {
            actionIcon.SetCharacter(currentlyShowingParty.owner as ECS.Character);
            actionIcon.SetAction((currentlyShowingParty as CharacterParty).actionData.currentAction);
            actionIcon.gameObject.SetActive(true);
        } else {
            actionIcon.gameObject.SetActive(false);
        }
    }
    private void UpdateQuest() {
        if (currentlyShowingParty is CharacterParty) {
            CharacterParty party = currentlyShowingParty as CharacterParty;
            if (party.actionData.questDataAssociatedWithCurrentAction != null) {
                Quest quest = party.actionData.questDataAssociatedWithCurrentAction.parentQuest;
                questTitleLbl.text = quest.name;
                questDescriptionLbl.text = quest.questDescription;
                questGO.SetActive(true);
            } else {
                questGO.SetActive(false);
            }
        } else {
            questGO.SetActive(false);
        }
    }
    private void UpdatePartyCharacters() {
        for (int i = 0; i < characterItems.Length; i++) {
            PartyCharacterItem item = characterItems[i];
            ICharacter character = currentlyShowingParty.icharacters.ElementAtOrDefault(i);
            if (character == null) {
                item.gameObject.SetActive(false);
            } else {
                if (Utilities.IsEven(i)) {
                    item.SetBGColor(evenColor);
                } else {
                    item.SetBGColor(oddColor);
                }
                item.SetCharacter(character);
                item.gameObject.SetActive(true);
            }
        }
        //for (int i = 0; i < currentlyShowingParty.icharacters.Count; i++) {
        //    ICharacter member = currentlyShowingParty.icharacters[i];
        //    if(member.characterPortrait.transform.parent != content.transform) {
        //        member.characterPortrait.transform.SetParent(content.transform);
        //    }
        //    member.characterPortrait.SetImageSize(IMAGE_SIZE.X36, false);
        //    member.characterPortrait.gameObject.SetActive(true);
        //    member.characterPortrait.ToggleNameLabel(true);
        //}
    }
}
