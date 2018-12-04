using UnityEngine;
using System.Collections;

public class ItemListOnClick : MonoBehaviour {

	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			int idToUse = int.Parse (url);
            //Debug.Log("Clicked " + url);
			//if(CombatPrototypeUI.Instance.currSelectedCharacter != null){
			//	CombatPrototypeUI.Instance.SetItemAsSelected(CombatPrototypeUI.Instance.currSelectedCharacter.equippedItems[idToUse]);
			//}
		}
	}
}