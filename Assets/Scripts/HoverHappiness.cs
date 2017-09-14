using UnityEngine;
using System.Collections;

public class HoverHappiness : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if(UIManager.Instance.currentlyShowingKingdom != null){
				Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
//				int happinessPoints = 0;
//				int bonusHappiness = 0;
//				int totalStructures = 0;
				int happiness = 0;
				for (int i = 0; i < kingdom.cities.Count; i++) {
					int happinessIncrease = (kingdom.cities [i].happinessPoints * 2) + kingdom.cities [i].bonusHappiness;
					int happinessDecrease = (kingdom.cities [i].structures.Count * 3);
					happiness += (happinessIncrease - happinessDecrease);
//					happinessPoints += kingdom.cities [i].happinessPoints;
//					bonusHappiness += kingdom.cities [i].bonusHappiness;
//					totalStructures += kingdom.cities [i].structures.Count;
				}
//				int happiness = (happinessPoints * 2) + bonusHappiness;
//				happiness -= (totalStructures * 3);
				UIManager.Instance.ShowSmallInfo ("[b]MONTHLY HAPPINESS: " + happiness + "[/b]");
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
