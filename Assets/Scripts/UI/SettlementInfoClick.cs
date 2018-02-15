using UnityEngine;
using System.Collections;

public class SettlementInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
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
					Quest quest = ((Settlement)UIManager.Instance.settlementInfoUI.currentlyShowingLandmark).GetQuestByID(idToUse);
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
}
