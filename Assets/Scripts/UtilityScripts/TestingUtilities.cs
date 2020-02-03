namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
<<<<<<< Updated upstream
            // if(region.settlement != null) {
            //     string summary = "Location Job Queue: ";
            //     if (region.settlement.availableJobs.Count > 0) {
            //         for (int i = 0; i < region.settlement.availableJobs.Count; i++) {
            //             JobQueueItem jqi = region.settlement.availableJobs[i];
            //             if (jqi is GoapPlanJob) {
            //                 GoapPlanJob gpj = jqi as GoapPlanJob;
            //                 summary += "\n" + gpj.name + " Targeting " + gpj.targetPOI?.name ?? "None";
            //             } else {
            //                 summary += "\n" + jqi.name;
            //             }
            //             summary += "\n\tAssigned Character: " + jqi.assignedCharacter?.name ?? "None";
            //             if (UIManager.Instance.characterInfoUI.isShowing) {
            //                 summary += "\n\tCan character take job? " + jqi
            //                                .CanCharacterDoJob(UIManager.Instance.characterInfoUI.activeCharacter)
            //                                .ToString();
            //             }
            //
            //         }
            //     } else {
            //         summary += "\nNone";
            //     }
            //     summary += "\nActive Quest: ";
            //     if (region.owner != null && region.owner.activeQuest != null) {
            //         summary += region.owner.activeQuest.name;
            //     } else {
            //         summary += "None";
            //     }
            //     UIManager.Instance.ShowSmallInfo(summary);
            // }
=======
            List<Settlement> settlements = GetSettlementsInRegion(region);
            string summary = "Locations Job Queue";
            for (int i = 0; i < settlements.Count; i++) {
                Settlement settlement = settlements[i];
                summary += $"\n<b>{settlement.name} Location Job Queue: </b>";
                if (settlement.availableJobs.Count > 0) {
                    for (int j = 0; j < settlement.availableJobs.Count; j++) {
                        JobQueueItem jqi = settlement.availableJobs[j];
                        if (jqi is GoapPlanJob) {
                            GoapPlanJob gpj = jqi as GoapPlanJob;
                            summary += "\n" + gpj.name + " Targeting " + gpj.targetPOI?.name ?? "None";
                        } else {
                            summary += "\n" + jqi.name;
                        }
                        summary += "\nAssigned Character: " + jqi.assignedCharacter?.name ?? "None";
                        if (UIManager.Instance.characterInfoUI.isShowing) {
                            summary += "\nCan character take job? " + jqi
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
                summary += "\n";
                UIManager.Instance.ShowSmallInfo(summary);
            }
>>>>>>> Stashed changes
        }
        public static void HideLocationInfo() {
            UIManager.Instance.HideSmallInfo();
        }
    }
}