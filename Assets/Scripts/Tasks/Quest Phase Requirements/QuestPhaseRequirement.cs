using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 Base class for the requirements of a quest phase before the quest can advance.
 Each character that accepted the quest will have their own instance 
 of this class in their quest data.
     */
public class QuestPhaseRequirement : ICloneable{

    protected bool _isRequirementMet;
    protected ECS.Character owner;

    #region getters/setters
    public bool isRequirementMet {
        get { return _isRequirementMet; }
    }
    #endregion

    public QuestPhaseRequirement() {
        _isRequirementMet = false;
    }
	public virtual void ActivateRequirement(ECS.Character owner){ this.owner = owner; }
    public object Clone() {
        return this.MemberwiseClone();
    }
}

public class MustHaveItems : QuestPhaseRequirement {

    private List<string> neededItems;

    public MustHaveItems(string neededItem) : base(){
        neededItems = new List<string>();
        neededItems.Add(neededItem);
    }
    public MustHaveItems(List<string> neededItems) : base() {
        this.neededItems = new List<string>(neededItems);
    }

    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Character, ECS.Item>(Signals.OBTAIN_ITEM, CheckIfRequirementMet);
    }

    protected void CheckIfRequirementMet(ECS.Character character, ECS.Item item) {
        if (character.id == owner.id) {
            for (int i = 0; i < neededItems.Count; i++) {
                string currNeededItem = neededItems[i];
                if (!character.HasItem(currNeededItem)) {
                    //the character does not yet have all of the needed items, return.
                    return;
                }
            }
            //the character has all needed items, requirement has been met
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }

}

public class MustFindItem : QuestPhaseRequirement {

    private string neededItem;

    public MustFindItem(string neededItem) : base() {
        this.neededItem = neededItem;
    }

    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Character, ECS.Item>(Signals.OBTAIN_ITEM, CheckIfRequirementMet);
    }

    protected void CheckIfRequirementMet(ECS.Character character, ECS.Item item) {
        if (character.id == owner.id) {
            if (!item.itemName.Equals(neededItem)) {
                return; //character did not find the right item
            }
            //the character found the needed item
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }
}

public class MustFinishAllPhaseTasks : QuestPhaseRequirement {

    public MustFinishAllPhaseTasks() : base() {}

    public override void ActivateRequirement(ECS.Character owner) {
        base.ActivateRequirement(owner);
        Messenger.AddListener<ECS.Character, CharacterTask>(Signals.TASK_SUCCESS, CheckIfRequirementMet);
    }

    protected void CheckIfRequirementMet(ECS.Character character, CharacterTask succeededTask) {
        if (character.id == owner.id && succeededTask.assignedCharacter.id == character.id) {
            for (int i = 0; i < character.questData.tasks.Count; i++) {
                if (!character.questData.tasks[i].isDone) {
                    return; //character has not yet finished all tasks
                }
            }
            //the character finished all the tasks
            _isRequirementMet = true;
            character.questData.CheckPhaseAdvancement();
        }
    }

}
