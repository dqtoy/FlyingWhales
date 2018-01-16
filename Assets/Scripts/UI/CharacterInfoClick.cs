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
			}
		}
	}
}
