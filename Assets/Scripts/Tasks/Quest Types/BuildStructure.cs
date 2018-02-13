using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class BuildStructure : Quest {

    private HexTile _target;
    private int _civilians;
    private MATERIAL _materialToUse;
    private Construction _constructionData;

    #region getters/setters
    public HexTile target {
        get { return _target; }
    }
    #endregion

    public BuildStructure(TaskCreator createdBy, HexTile target, MATERIAL materialToUse, Construction constructionData) : base(createdBy, QUEST_TYPE.BUILD_STRUCTURE) {
        _target = target;
        _materialToUse = materialToUse;
        _constructionData = constructionData;
        _questFilters = new List<QuestFilter>() {
            new MustBeFaction((createdBy as InternalQuestManager).owner)
        };
    }

    #region overrides
    public override void OnQuestPosted() {
        base.OnQuestPosted();
        _postedAt.ReduceAssets(_constructionData.production, _materialToUse); //reduce the assets of the settlement that posted this quest. TODO: Return resources when quest is cancelled or failed?
        ////reserve 5 civilians
        //_postedAt.AdjustReservedPopulation(5);
        //_postedAt.AdjustPopulation(-5);
    }
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        Collect collect = new Collect(this);
        collect.InititalizeAction(_constructionData.production.civilianCost);
        collect.onTaskActionDone += this.PerformNextQuestAction;
        collect.onTaskDoAction += collect.BuildStructure;

        GoToLocation goToLandmark = new GoToLocation(this); //Go to the target tile
        goToLandmark.InititalizeAction(_target);
        goToLandmark.SetPathfindingMode(PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP);
        goToLandmark.onTaskDoAction += goToLandmark.Generic;
        goToLandmark.onTaskActionDone += WaitForDays;

        _questLine.Enqueue(collect);
        _questLine.Enqueue(goToLandmark);
    }
    #endregion

    private void WaitForDays() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(_constructionData.production.duration);
        SchedulingManager.Instance.AddEntry(dueDate, () => OccupyTarget());
        //_target.AddHistory(_assignedParty.name + " started building a " + Utilities.NormalizeString(_constructionData.structure.name));
    }

    private void OccupyTarget() {
        LandmarkManager.Instance.CreateNewLandmarkOnTile(_target, Utilities.ConvertMaterialToLandmarkType(_target.materialOnTile));
        _target.landmarkOnTile.AddHistory(_assignedParty.name + " finished building a " + Utilities.NormalizeString(_constructionData.structure.name));
        //Build a new structure on that tile
        _target.landmarkOnTile.OccupyLandmark((createdBy as InternalQuestManager).owner);
        _postedAt.AddLandmarkAsOwned(_target.landmarkOnTile);
        //_target.AdjustPopulation(5);
        _target.landmarkOnTile.AdjustPopulation(_constructionData.production.civilianCost);
        _assignedParty.AdjustCivilians(-_constructionData.production.civilianCost);
        GoBackToQuestGiver(TASK_STATUS.SUCCESS);
    }
}
