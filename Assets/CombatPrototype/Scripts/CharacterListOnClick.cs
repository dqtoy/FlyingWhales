using UnityEngine;
using System.Collections;

namespace ECS{
	public class CharacterListOnClick : MonoBehaviour {

		void OnClick(){
			UILabel lbl = GetComponent<UILabel> ();
			string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
			if (!string.IsNullOrEmpty (url)) {
				string id = url.Substring (0, url.IndexOf ('_'));
				int idToUse = int.Parse (id);
				if(url.Contains("_sideA")){
//					CombatPrototype.Instance.charactersSideA [idToUse];
				}else if(url.Contains("_sideB")){
//					CombatPrototype.Instance.charactersSideB [idToUse];
				}
			}
		}
	}
}

