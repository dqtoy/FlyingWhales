using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterInfoUI : UIMenu {

    internal bool isShowing;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel characterInfoLbl;

    private ECS.Character currentlyShowingCharacter;

    internal override void Initialize() {
        Messenger.AddListener("UpdateUI", UpdateCharacterInfo);
        tweenPos.AddOnFinished(() => UpdateCharacterInfo());
    }

    public void ShowCharacterInfo() {
        isShowing = true;
        tweenPos.PlayForward();
    }
    public void HideCharacterInfo() {
        isShowing = false;
        tweenPos.PlayReverse();
    }

	public void SetCharacterAsActive(ECS.Character character) {
        currentlyShowingCharacter = character;
        if (isShowing) {
            UpdateCharacterInfo();
        }
    }

    public void UpdateCharacterInfo() {
        if(currentlyShowingCharacter == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]Name:[/b] " + currentlyShowingCharacter.name;
		text += "\n[b]Gender:[/b] " + currentlyShowingCharacter.gender.ToString();
		text += "\n[b]Race:[/b] " + currentlyShowingCharacter.raceSetting.race.ToString ();
		text += "\n[b]Faction:[/b] " + "[url=" + currentlyShowingCharacter.faction.id + "_faction]" + currentlyShowingCharacter.faction.name + "[/url]";
		text += "\n[b]Class:[/b] " + currentlyShowingCharacter.characterClass.className;
		text += "\n[b]Role:[/b] " + currentlyShowingCharacter.role.roleType.ToString();

		text += "\n[b]Current Quest:[/b] ";
		if(currentlyShowingCharacter.currentQuest != null){
			text += currentlyShowingCharacter.currentQuest.questType.ToString();
		}else{
			text += "NONE";
		}

		text += "\n[b]Home:[/b] " + currentlyShowingCharacter.home.location.tileName;

		//Stats
		text += "\n[b]HP:[/b] " + currentlyShowingCharacter.currentHP.ToString() + "/" + currentlyShowingCharacter.maxHP.ToString() + " (" + currentlyShowingCharacter.baseMaxHP.ToString() + ")";
		text += "\n[b]Strength:[/b] " + currentlyShowingCharacter.strength.ToString() + " (" + currentlyShowingCharacter.baseStrength.ToString() + ")";
		text += "\n[b]Intelligence:[/b] " + currentlyShowingCharacter.intelligence.ToString() + " (" + currentlyShowingCharacter.baseIntelligence.ToString() + ")";
		text += "\n[b]Agility:[/b] " + currentlyShowingCharacter.agility.ToString() + " (" + currentlyShowingCharacter.baseAgility.ToString() + ")";

		text += "\n[b]Traits:[/b] ";
		if(currentlyShowingCharacter.traits.Count > 0){
			for (int i = 0; i < currentlyShowingCharacter.traits.Count; i++) {
				Trait trait = currentlyShowingCharacter.traits [i];
				text += "\n  - " + trait.trait.ToString();
			}
		}else{
			text += "NONE";
		}

		text += "\n[b]Skills:[/b] ";
		if(currentlyShowingCharacter.skills.Count > 0){
			for (int i = 0; i < currentlyShowingCharacter.skills.Count; i++) {
				ECS.Skill skill = currentlyShowingCharacter.skills [i];
				text += "\n  - " + skill.skillName;
			}
		}else{
			text += "NONE";
		}

		text += "\n[b]Inventory:[/b] ";
		if(currentlyShowingCharacter.inventory.Count > 0){
			for (int i = 0; i < currentlyShowingCharacter.inventory.Count; i++) {
				ECS.Item item = currentlyShowingCharacter.inventory [i];
				text += "\n  - " + item.itemName;
			}
		}else{
			text += "NONE";
		}

		text += "\n[b]Equipped Items:[/b] ";
		if(currentlyShowingCharacter.equippedItems.Count > 0){
			for (int i = 0; i < currentlyShowingCharacter.equippedItems.Count; i++) {
				ECS.Item item = currentlyShowingCharacter.equippedItems [i];
				text += "\n  - " + item.itemName + " (" + item.itemType.ToString() + ")";
			}
		}else{
			text += "NONE";
		}
        characterInfoLbl.text = text;
    }
}
