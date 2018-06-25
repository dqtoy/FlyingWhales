using UnityEngine;
using System.Collections;

public class FactionInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_character")){
				ECS.Character character = UIManager.Instance.factionInfoUI.currentlyShowingFaction.GetCharacterByID(idToUse);
				if (character != null) {
					UIManager.Instance.ShowCharacterInfo(character);
				}
			}else if(url.Contains("_landmark")){
				BaseLandmark landmark = LandmarkManager.Instance.GetLandmarkByID(idToUse);
				if(landmark != null){
					UIManager.Instance.ShowLandmarkInfo(landmark);
				}
			} else if (url.Contains("_faction")) {
                Faction faction = FactionManager.Instance.GetFactionBasedOnID(idToUse);
                if (faction != null) {
                    UIManager.Instance.ShowFactionInfo(faction);
                }
            }
            //else if (url.Contains("_quest")) {
            //    OldQuest.Quest quest = FactionManager.Instance.GetQuestByID(idToUse);
            //    if (quest != null) {
            //        UIManager.Instance.ShowQuestInfo(quest);
            //    }
            //}
        }
	}
}
