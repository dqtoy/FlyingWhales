using Boo.Lang;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            List<Settlement> settlements = GetSettlementsInRegion(region);
            for (int i = 0; i < settlements.Count; i++) {
                Settlement settlement = settlements[i];
                string summary = $"\t{settlement.name} Location Job Queue: ";
                if (settlement.availableJobs.Count > 0) {
                    for (int j = 0; j < settlement.availableJobs.Count; j++) {
                        JobQueueItem jqi = settlement.availableJobs[j];
                        if (jqi is GoapPlanJob) {
                            GoapPlanJob gpj = jqi as GoapPlanJob;
                            summary += "\n\t" + gpj.name + " Targeting " + gpj.targetPOI?.name ?? "None";
                        } else {
                            summary += "\n\t" + jqi.name;
                        }
                        summary += "\n\t\tAssigned Character: " + jqi.assignedCharacter?.name ?? "None";
                        if (UIManager.Instance.characterInfoUI.isShowing) {
                            summary += "\n\t\tCan character take job? " + jqi
                                           .CanCharacterDoJob(UIManager.Instance.characterInfoUI.activeCharacter)
                                           .ToString();
                        }
            
                    }
                } else {
                    summary += "\n\tNone";
                }
                summary += "\n\tActive Quest: ";
                if (settlement.owner != null && settlement.owner.activeQuest != null) {
                    summary += settlement.owner.activeQuest.name;
                } else {
                    summary += "None";
                }
                UIManager.Instance.ShowSmallInfo(summary);
            }
        }
        public static void HideLocationInfo() {
            UIManager.Instance.HideSmallInfo();
        }

        private static List<Settlement> GetSettlementsInRegion(Region region) {
            List<Settlement> settlements = new List<Settlement>();
            for (int i = 0; i < LandmarkManager.Instance.allSetttlements.Count; i++) {
                Settlement settlement = LandmarkManager.Instance.allSetttlements[i];
                if (settlement.region == region) {
                    settlements.Add(settlement);
                }
            }
            return settlements;
        }
    }
}