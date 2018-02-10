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
    public ExploreRegion(TaskCreator createdBy, Region regionToExplore) : base(createdBy, QUEST_TYPE.EXPLORE_REGION) {
        _questFilters = new List<QuestFilter>() {
//            new MustBeRole(CHARACTER_ROLE.CHIEFTAIN)
        };
        _regionToExplore = regionToExplore;
    }

    private void EndQuestAfterDays() {
        ScheduleQuestEnd(30, TASK_STATUS.SUCCESS);
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();
        //PickRegionToExplore(); //pick a region to explore

        GoToLocation goToRegionAction = new GoToLocation(this); //Go to the picked region
        goToRegionAction.InititalizeAction(_regionToExplore.centerOfMass);
        goToRegionAction.onTaskActionDone += this.PerformNextQuestAction;
        goToRegionAction.onTaskActionDone += EndQuestAfterDays; //After 30 days of exploring, the Quest ends in SUCCESS.
        goToRegionAction.onTaskDoAction += goToRegionAction.Generic;

        RoamRegion roamRegionAction = new RoamRegion(this);
        roamRegionAction.InititalizeAction(_regionToExplore);
        roamRegionAction.onTaskActionDone += RepeatCurrentAction;
        roamRegionAction.onTaskDoAction += roamRegionAction.ExploreRegion;

        //Enqueue all actions
        _questLine.Enqueue(goToRegionAction); 
        _questLine.Enqueue(roamRegionAction);
    }
	protected override void EndQuest(TASK_STATUS result) {
        //_currentAction.actionDoer.DestroyAvatar();
        base.EndQuest(result);
    }
    #endregion
}
