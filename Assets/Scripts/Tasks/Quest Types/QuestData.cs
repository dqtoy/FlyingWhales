using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestData {

    private Quest _activeQuest;
    private int _currentPhase;
    private ECS.Character _owner;

    private List<CharacterTask> _importantTasks;
    private List<CharacterTask> _unimportantTasks;

    #region getters/setters
    public Quest activeQuest {
        get { return _activeQuest; }
    }
    public List<CharacterTask> allTasks {
        get { return GetAllTasks(); }
    }
    #endregion

    public QuestData(ECS.Character owner) {
        _owner = owner;
    }

    public void SetActiveQuest(Quest quest) {
        _activeQuest = quest;
        if (quest == null) {
            //quest is being set to null, it means the quest is no longer available or the character chose to leave the quest
            if (_owner.currentTask != null && allTasks.Contains(_owner.currentTask)) {
                _owner.currentTask.EndTask(TASK_STATUS.CANCEL); //cancel the characters current task if it comes from the list of tasks granted by his/her active quest
            }
        }
        SetQuestPhase(0);
    }
    public void SetQuestPhase(int phase) {
        _currentPhase = phase;
        if (_activeQuest != null) {
            _importantTasks = new List<CharacterTask>(GetQuestPhase().importantTasks);
            _unimportantTasks = new List<CharacterTask>(GetQuestPhase().untimportantTasks);
        } else {
            _importantTasks.Clear();
            _unimportantTasks.Clear();
        }
    }
    public void AdvanceToNextPhase() {
        SetQuestPhase(_currentPhase + 1);
        UIManager.Instance.UpdateQuestsSummary();
    }
    public QuestPhase GetQuestPhase() {
        return _activeQuest.phases[_currentPhase];
    }
    private void EndQuest(TASK_STATUS taskStatus) {
        _activeQuest.EndQuest(taskStatus, _owner);
    }
    private List<CharacterTask> GetAllTasks() {
        List<CharacterTask> allTasks = new List<CharacterTask>();
        allTasks.AddRange(_importantTasks);
        allTasks.AddRange(_unimportantTasks);
        return allTasks;
    }
    /*
     What to do when a task succeeds?
         */
    public void OnTaskSuccess(CharacterTask succeededTask) {
        if (!_importantTasks.Contains(succeededTask) && !_unimportantTasks.Contains(succeededTask)) {
            throw new System.Exception(_owner.name + " does not have a task " + succeededTask.taskType.ToString() + " in his task list!");
        }

        if (_unimportantTasks.Contains(succeededTask)) {
            //succeeded task is unimportant to the progress of the quest
            succeededTask.ResetTask();
        } else {
            //succeeded task is important to the progress of the quest, check if the character has done all the important tasks.

            //check if all the tasks in the important tasks list are done
            for (int i = 0; i < _importantTasks.Count; i++) {
                CharacterTask currTask = _importantTasks[i];
                if (!currTask.isDone) {
                    return; //there is still a pending task
                }
            }

            //All tasks are done, check if there is still a next phase
            if (_activeQuest.phases.Count > _currentPhase + 1) {
                //there is still a next phase, advance to the next phase
                AdvanceToNextPhase();
            } else {
                //there are no more phases, end the quest
                EndQuest(TASK_STATUS.SUCCESS);
            }
        }
    }

    public void AddQuestTasksToWeightedDictionary(WeightedDictionary<CharacterTask> actionWeights) {
        for (int i = 0; i < _importantTasks.Count; i++) {
            CharacterTask currTask = _importantTasks[i];
            if (!currTask.isDone) {
                actionWeights.AddElement(currTask, currTask.GetSelectionWeight(_owner));
            }
        }
    }
}
