using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel generalInfoLbl;
	[SerializeField] private UILabel statInfoLbl;
	[SerializeField] private UILabel traitInfoLbl;
	[SerializeField] private UILabel followersLbl;
	[SerializeField] private UILabel equipmentInfoLbl;
	[SerializeField] private UILabel inventoryInfoLbl;
	[SerializeField] private UILabel relationshipsLbl;
	[SerializeField] private UILabel historyLbl;

	[SerializeField] private UIScrollView followersScrollView;
    [SerializeField] private UIScrollView equipmentScrollView;
	[SerializeField] private UIScrollView inventoryScrollView;
    [SerializeField] private UIScrollView relationshipsScrollView;
	[SerializeField] private UIScrollView historyScrollView;

	private ECS.Character _activeCharacter;

    internal ECS.Character currentlyShowingCharacter {
        get { return _data as ECS.Character; }
    }

	internal ECS.Character activeCharacter{
		get { return _activeCharacter; }
	}

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdateCharacterInfo);
    }

	public override void ShowMenu (){
		base.ShowMenu ();
		_activeCharacter = (ECS.Character)_data;
	}
    public override void OpenMenu() {
        base.OpenMenu();
        UpdateCharacterInfo();
    }

    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateCharacterInfo();
        }
    }

	public void UpdateCharacterInfo(){
		if(currentlyShowingCharacter == null) {
			return;
		}
		UpdateGeneralInfo();
		UpdateStatInfo ();
		UpdateTraitInfo	();
		UpdateFollowersInfo ();
		UpdateEquipmentInfo ();
		UpdateInventoryInfo ();
		UpdateRelationshipInfo ();
		UpdateHistoryInfo ();
	}
    public void UpdateGeneralInfo() {
        string text = string.Empty;
        text += currentlyShowingCharacter.id;
        text += "\n" + currentlyShowingCharacter.name;
		text += "\n" + Utilities.GetNormalizedSingularRace (currentlyShowingCharacter.raceSetting.race) + " " + Utilities.NormalizeString (currentlyShowingCharacter.gender.ToString ());
		if(currentlyShowingCharacter.characterClass != null && currentlyShowingCharacter.characterClass.className != "Classless"){
			text += " " + currentlyShowingCharacter.characterClass.className;
		}
		if(currentlyShowingCharacter.role != null){
			text += " (" + currentlyShowingCharacter.role.roleType.ToString() + ")";
		}

		text += "\nFaction: " + (currentlyShowingCharacter.faction != null ? currentlyShowingCharacter.faction.urlName : "NONE");
        text += ",    Village: ";
        if (currentlyShowingCharacter.home != null) {
            text += currentlyShowingCharacter.home.urlName;
        } else {
            text += "NONE";
        }
        text += "\nCurrent Action: ";
        if (currentlyShowingCharacter.currentTask != null) {
            text += currentlyShowingCharacter.currentTask.taskType.ToString();
        } else {
            text += "NONE";
        }
        text += "\nCurrent Quest: ";
        if (currentlyShowingCharacter.currentQuest != null) {
            text += currentlyShowingCharacter.currentQuest.questName.ToString() + "(" + currentlyShowingCharacter.currentQuestPhase.phaseName + ")";
        } else {
            text += "NONE";
        }
        text += "\nGold: " +  currentlyShowingCharacter.gold.ToString();
        text += ",    Prestige: " + currentlyShowingCharacter.prestige.ToString();
		text += "\nParty: " + (currentlyShowingCharacter.party != null ? currentlyShowingCharacter.party.urlName : "NONE");
		text += "\nCivilians: " + "[url=civilians]" + currentlyShowingCharacter.civilians.ToString () + "[/url]";
//        foreach (KeyValuePair<RACE, int> kvp in currentlyShowingCharacter.civiliansByRace) {
//            if (kvp.Value > 0) {
//                text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
//            }
//        }
        //		text += "\n[b]Skills:[/b] ";
        //		if(currentlyShowingCharacter.skills.Count > 0){
        //			for (int i = 0; i < currentlyShowingCharacter.skills.Count; i++) {
        //				ECS.Skill skill = currentlyShowingCharacter.skills [i];
        //				text += "\n  - " + skill.skillName;
        //			}
        //		}else{
        //			text += "NONE";
        //		}

        generalInfoLbl.text = text;
//        infoScrollView.ResetPosition();

    }

	private void UpdateStatInfo(){
		string text = string.Empty;
		text += "[b]STATS[/b]";
		text += "\nHP: " + currentlyShowingCharacter.currentHP.ToString() + "/" + currentlyShowingCharacter.maxHP.ToString();
		text += "\nStr: " + currentlyShowingCharacter.strength.ToString();
		text += "\nInt: " + currentlyShowingCharacter.intelligence.ToString();
		text += "\nAgi: " + currentlyShowingCharacter.agility.ToString();
		statInfoLbl.text = text;
	}
	private void UpdateTraitInfo(){
		string text = string.Empty;
		text += "[b]TRAITS AND TAGS[/b]";
		if(currentlyShowingCharacter.traits.Count > 0){
			text += "\n";
			for (int i = 0; i < currentlyShowingCharacter.traits.Count; i++) {
				Trait trait = currentlyShowingCharacter.traits [i];
				if(i > 0){
					text += ", ";
				}
				text += Utilities.NormalizeString(trait.trait.ToString());
			}
			if(currentlyShowingCharacter.traits.Count > 0){
				text += ", ";
			}
			for (int i = 0; i < currentlyShowingCharacter.tags.Count; i++) {
				CharacterTag tag = currentlyShowingCharacter.tags [i];
				if(i > 0){
					text += ", ";
				}
				text += tag.tagName;
			}
		}else{
			text += "\nNONE";
		}
		traitInfoLbl.text = text;
	}
	private void UpdateFollowersInfo(){
		string text = string.Empty;
		if(currentlyShowingCharacter.party != null && currentlyShowingCharacter.party.partyLeader.id == currentlyShowingCharacter.id){
			for (int i = 0; i < currentlyShowingCharacter.party.followers.Count; i++) {
				ECS.Character follower = currentlyShowingCharacter.party.followers [i];
				if(i > 0){
					text += "\n";
				}
				text += follower.urlName;
			}
		}else{
			text += "NONE";
		}
		followersLbl.text = text;
		followersScrollView.UpdatePosition ();
	}

	private void UpdateEquipmentInfo(){
		string text = string.Empty;
		if(currentlyShowingCharacter.equippedItems.Count > 0){
			for (int i = 0; i < currentlyShowingCharacter.equippedItems.Count; i++) {
				ECS.Item item = currentlyShowingCharacter.equippedItems [i];
				if(i > 0){
					text += "\n";
				}
				text += item.itemName;
				if(item is ECS.Weapon){
					ECS.Weapon weapon = (ECS.Weapon)item;
					if(weapon.bodyPartsAttached.Count > 0){
						text += " (";
						for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
							if(j > 0){
								text += ", ";
							}
							text += weapon.bodyPartsAttached [j].name;
						}
						text += ")";
					}
				}else if(item is ECS.Armor){
					ECS.Armor armor = (ECS.Armor)item;
					text += " (" + armor.bodyPartAttached.name + ")";
				}
			}
		}else{
			text += "NONE";
		}
		equipmentInfoLbl.text = text;
		equipmentScrollView.UpdatePosition ();
	}

	private void UpdateInventoryInfo(){
		string text = string.Empty;
		if(currentlyShowingCharacter.inventory.Count > 0) {
			for (int i = 0; i < currentlyShowingCharacter.inventory.Count; i++) {
				ECS.Item item = currentlyShowingCharacter.inventory [i];
				if(i > 0){
					text += "\n";
				}
				text += item.itemName;
			}
		}else{
			text += "NONE";
		}
		inventoryInfoLbl.text = text;
		inventoryScrollView.UpdatePosition ();
	}

	private void UpdateRelationshipInfo(){
		string text = string.Empty;
		if (currentlyShowingCharacter.relationships.Count > 0) {
			bool isFirst = true;
			foreach (KeyValuePair<ECS.Character, Relationship> kvp in currentlyShowingCharacter.relationships) {
				if(!isFirst){
					text += "\n";
				}else{
					isFirst = false;
				}
				text += kvp.Key.role.roleType.ToString() + " " + kvp.Key.urlName + ": " + kvp.Value.totalValue.ToString();
			}
		} else {
			text += "NONE";
		}

		relationshipsLbl.text = text;
		relationshipsScrollView.UpdatePosition();
	}
	private void UpdateHistoryInfo(){
		string text = string.Empty;
		if (currentlyShowingCharacter.history.Count > 0) {
			for (int i = 0; i < currentlyShowingCharacter.history.Count; i++) {
				if(i > 0){
					text += "\n";
				}
				text += currentlyShowingCharacter.history[i];
			}
		} else {
			text += "NONE";
		}

		historyLbl.text = text;
		historyScrollView.UpdatePosition();
	}
	public void CenterCameraOnCharacter() {
        CameraMove.Instance.CenterCameraOn(currentlyShowingCharacter.currLocation.gameObject);
    }

	#region Overrides
	public override void HideMenu (){
		_activeCharacter = null;
		base.HideMenu ();
	}
	#endregion
//	public void OnClickCloseBtn(){
////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
//		HideMenu ();
//	}
}
