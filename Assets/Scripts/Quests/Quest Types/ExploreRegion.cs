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
    public ExploreRegion(QuestCreator createdBy, int daysBeforeDeadline, Region regionToExplore) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.EXPLORE_REGION) {
        _questFilters = new List<QuestFilter>() {
            new MustBeRole(CHARACTER_ROLE.CHIEFTAIN)
        };
        _regionToExplore = regionToExplore;
    }

    private void EndQuestAfterDays() {
        ScheduleQuestEnd(30, QUEST_RESULT.SUCCESS);
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();
        //PickRegionToExplore(); //pick a region to explore

        GoToLocation goToRegionAction = new GoToLocation(this); //Go to the picked region
        goToRegionAction.InititalizeAction(_regionToExplore.centerOfMass);
        goToRegionAction.onQuestActionDone += this.PerformNextQuestAction;
        goToRegionAction.onQuestActionDone += EndQuestAfterDays; //After 30 days of exploring, the Quest ends in SUCCESS.
        goToRegionAction.onQuestDoAction += goToRegionAction.Generic;

        RoamRegion roamRegionAction = new RoamRegion(this);
        roamRegionAction.InititalizeAction(_regionToExplore);
        roamRegionAction.onQuestActionDone += RepeatCurrentAction;
        roamRegionAction.onQuestDoAction += roamRegionAction.ExploreRegion;

        //Enqueue all actions
        _questLine.Enqueue(goToRegionAction); 
        _questLine.Enqueue(roamRegionAction);
    }
    protected override void EndQuest(QUEST_RESULT result) {
        _currentAction.onQuestActionDone = null;
        //_currentAction.actionDoer.DestroyAvatar();
        base.EndQuest(result);
    }
    #endregion
}
