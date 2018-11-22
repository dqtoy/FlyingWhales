using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenanceTemple : StructureObj {

    public PenanceTemple() : base() {
        _specificObjectType = LANDMARK_TYPE.PENANCE_TEMPLE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 50, currency = CURRENCY.MANA };
        _needsMinionAssignment = true;
    }

    #region Overrides
    public override IObject Clone() {
        PenanceTemple clone = new PenanceTemple();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAssignCharacter() {
        base.OnAssignCharacter();
        ScheduleCharacterToRemoveTrait();
    }
    #endregion

    #region Utilities
    private void ScheduleCharacterToRemoveTrait() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddHours(80);
        SchedulingManager.Instance.AddEntry(dueDate, () => CharacterRemovedTrait());
    }
    private void CharacterRemovedTrait() {
        if (!isRuined && _assignedCharacter != null) {
            Trait negativeTrait = _assignedCharacter.GetRandomNegativeTrait();
            if(negativeTrait != null) {
                _assignedCharacter.RemoveTrait(negativeTrait);
            }
            _assignedCharacter.minion.GoBackFromAssignment();
        }
        OnEndStructureEffect();
    }
    #endregion
}
