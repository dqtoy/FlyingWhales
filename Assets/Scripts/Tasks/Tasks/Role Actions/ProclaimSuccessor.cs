using UnityEngine;
using System.Collections;
using ECS;

public class ProclaimSuccessor : CharacterTask {
    public ProclaimSuccessor(TaskCreator createdBy) : base(createdBy, TASK_TYPE.PROCLAIM_SUCCESSOR) {
        SetStance(STANCE.NEUTRAL);
        _needsSpecificTarget = true;
        _specificTargetClassification = "character";
        _filters = new TaskFilter[] {
            new MustNotHaveTags(CHARACTER_TAG.SUCCESSOR),
            new MustBeFaction((createdBy as ECS.Character).faction)
        };

    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        if (_specificTarget == null) {
            WeightedDictionary<ECS.Character> characterWeights = GetCharacterTargetWeights(character);
            _specificTarget = characterWeights.PickRandomElementGivenWeights();
        }

        ECS.Character successor = _specificTarget as ECS.Character;
        successor.AssignTag(CHARACTER_TAG.SUCCESSOR);
    }
    public override bool CanBeDone(Character character, ILocation location) {
        if (character.faction != null && character.faction is Tribe) {
            if ((character.faction as Tribe).successor == null) {
                return true; //If character is part of a faction and there are no Successor tag for any character of the faction
            }
        }
        return base.CanBeDone(character, location);
    }
    public override bool AreConditionsMet(Character character) {
        return CanBeDone(character, null);
    }
    public override int GetSelectionWeight(Character character) {
        return 20;
    }
    protected override WeightedDictionary<Character> GetCharacterTargetWeights(Character character) {
        WeightedDictionary<Character> characterWeights = base.GetCharacterTargetWeights(character);
        for (int i = 0; i < character.faction.characters.Count; i++) {
            ECS.Character currCharacter = character.faction.characters[i];
            if (currCharacter.id != character.id) {
                characterWeights.AddElement(currCharacter, 1); //Each character of the same faction: +1 Weight
            }
        }
        return characterWeights;
    }
    #endregion
}
