using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingArena : StructureObj {

    public TrainingArena() : base() {
        _specificObjectType = LANDMARK_TYPE.TRAINING_ARENA;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY };
    }

    #region Overrides
    public override IObject Clone() {
        TrainingArena clone = new TrainingArena();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAssignCharacter() {
        base.OnAssignCharacter();
        ScheduleCharacterToGainExperience();
    }
    #endregion

    #region Utilities
    private void ScheduleCharacterToGainExperience() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddHours(80);
        SchedulingManager.Instance.AddEntry(dueDate, () => CharacterGainsExperience());
    }
    private void CharacterGainsExperience() {
        if(!isRuined && _assignedCharacter != null) {
            _assignedCharacter.minion.AdjustExp(100);
            _assignedCharacter.minion.GoBackFromAssignment();
        }
    }
    #endregion
}
