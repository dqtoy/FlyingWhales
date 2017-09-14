using UnityEngine;
using System.Collections;

public class HoverPrestige : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if(UIManager.Instance.currentlyShowingKingdom != null){
				Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
				int prestige = 10 + (2 * kingdom.cities.Count) + kingdom.bonusPrestige;
				UIManager.Instance.ShowSmallInfo ("[b]MONTHLY PRESTIGE: " + prestige + "[/b]");
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
