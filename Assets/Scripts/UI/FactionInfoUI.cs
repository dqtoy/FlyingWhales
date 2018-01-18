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
        text += "[b]Name:[/b] " + currentlyShowingFaction.name;
		text += "\n[b]Race:[/b] " + currentlyShowingFaction.race.ToString();
		text += "\n[b]Faction Type:[/b] " + currentlyShowingFaction.factionType.ToString();
		text += "\n[b]Faction Size:[/b] " + currentlyShowingFaction.factionSize.ToString();
		text += "\n[b]Owned Landmarks: [/b] ";
		if(currentlyShowingFaction.ownedLandmarks.Count > 0){
			for (int i = 0; i < currentlyShowingFaction.ownedLandmarks.Count; i++) {
				BaseLandmark landmark = currentlyShowingFaction.ownedLandmarks [i];
				text += "[url=" + landmark.id + "_landmark]" + "\n  - " + landmark.location.tileName + " (" + landmark.specificLandmarkType.ToString() + ")" + "[/url]";
			}
		}else{
			text += "NONE";
		}

		text += "\n[b]Initial Technologies: [/b] ";
		if (currentlyShowingFaction.inititalTechnologies.Count > 0) {
			text += "\n";
			for (int i = 0; i < currentlyShowingFaction.inititalTechnologies.Count; i++) {
				TECHNOLOGY currTech = currentlyShowingFaction.inititalTechnologies[i];
				text += currTech.ToString();
				if (i + 1 != currentlyShowingFaction.inititalTechnologies.Count) {
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
				text += "\n" + "[url=" + currChar.id + "_character]" + currChar.name + "[/url]" + " - " + currChar.characterClass.className + "/" + currChar.role.roleType.ToString();
				if (currChar.currentQuest != null) {
					text += " (" + currChar.currentQuest.questType.ToString() + ")";
				}
			}
		} else {
			text += "NONE";
		}
			
        factionInfoLbl.text = text;

        //Relationships
        string relationshipText = string.Empty;
        relationshipText += "\n[b]Relationships:[/b] ";
        if (currentlyShowingFaction.relationships.Count > 0) {
            foreach (KeyValuePair<Faction, FactionRelationship> kvp in currentlyShowingFaction.relationships) {
                relationshipText += "\n "  + kvp.Key.factionType.ToString() + "[-] [url=" + kvp.Key.id + "_faction]" + kvp.Key.name + "[/url]: " + kvp.Value.relationshipStatus.ToString();
            }
        } else {
            relationshipText += "NONE";
        }

        relationshipsLbl.text = relationshipText;
    }
}
