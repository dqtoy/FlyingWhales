using UnityEngine;
using System.Collections;

public class HoverPrestige : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if(UIManager.Instance.currentlyShowingKingdom != null){
				Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
				//int monthlyPrestigeGain = kingdom.GetMonthlyPrestigeGain();
				//UIManager.Instance.ShowSmallInfo ("[b]MONTHLY PRESTIGE: " + monthlyPrestigeGain + "[/b]");
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
