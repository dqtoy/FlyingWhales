using UnityEngine;
using System.Collections;

public class HoverStability : MonoBehaviour {

    void OnHover(bool isOver) {
        if (isOver) {
            if (UIManager.Instance.currentlyShowingKingdom != null) {
                Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
                int stability = kingdom.GetMonthlyStabilityGain();
                UIManager.Instance.ShowSmallInfo("[b]Monthly Stability: " + stability + "[/b]");
            }

        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
