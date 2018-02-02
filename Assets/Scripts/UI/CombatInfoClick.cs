using UnityEngine;
using System.Collections;

public class CombatInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_character")){
				ECS.Character character = UIManager.Instance.combatLogUI.currentlyShowingCombat.GetAliveCharacterByID(idToUse);
				if (character != null) {
					UIManager.Instance.ShowCharacterInfo(character);
				}
			}
        }
	}
}
