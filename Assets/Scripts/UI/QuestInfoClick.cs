using UnityEngine;
using System.Collections;

public class QuestInfoClick : MonoBehaviour {
    void OnClick() {
        UILabel lbl = GetComponent<UILabel>();
        string url = lbl.GetUrlAtPosition(UICamera.lastWorldPosition);
        if (!string.IsNullOrEmpty(url)) {
            string id = url.Substring(0, url.IndexOf('_'));
            int idToUse = int.Parse(id);
            //Debug.Log("Clicked " + url);
            if (url.Contains("_faction")) {
                Faction faction = FactionManager.Instance.GetFactionBasedOnID(idToUse);
                if (faction != null) {
                    UIManager.Instance.ShowFactionInfo(faction);
                }
            } else if (url.Contains("_landmark")) {
                if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home.id == idToUse) {
                    UIManager.Instance.ShowLandmarkInfo(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home);
                }
            } else if (url.Contains("_character")) {
                ECS.Character character = CharacterManager.Instance.GetCharacterByID(idToUse);
                if (character != null) {
                    UIManager.Instance.ShowCharacterInfo(character);
                }
            } else if (url.Contains("_quest")) {
                OldQuest.Quest quest = FactionManager.Instance.GetQuestByID(idToUse);
                if (quest != null) {
                    UIManager.Instance.ShowQuestInfo(quest);
                }
			} else if (url.Contains("_party")) {
				Party party = UIManager.Instance.questInfoUI.currentlyShowingQuest.assignedParty;
				if(party != null){
					UIManager.Instance.ShowCharacterInfo(party.partyLeader);
				}
			}
        }
    }
}
