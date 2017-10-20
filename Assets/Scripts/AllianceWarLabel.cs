using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AllianceWarLabel : MonoBehaviour {

	internal Dictionary<int, Kingdom> kingdomsInLabel = new Dictionary<int, Kingdom> ();
	internal Dictionary<int, City> citiesInLabel = new Dictionary<int, City> ();

	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			if(url.Contains("_city")){
				if(citiesInLabel.ContainsKey(idToUse)){
					City currCity = citiesInLabel[idToUse];
					CameraMove.Instance.CenterCameraOn(currCity.hexTile.gameObject);
				}
			}else if(url.Contains("_kingdom")){
				if(kingdomsInLabel.ContainsKey(idToUse)){
					Kingdom currKingdom = kingdomsInLabel[idToUse];
					UIManager.Instance.SetKingdomAsActive (currKingdom);
					CameraMove.Instance.CenterCameraOn(currKingdom.capitalCity.hexTile.gameObject);
				}
			}
		}
	}
}
