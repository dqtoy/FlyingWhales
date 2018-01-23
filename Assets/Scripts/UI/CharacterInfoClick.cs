using UnityEngine;
using System.Collections;

public class CharacterInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_faction")){
				Faction faction = FactionManager.Instance.GetFactionBasedOnID (idToUse);
				if(faction != null){
					UIManager.Instance.ShowFactionInfo (faction);
				}
			} else if(url.Contains("_landmark")){
				if(UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home.id == idToUse){
					UIManager.Instance.ShowSettlementInfo (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home);
				}
			} else if (url.Contains("_character")) {
                ECS.Character character = FactionManager.Instance.GetCharacterByID(idToUse);
                if (character != null) {
                    UIManager.Instance.ShowCharacterInfo(character);
                }
            } else if (url.Contains("_quest")) {
                Quest quest = FactionManager.Instance.GetQuestByID(idToUse);
                if (quest != null) {
                    UIManager.Instance.ShowQuestInfo(quest);
                }
            }
        }
	}
}
