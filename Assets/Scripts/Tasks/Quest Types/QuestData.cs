using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestData {

    private Quest _activeQuest;
    private int _currentPhase;
    private ECS.Character _owner;

    private List<CharacterTask> _tasks;
    private List<QuestPhaseRequirement> _advancementRequirements;

    #region getters/setters
    public Quest activeQuest {
        get { return _activeQuest; }
    }
    public List<CharacterTask> tasks {
        get { return _tasks; }
    }
	public int currentPhase {
		get { return _currentPhase; }
	}
    #endregion

    public QuestData(ECS.Character owner) {
        _owner = owner;
        _advancementRequirements = new List<QuestPhaseRequirement>();
    }

    public void SetActiveQuest(Quest quest) {
        _activeQuest = quest;
        if (quest == null) {
            //quest is being set to null, it means the quest is no longer available or the character chose to leave the quest
            if (_owner.currentTask != null && tasks.Contains(_owner.currentTask)) {
                _owner.currentTask.EndTask(TASK_STATUS.CANCEL); //cancel the characters current task if it comes from the list of tasks granted by his/her active quest
            }
        }
        SetQuestPhase(0);
    }
    public void SetQuestPhase(int phase) {
        _currentPhase = phase;
        if (_activeQuest != null) {
            QuestPhase questPhase = GetQuestPhase();
            _tasks = new List<CharacterTask>(questPhase.tasks);
            for (int i = 0; i < _advancementRequirements.Count; i++) {
                QuestPhaseRequirement currRequirement = _advancementRequirements[i];
                currRequirement.DeactivateRequirement();
            }
            _advancementRequirements.Clear();
            questPhase.OnPhaseActive(_owner);
        } else {
            _tasks.Clear();
        }
    }
    public void AdvanceToNextPhase() {
        if (_activeQuest.phases.Count > _currentPhase + 1) {
            Debug.Log("Setting " + _owner.name + "'s quest phase to " + (_currentPhase + 1).ToString());
            //there is still a next phase, advance to the next phase
            SetQuestPhase(_currentPhase + 1);
        } else {
            Debug.Log("All phases have been finished! Quest: " + activeQuest.questName + " has ended!");
            //there are no more phases, end the quest
            EndQuest(TASK_STATUS.SUCCESS);
        }
        UIManager.Instance.UpdateQuestsSummary();
    }
    public QuestPhase GetQuestPhase() {
        return _activeQuest.phases[_currentPhase];
    }
    public void EndQuest(TASK_STATUS taskStatus) {
        _activeQuest.EndQuest(taskStatus, _owner);
    }
    //private List<CharacterTask> GetAllTasks() {
    //    List<CharacterTask> allTasks = new List<CharacterTask>();
    //    allTasks.AddRange(_tasks);
    //    return allTasks;
    //}
    ///*
    // What to do when a task succeeds?
    //     */
    //public void OnTaskSuccess(CharacterTask succeededTask) {
    //    if (!_importantTasks.Contains(succeededTask) && !_unimportantTasks.Contains(succeededTask)) {
    //        throw new System.Exception(_owner.name + " does not have a task " + succeededTask.taskType.ToString() + " in his task list!");
    //    }

    //    if (_unimportantTasks.Contains(succeededTask)) {
    //        //succeeded task is unimportant to the progress of the quest
    //        succeededTask.ResetTask();
    //    } else {
    //        //succeeded task is important to the progress of the quest, check if the character has done all the important tasks.

    //        //check if all the tasks in the important tasks list are done
    //        for (int i = 0; i < _importantTasks.Count; i++) {
    //            CharacterTask currTask = _importantTasks[i];
    //            if (!currTask.isDone) {
    //                return; //there is still a pending task
    //            }
    //        }

    //        //All tasks are done, check if there is still a next phase
    //        if (_activeQuest.phases.Count > _currentPhase + 1) {
    //            //there is still a next phase, advance to the next phase
    //            AdvanceToNextPhase();
    //        } else {
    //            //there are no more phases, end the quest
    //            EndQuest(TASK_STATUS.SUCCESS);
    //        }
    //    }
    //}

    public void AddQuestTasksToWeightedDictionary(WeightedDictionary<CharacterTask> actionWeights) {
        for (int i = 0; i < _tasks.Count; i++) {
            CharacterTask currTask = _tasks[i];
            if (!currTask.isDone && !currTask.forPlayerOnly && currTask.AreConditionsMet(_owner)) {
                actionWeights.AddElement(currTask, currTask.GetSelectionWeight(_owner));
            }
        }
    }

    public void AddPhaseRequirement(QuestPhaseRequirement requirement) {
        _advancementRequirements.Add(requirement);
        requirement.ActivateRequirement(_owner);
    }
    public void CheckPhaseAdvancement() {
        for (int i = 0; i < _advancementRequirements.Count; i++) {
            QuestPhaseRequirement currRequirement = _advancementRequirements[i];
            if (!currRequirement.isRequirementMet) {
                return; //there is a requirement that has not yet been met
            }
        }
        Debug.Log("All requirements for " + _owner.name + "'s quest (" + activeQuest.questName + "). Quest phase: " + GetQuestPhase().phaseName);
        AdvanceToNextPhase(); //all requirements have been met, advance to next phase
    }
}
