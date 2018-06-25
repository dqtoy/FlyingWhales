using UnityEngine;
using System.Collections;

public class LandmarkHistoryInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_combat")){
				if(UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark != null){
					if (UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.combatHistory.ContainsKey (idToUse)){
						UIManager.Instance.ShowCombatLog (UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.combatHistory[idToUse]);
					}
				}
			}
        }
	}
}
