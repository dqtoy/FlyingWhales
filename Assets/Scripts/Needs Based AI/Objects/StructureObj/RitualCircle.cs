using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualCircle : StructureObj {

    private string _traitForTheDay;
    private string _lockedTrait;

    #region getters/setters
    public string traitForTheDay {
        get { return _traitForTheDay; }
    }
    #endregion
    public RitualCircle() : base() {
        _specificObjectType = LANDMARK_TYPE.RITUAL_CIRCLE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 100, currency = CURRENCY.MANA };
        _needsMinionAssignment = true;
        _traitForTheDay = string.Empty;
    }

    #region Overrides
    public override IObject Clone() {
        RitualCircle clone = new RitualCircle();
        SetCommonData(clone);
        return clone;
    }
    public override void StartDayAction() {
        base.StartDayAction();
        _traitForTheDay = AttributeManager.Instance.GetRandomPositiveTrait();
        Messenger.Broadcast(Signals.UPDATE_RITUAL_CIRCLE_TRAIT, this);
    }
    public override void OnAssignCharacter() {
        base.OnAssignCharacter();
        ScheduleCharacterToGainTrait();
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        //newLocation.SetMaxDefenderCount(2);
        base.OnAddToLandmark(newLocation);
        _traitForTheDay = AttributeManager.Instance.GetRandomPositiveTrait();
        Messenger.Broadcast(Signals.UPDATE_RITUAL_CIRCLE_TRAIT, this);
    }
    #endregion

    #region Utilities
    private void ScheduleCharacterToGainTrait() {
        _lockedTrait = _traitForTheDay;
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(80);
        SchedulingManager.Instance.AddEntry(dueDate, () => CharacterGainsTrait());
    }
    private void CharacterGainsTrait() {
        if (!isRuined && _assignedCharacter != null) {
            _assignedCharacter.AddTrait(AttributeManager.Instance.allTraits[_lockedTrait]);
            _assignedCharacter.minion.GoBackFromAssignment();
            _lockedTrait = string.Empty;
        }
        OnEndStructureEffect();
    }
    #endregion
}
