using UnityEngine;
using System.Collections;

public class PartyInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_character")){
				Party party = UIManager.Instance.partyinfoUI.currentlyShowingParty;
				ECS.Character character = party.GetCharacterByID(idToUse);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}
			} else if (url.Contains("_quest")) {
				if(UIManager.Instance.partyinfoUI.currentlyShowingParty.currentTask != null && UIManager.Instance.partyinfoUI.currentlyShowingParty.currentTask.taskType == TASK_TYPE.QUEST){
					Quest quest = (Quest)UIManager.Instance.partyinfoUI.currentlyShowingParty.currentTask;
					if (quest != null) {
						UIManager.Instance.ShowQuestInfo(quest);
					}
				}
			}  else if(url.Contains("_prisoner")){
				Party party = UIManager.Instance.partyinfoUI.currentlyShowingParty;
				ECS.Character character = party.GetPrisonerByID(idToUse);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}
			} 
        }
	}
}
