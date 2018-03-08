using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestPhase {
    private Quest _sourceQuest;
    private List<CharacterTask> _tasks;

    public QuestPhase(Quest sourceQuest) {
        _sourceQuest = sourceQuest;
    }

    #region virtuals
    public virtual void OnCompleteTask() { }
    public virtual void OnPhaseComplete() { }
    /*
     Add tasks from this phase to a characters action weights
         */
    public virtual void AddTasksToWeightedDictionary(WeightedDictionary<CharacterTask> actionWeights) { }
    #endregion
}
