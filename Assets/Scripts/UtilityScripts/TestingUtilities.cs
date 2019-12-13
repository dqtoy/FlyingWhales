namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            if(region.area != null) {
                string summary = "Location Job Queue: ";
                if (region.area.availableJobs.Count > 0) {
                    for (int i = 0; i < region.area.availableJobs.Count; i++) {
                        JobQueueItem jqi = region.area.availableJobs[i];
                        if (jqi is GoapPlanJob) {
                            GoapPlanJob gpj = jqi as GoapPlanJob;
                            summary += "\n" + gpj.name + " Targeting " + gpj.targetPOI?.name ?? "None";
                        } else {
                            summary += "\n" + jqi.name;
                        }
                        summary += "\n\tAssigned Character: " + jqi.assignedCharacter?.name ?? "None";

                    }
                } else {
                    summary += "\nNone";
                }
                summary += "\nActive Quest: ";
                if (region.owner != null && region.owner.activeQuest != null) {
                    summary += region.owner.activeQuest.name;
                } else {
                    summary += "None";
                }
                UIManager.Instance.ShowSmallInfo(summary);
            }
        }
        public static void HideLocationInfo() {
            UIManager.Instance.HideSmallInfo();
        }
    }
}