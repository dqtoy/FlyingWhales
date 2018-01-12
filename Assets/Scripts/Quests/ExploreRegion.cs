using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploreRegion : Quest {

    private Region _regionToExplore;

    #region getters/setters
    public Region regionToExplore {
        get { return _regionToExplore; }
    }
    #endregion
    public ExploreRegion(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers, Region regionToExplore) 
        : base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.EXPLORE_REGION) {
        _questFilters = new List<QuestFilter>() {
            new MustBeRole(CHARACTER_ROLE.CHIEFTAIN)
        };
        _regionToExplore = regionToExplore;
    }

    private void PickRegionToExplore() {
        List<Region> elligibleRegions = new List<Region>();
        Faction source = ((InternalQuestManager)_createdBy).owner;
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
        //PickRegionToExplore(); //pick a region to explore

        GoToLocation goToRegionAction = new GoToLocation(); //Go to the picked region
        goToRegionAction.InititalizeAction(_regionToExplore.centerOfMass);
        goToRegionAction.onQuestActionDone += this.PerformNextQuestAction;
        goToRegionAction.onQuestActionDone += EndQuestAfterDays; //After 30 days of exploring, the Quest ends in SUCCESS.

        RoamRegion roamRegionAction = new RoamRegion();
        roamRegionAction.InititalizeAction(_regionToExplore);

        //Enqueue all actions
        _questLine.Enqueue(goToRegionAction); 
        _questLine.Enqueue(roamRegionAction);
    }
    protected override void EndQuest(QUEST_RESULT result) {
        base.EndQuest(result);
    }
    #endregion
}
