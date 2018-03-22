using UnityEngine;
using System.Collections;

public class HypnotizeState : State {
    public HypnotizeState(CharacterTask parentTask) : base(parentTask, STATE.HYPNOTIZE) {
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        PerformHypnotize();
        return true;
    }
    #endregion

    private void PerformHypnotize() {
        string chosenAction = TaskManager.Instance.hypnotizeActions.PickRandomElementGivenWeights();
        if (chosenAction == "hypnotize") {
            Log hypnotizeLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Hypnotize", "hypnotize");
            hypnotizeLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            hypnotizeLog.AddToFillers(parentTask.specificTarget as ECS.Character, (parentTask.specificTarget as ECS.Character).name, LOG_IDENTIFIER.TARGET_CHARACTER);
            (parentTask.specificTarget as ECS.Character).AssignTag(CHARACTER_TAG.HYPNOTIZED);
            MakeTargetCharacterAVampireFollower();
            parentTask.EndTask(TASK_STATUS.SUCCESS);
            return;
        }
    }

    private void MakeTargetCharacterAVampireFollower() {
		ECS.Character target = (ECS.Character)_parentTask.specificTarget;
		if(target.party != null){
			target.party.DisbandParty ();
		}
        if (_assignedCharacter.party == null) {
            Party party = _assignedCharacter.CreateNewParty();
			party.AddPartyMember(target);
        } else {
			_assignedCharacter.party.AddPartyMember(target);
        }
		target.SetFollowerState(true);

        //TODO: What happens if the target character already has a party, is it going to be disbanded? what happens to the followers then?
    }
}
