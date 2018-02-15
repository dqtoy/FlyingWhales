using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel factionInfoLbl;
    [SerializeField] private UILabel relationshipsLbl;
    [SerializeField] private UIScrollView infoScrollView;
    [SerializeField] private UIScrollView relationshipsScrollView;

    internal Faction currentlyShowingFaction {
        get { return _data as Faction; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdateFactionInfo);
        tweenPos.AddOnFinished(() => UpdateFactionInfo());
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdateFactionInfo();
    }

    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateFactionInfo();
        }
    }

    public void UpdateFactionInfo() {
        if(currentlyShowingFaction == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]ID:[/b] " + currentlyShowingFaction.id.ToString();
        text += "\n[b]Name:[/b] " + currentlyShowingFaction.name;
		text += "\n[b]Race:[/b] " + currentlyShowingFaction.race.ToString();
		text += "\n[b]Faction Type:[/b] " + currentlyShowingFaction.factionType.ToString();
		text += "\n[b]Faction Size:[/b] " + currentlyShowingFaction.factionSize.ToString();
		text += "\n[b]Owned Landmarks: [/b] ";
        List<BaseLandmark> ownedLandmarks = currentlyShowingFaction.GetAllOwnedLandmarks();
        if (ownedLandmarks.Count > 0){
			for (int i = 0; i < ownedLandmarks.Count; i++) {
				BaseLandmark landmark = ownedLandmarks [i];
				text += "\n  - " + landmark.urlName + " (" + landmark.specificLandmarkType.ToString() + ")";
			}
		}else{
			text += "NONE";
		}

		text += "\n[b]Initial Technologies: [/b] ";
		if (currentlyShowingFaction.initialTechnologies.Count > 0) {
			text += "\n";
			for (int i = 0; i < currentlyShowingFaction.initialTechnologies.Count; i++) {
				TECHNOLOGY currTech = currentlyShowingFaction.initialTechnologies[i];
				text += currTech.ToString();
				if (i + 1 != currentlyShowingFaction.initialTechnologies.Count) {
					text += ", ";
				}
			}
		} else {
			text += "NONE";
		}

		text += "\n[b]Characters: [/b] ";
		if (currentlyShowingFaction.characters.Count > 0) {
			for (int i = 0; i < currentlyShowingFaction.characters.Count; i++) {
				ECS.Character currChar = currentlyShowingFaction.characters[i];
				text += "\n" + currChar.urlName  + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
				if (currChar.currentTask != null) {
                    if(currChar.currentTask.taskType == TASK_TYPE.QUEST) {
                        text += " (" + ((Quest)currChar.currentTask).urlName + ")";
                    } else {
                        text += " (" + currChar.currentTask.taskType.ToString() + ")";
                    }
					
				}
			}
		} else {
			text += "NONE";
		}

		text += "\n[b]Production Preferences: [/b] ";
		foreach (PRODUCTION_TYPE prodType in currentlyShowingFaction.productionPreferences.Keys) {
			text += "\n" + prodType.ToString ();
			for (int i = 0; i < currentlyShowingFaction.productionPreferences[prodType].prioritizedMaterials.Count; i++) {
				text += "\n - " + Utilities.NormalizeString(currentlyShowingFaction.productionPreferences[prodType].prioritizedMaterials[i].ToString ());
			}
		}
			
        factionInfoLbl.text = text;
        infoScrollView.UpdatePosition();

        //Relationships
        string relationshipText = string.Empty;
        relationshipText += "\n[b]Relationships:[/b] ";
        if (currentlyShowingFaction.relationships.Count > 0) {
            foreach (KeyValuePair<Faction, FactionRelationship> kvp in currentlyShowingFaction.relationships) {
                relationshipText += "\n " + kvp.Key.factionType.ToString() + " " + kvp.Key.urlName + ": " + kvp.Value.relationshipStatus.ToString();
            }
        } else {
            relationshipText += "NONE";
        }

        relationshipsLbl.text = relationshipText;
        relationshipsScrollView.UpdatePosition();
    }
//	public void OnClickCloseBtn(){
////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
//		HideMenu ();
//	}
}
