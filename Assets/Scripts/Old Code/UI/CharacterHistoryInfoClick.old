using UnityEngine;
using System.Collections;

public class CharacterHistoryInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_combat")){
				if(UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null){
					if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.combatHistory.ContainsKey (idToUse)){
						UIManager.Instance.ShowCombatLog (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.combatHistory[idToUse]);
					}
				}
			}
        }
	}
}
