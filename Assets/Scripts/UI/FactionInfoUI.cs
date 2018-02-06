using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionInfoUI : UIMenu {

    internal bool isShowing;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel factionInfoLbl;
    [SerializeField] private UILabel relationshipsLbl;
    [SerializeField] private UIScrollView infoScrollView;
    [SerializeField] private UIScrollView relationshipsScrollView;

    internal Faction currentlyShowingFaction;

    internal override void Initialize() {
        Messenger.AddListener("UpdateUI", UpdateFactionInfo);
        tweenPos.AddOnFinished(() => UpdateFactionInfo());
    }

    public void ShowFactionInfo() {
        isShowing = true;
//        tweenPos.PlayForward();
		this.gameObject.SetActive (true);
    }
    public void HideFactionInfo() {
        isShowing = false;
//        tweenPos.PlayReverse();
		this.gameObject.SetActive (false);
    }

    public void SetFactionAsActive(Faction faction) {
        currentlyShowingFaction = faction;
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
		if(currentlyShowingFaction.ownedLandmarks.Count > 0){
			for (int i = 0; i < currentlyShowingFaction.ownedLandmarks.Count; i++) {
				BaseLandmark landmark = currentlyShowingFaction.ownedLandmarks [i];
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
	public void OnClickCloseBtn(){
//		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
		HideFactionInfo ();
	}
}
