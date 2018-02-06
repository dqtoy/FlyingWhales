using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class BuildStructure : Quest {

    private BaseLandmark _target;
    private int _civilians;

    #region getters/setters
    public BaseLandmark target {
        get { return _target; }
    }
    #endregion

    public BuildStructure(TaskCreator createdBy, BaseLandmark target) : base(createdBy, QUEST_TYPE.BUILD_STRUCTURE) {
        _target = target;
        _questFilters = new List<QuestFilter>() {
            new MustBeFaction((createdBy as InternalQuestManager).owner)
        };
    }

    #region overrides
    public override void OnQuestPosted() {
        base.OnQuestPosted();
        //reserve 5 civilians
        _postedAt.AdjustReservedPopulation(5);
        _postedAt.AdjustPopulation(-5);
    }
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        Collect collect = new Collect(this);
        collect.InititalizeAction(5);
        collect.onTaskActionDone += this.PerformNextQuestAction;
        collect.onTaskDoAction += collect.BuildStructure;

        GoToLocation goToLandmark = new GoToLocation(this); //Go to the picked region
        goToLandmark.InititalizeAction(_target);
        goToLandmark.onTaskDoAction += goToLandmark.Generic;
        goToLandmark.onTaskActionDone += WaitForDays;

        _questLine.Enqueue(collect);
        _questLine.Enqueue(goToLandmark);
    }
    #endregion

    private void WaitForDays() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(5);
        SchedulingManager.Instance.AddEntry(dueDate, () => OccupyTarget());
    }

    private void OccupyTarget() {
        //Build a new structure on that tile
        _target.OccupyLandmark((createdBy as InternalQuestManager).owner);
        _target.AdjustPopulation(5);
        _assignedParty.AdjustCivilians(-5);
        GoBackToQuestGiver(TASK_STATUS.SUCCESS);
    }
}
