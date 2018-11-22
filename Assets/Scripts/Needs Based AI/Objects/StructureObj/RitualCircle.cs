using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualCircle : StructureObj {

    private string traitForTheDay;
    private string lockedTrait;
    public RitualCircle() : base() {
        _specificObjectType = LANDMARK_TYPE.RITUAL_CIRCLE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 100, currency = CURRENCY.MANA };
        _needsMinionAssignment = true;
    }

    #region Overrides
    public override IObject Clone() {
        RitualCircle clone = new RitualCircle();
        SetCommonData(clone);
        return clone;
    }
    public override void StartDayAction() {
        base.StartDayAction();
        traitForTheDay = AttributeManager.Instance.GetRandomPositiveTrait();
    }
    public override void OnAssignCharacter() {
        base.OnAssignCharacter();
        ScheduleCharacterToGainTrait();
    }
    #endregion

    #region Utilities
    private void ScheduleCharacterToGainTrait() {
        lockedTrait = traitForTheDay;
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddHours(80);
        SchedulingManager.Instance.AddEntry(dueDate, () => CharacterGainsTrait());
    }
    private void CharacterGainsTrait() {
        if (!isRuined && _assignedCharacter != null) {
            _assignedCharacter.AddTrait(AttributeManager.Instance.allTraits[lockedTrait]);
            _assignedCharacter.minion.GoBackFromAssignment();
            lockedTrait = string.Empty;
        }
        OnEndStructureEffect();
    }
    #endregion
}
