using UnityEngine;
using System.Collections;

public class HoverStability : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if(UIManager.Instance.currentlyShowingKingdom != null){
				Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
//				int stabilityPoints = 0;
//				int bonusStability = 0;
//				int totalStructures = 0;
				int stability = 0;
				for (int i = 0; i < kingdom.cities.Count; i++) {
					int stabilityIncrease = (kingdom.cities [i].stabilityPoints * 2) + kingdom.cities [i].bonusStability;
					int stabilityDecrease = (kingdom.cities [i].structures.Count * 3);
					stability += (stabilityIncrease - stabilityDecrease);
//					stabilityPoints += kingdom.cities [i].stabilityPoints;
//					bonusStability += kingdom.cities [i].bonusStability;
//					totalStructures += kingdom.cities [i].structures.Count;
				}
//				int stability = (stabilityPoints * 2) + bonusStability;
//				stability -= (totalStructures * 3);
				UIManager.Instance.ShowSmallInfo ("[b]MONTHLY STABILITY: " + stability + "[/b]");
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
