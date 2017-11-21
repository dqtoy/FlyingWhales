using UnityEngine;
using System.Collections;

public class HoverStability : MonoBehaviour {

    void OnHover(bool isOver) {
        if (isOver) {
            if (UIManager.Instance.currentlyShowingKingdom != null) {
                Kingdom kingdom = UIManager.Instance.currentlyShowingKingdom;
                int stability = kingdom.GetMonthlyStabilityGain();

                string stabilitySummary = string.Empty;
                stabilitySummary += "\n From King : " + kingdom.king.GetStabilityContribution().ToString();
                //int stabilityFromSupporting = 0;
                //int stabilityFromDissenting = 0;
                //int stabilityFromResources = 0;
                //int weaponsContribution = 0;
                //int armorContribution = 0;
                //for (int i = 0; i < kingdom.cities.Count; i++) {
                //    kingdom.cities[i].MonthlyResourceBenefits(ref weaponsContribution, ref armorContribution, ref stabilityFromResources);
                //    Citizen governor = kingdom.cities[i].governor;
                //    if(governor.loyaltyToKing >= 0) {
                //        stabilityFromSupporting += governor.GetStabilityContribution();
                //    } else {
                //        stabilityFromDissenting += governor.GetStabilityContribution();
                //    }
                //}
                //stabilitySummary += "\n From Supporting Governors : " + stabilityFromSupporting.ToString();
                //stabilitySummary += "\n From Dissenting Governors : " + stabilityFromDissenting.ToString();
                //stabilitySummary += "\n From Resources : " + stabilityFromResources.ToString();

                //int overpopulation = kingdom.GetOverpopulationPercentage();
                //stabilitySummary += "\n From Overpopulation : " + (overpopulation / 10) * -1;

                //stabilitySummary += "\n From Recent Conquests : " + (kingdom.stabilityDecreaseFromInvasionCounter * -2).ToString();

                //Stability has a -5 monthly reduction when the Kingdom is Medium and a -10 monthly reduction when the Kingdom is Large
                if (kingdom.kingdomSize == KINGDOM_SIZE.MEDIUM) {
                    stabilitySummary += "\n From Kingdom Size : -1";
                } else if (kingdom.kingdomSize == KINGDOM_SIZE.LARGE) {
                    stabilitySummary += "\n From Kingdom Size : -2";
                }

                //string stabilityFromInvasionSummary = string.Empty;
                //if(kingdom.datesStabilityDecreaseWillExpire.Count > 0) {
                //    stabilityFromInvasionSummary += "\n Expiry Dates: ";
                //    for (int i = 0; i < kingdom.datesStabilityDecreaseWillExpire.Count; i++) {
                //        GameDate expiryDate = kingdom.datesStabilityDecreaseWillExpire[i];
                //        stabilityFromInvasionSummary += "\n" + ((MONTH)expiryDate.month).ToString() + " " + expiryDate.day.ToString() + ", " + expiryDate.year.ToString();
                //    }
                //}

                UIManager.Instance.ShowSmallInfo("[b]Monthly Stability: " + stability + stabilitySummary + "[/b]");
            }

        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
