using UnityEngine;
using System.Collections;

public class HoverTech : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if(UIManager.Instance.currentlyShowingKingdom != null){
				Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
				int tech = (int)((kingdom.cities.Count + kingdom.GetTechContributionFromCitizens()) * kingdom.techProductionPercentage);
				UIManager.Instance.ShowSmallInfo ("[b]TECH PER TICK: " + tech + "[/b]");
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
