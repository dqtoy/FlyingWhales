using UnityEngine;
using System.Collections;

public class HoverStability : MonoBehaviour {

    void OnHover(bool isOver) {
        if (isOver) {
            if (UIManager.Instance.currentlyShowingKingdom != null) {
                Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
                int stability = kingdom.GetMonthlyStabilityGain();
                string stabilityFromInvasionSummary = string.Empty;
                if(kingdom.datesStabilityDecreaseWillExpire.Count > 0) {
                    stabilityFromInvasionSummary += "\n Expiry Dates: ";
                    for (int i = 0; i < kingdom.datesStabilityDecreaseWillExpire.Count; i++) {
                        GameDate expiryDate = kingdom.datesStabilityDecreaseWillExpire[i];
                        stabilityFromInvasionSummary += "\n" + ((MONTH)expiryDate.month).ToString() + " " + expiryDate.day.ToString() + ", " + expiryDate.year.ToString();
                    }
                }
                
                UIManager.Instance.ShowSmallInfo("[b]Monthly Stability: " + stability + stabilityFromInvasionSummary + "[/b]");
            }

        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
