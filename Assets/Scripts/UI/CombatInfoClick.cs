using UnityEngine;
using System.Collections;
using ECS;

public class CombatInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_character")){
				ICharacter ichar = UIManager.Instance.combatLogUI.currentlyShowingCombat.GetAliveCharacterByID(idToUse);
				if (ichar != null) {
                    if(ichar is Character) {
                        Character character = ichar as Character;
                        UIManager.Instance.ShowCharacterInfo(character);
                    } else {
                        //Show monster UI
                    }
				}
			}
        }
	}
}
