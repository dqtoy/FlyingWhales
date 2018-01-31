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
				Quest quest = FactionManager.Instance.GetQuestByID(idToUse);
				if (quest != null) {
					UIManager.Instance.ShowQuestInfo(quest);
				}
			} 
        }
	}
}
