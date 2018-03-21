using UnityEngine;
using System.Collections;
using ECS;

public class ProclaimSuccessorState : State {
    public ProclaimSuccessorState(CharacterTask parentTask) : base(parentTask, STATE.PROCLAIM_SUCCESSOR) {
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        Character successor = null;
        if (parentTask.specificTarget == null) {
            WeightedDictionary<ECS.Character> characterWeights = GetCharacterTargetWeights(_assignedCharacter);
            successor = characterWeights.PickRandomElementGivenWeights();
        } else {
            successor = parentTask.specificTarget as ECS.Character;
        }

        if (successor.HasTag(CHARACTER_TAG.SUCCESSOR)) {
            throw new System.Exception(successor.name + " is already a successor!");
        }
        Successor tag = successor.AssignTag(CHARACTER_TAG.SUCCESSOR) as Successor;
        tag.SetCharacterToSucceed(_assignedCharacter);
        Log proclaimLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "ProclaimSuccessor", "proclaim");
        proclaimLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        proclaimLog.AddToFillers(successor, successor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        _assignedCharacter.AddHistory(proclaimLog);
        successor.AddHistory(proclaimLog);

        parentTask.EndTask(TASK_STATUS.SUCCESS);
        return true;
    }
    #endregion

    protected WeightedDictionary<Character> GetCharacterTargetWeights(Character character) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < character.faction.characters.Count; i++) {
            ECS.Character currCharacter = character.faction.characters[i];
            if (currCharacter.id != character.id) {
                characterWeights.AddElement(currCharacter, 1); //Each character of the same faction: +1 Weight
            }
        }
        return characterWeights;
    }
}
