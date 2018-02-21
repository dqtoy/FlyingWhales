using UnityEngine;
using System.Collections;

public class SettlementInfoClick : MonoBehaviour {
	bool isHovering = false;
	UILabel lbl;

	void Start(){
		lbl = GetComponent<UILabel> ();
	}
	void Update(){
		if(isHovering){
			string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
			if (!string.IsNullOrEmpty (url)) {
				if(url == "civilians"){
					string hoverText = string.Empty;
					foreach (RACE race in UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.civiliansByRace.Keys) {
						if (UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.civiliansByRace[race] > 0){
							hoverText += "[b]" + race.ToString() + "[/b] - " + UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.civiliansByRace[race].ToString() + "\n";
						}
					}
					hoverText.TrimEnd ('\n');
					UIManager.Instance.ShowSmallInfo (hoverText);
					return;
				}
//				else if(url == "itemchest"){
//					ItemChest itemChest = (ItemChest)UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.landmarkEncounterable;
//					if(itemChest.itemsInChest.Count > 0){
//						string hoverText = string.Empty;
//						for (int i = 0; i < itemChest.itemsInChest.Count; i++) {
//							hoverText += itemChest.itemsInChest[i].nameWithQuality + "\n";
//						}
//						hoverText.TrimEnd ('\n');
//						UIManager.Instance.ShowSmallInfo (hoverText);
//						return;
//					}
//				}
			}

			if(UIManager.Instance.smallInfoGO.activeSelf){
				UIManager.Instance.HideSmallInfo ();
			}
		}
	}
	void OnClick(){
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			if(!url.Contains("_")){
				return;
			}
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_faction")){
				Faction faction = UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.owner;
				if(faction != null){
					UIManager.Instance.ShowFactionInfo (faction);
				}
			} else if(url.Contains("_character")){
				BaseLandmark landmark = UIManager.Instance.settlementInfoUI.currentlyShowingLandmark;
				ECS.Character character = landmark.GetCharacterAtLocationByID(idToUse);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}else{
					character = landmark.location.GetCharacterAtLocationByID(idToUse);
					if(character != null){
						UIManager.Instance.ShowCharacterInfo(character);
					}
				}
			} else if(url.Contains("_hextile")){
				if(UIManager.Instance.settlementInfoUI.currentlyShowingLandmark != null && UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.location.id == idToUse){
					UIManager.Instance.ShowHexTileInfo (UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.location);
				}
            } else if (url.Contains("_quest")) {
				if(UIManager.Instance.settlementInfoUI.currentlyShowingLandmark is Settlement){
					OldQuest.Quest quest = ((Settlement)UIManager.Instance.settlementInfoUI.currentlyShowingLandmark).GetQuestByID(idToUse);
					if (quest != null) {
						UIManager.Instance.ShowQuestInfo(quest);
					}	
				}
			} else if (url.Contains("_party")) {
				Party party = UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.GetPartyAtLocationByLeaderID(idToUse);
				if (party != null) {
					UIManager.Instance.ShowPartyInfo (party);
				} else {
					party = UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.location.GetPartyAtLocationByLeaderID(idToUse);
					if (party != null) {
						UIManager.Instance.ShowPartyInfo (party);
					}
				}
			} else if(url.Contains("_prisoner")){
				BaseLandmark landmark = UIManager.Instance.settlementInfoUI.currentlyShowingLandmark;
				ECS.Character character = landmark.GetPrisonerByID(idToUse);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}
			} 
        }
	}

	void OnHover(bool isOver){
		isHovering = isOver;
		if(!isOver){
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
