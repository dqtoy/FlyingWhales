using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingArena : StructureObj {

    public TrainingArena() : base() {
        _specificObjectType = LANDMARK_TYPE.TRAINING_ARENA;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY };
        _needsMinionAssignment = true;
    }

    #region Overrides
    public override IObject Clone() {
        TrainingArena clone = new TrainingArena();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAssignCharacter() {
        base.OnAssignCharacter();
        ScheduleCharacterToGainLevel();
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        //newLocation.SetMaxDefenderCount(2);
        base.OnAddToLandmark(newLocation);
    }
    #endregion

    #region Utilities
    private void ScheduleCharacterToGainLevel() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(80);
        SchedulingManager.Instance.AddEntry(dueDate, () => CharacterGainsLevel());
    }
    private void CharacterGainsLevel() {
        if(!isRuined && _assignedCharacter != null) {
            _assignedCharacter.minion.LevelUp();
            _assignedCharacter.minion.GoBackFromAssignment();
        }
        OnEndStructureEffect();
    }
    #endregion
}
