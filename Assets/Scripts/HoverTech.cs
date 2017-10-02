using UnityEngine;
using System.Collections;

public class HoverTech : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if(UIManager.Instance.currentlyShowingKingdom != null){
				Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
                int tech = kingdom.GetMonthlyTechGain();
				UIManager.Instance.ShowSmallInfo ("[b]Monthly Tech: " + tech + "[/b]");
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
