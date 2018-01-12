using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploreRegion : Quest {

    private Region _regionToExplore;

    public ExploreRegion(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers) 
        : base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.EXPLORE_REGION) {
        _questFilters = new List<QuestFilter>() {
            new MustBeRole(CHARACTER_ROLE.CHIEFTAIN)
        };
    }

    private void PickRegionToExplore() {
        List<Region> elligibleRegions = new List<Region>();
        Faction source = (Faction)_createdBy;
        for (int i = 0; i < source.settlements.Count; i++) {
            Region currRegion = source.settlements[i].location.region;
            for (int j = 0; j < currRegion.adjacentRegionsViaMajorRoad.Count; j++) {
                Region adjacentRegion = currRegion.adjacentRegionsViaMajorRoad[j];
                if (adjacentRegion.occupant == null) {
                    if (!elligibleRegions.Contains(currRegion)) {
                        elligibleRegions.Add(currRegion);
                    }
                }
            }
        }
        if (elligibleRegions.Count <= 0) {
            throw new System.Exception("Could not find elligible region to explore!");
        }
        _regionToExplore = elligibleRegions[Random.Range(0, elligibleRegions.Count)];
    }

    private void EndQuestAfterDays() {
        ScheduleQuestEnd(30, QUEST_RESULT.SUCCESS);
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();
        PickRegionToExplore(); //pick a region to explore

        GoToRegion goToRegionAction = new GoToRegion(); //Go to the picked region
        goToRegionAction.InititalizeAction(_regionToExplore);
        goToRegionAction.onQuestActionDone += this.PerformNextQuestAction;
        goToRegionAction.onQuestActionDone += EndQuestAfterDays; //After 30 days of exploring, the Quest ends in SUCCESS.

        RoamRegion roamRegionAction = new RoamRegion();
        roamRegionAction.InititalizeAction(_regionToExplore);

        //Enqueue all actions
        _questLine.Enqueue(goToRegionAction); 
        _questLine.Enqueue(roamRegionAction);
    }
    #endregion
}
