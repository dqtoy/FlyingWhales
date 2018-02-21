using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveLandmark : OldQuest.Quest {

	private BaseLandmark _target;

	#region getters/setters
	public BaseLandmark target {
		get { return _target; }
	}
	#endregion

	public SaveLandmark(TaskCreator createdBy, BaseLandmark target) : base(createdBy, QUEST_TYPE.SAVE_LANDMARK) {
		_target = target;
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction((createdBy as BaseLandmark).owner)
		};
	}
	#region overrides

	protected override void ConstructQuestLine() {
		base.ConstructQuestLine();

		GoToLocation goToLandmark = new GoToLocation(this); //Go to the picked region
		goToLandmark.InititalizeAction(_target);
		goToLandmark.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
		goToLandmark.onTaskDoAction += goToLandmark.Generic;
		goToLandmark.onTaskActionDone += SaveTheLandmark;


		GoToLocation goBackToQuestPoster = new GoToLocation(this); //Go to the picked region
		goBackToQuestPoster.InititalizeAction(_postedAt);
		goBackToQuestPoster.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
		goBackToQuestPoster.onTaskDoAction += goToLandmark.Generic;
		goBackToQuestPoster.onTaskActionDone += SuccessQuest;

		_questLine.Enqueue(goToLandmark);
		_questLine.Enqueue(goBackToQuestPoster);
	}
	#endregion


	private void SaveTheLandmark(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (2);
		SchedulingManager.Instance.AddEntry (newSched, () => PerformNextQuestAction ());
	}

	private void SuccessQuest(){
		EndQuest (TASK_STATUS.SUCCESS);
	}
}
