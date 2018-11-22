using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaExtractor : StructureObj {

    private const int MANA_PER_TICK = 1;

    public ManaExtractor() : base() {
        _specificObjectType = LANDMARK_TYPE.MANA_EXTRACTOR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _effectCost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY };
    }

    #region Overrides
    public override IObject Clone() {
        ManaExtractor clone = new ManaExtractor();
        SetCommonData(clone);
        return clone;
    }
    public override void StartState(ObjectState state) {
        base.StartState(state);
        if (state.stateName == "Ruined") {
            Messenger.RemoveListener(Signals.HOUR_STARTED, ProduceMana);
        }
    }
    public override void OnAssignCharacter() {
        base.OnAssignCharacter();
        Messenger.AddListener(Signals.HOUR_STARTED, ProduceMana);
        ScheduleCharacterToGoHome();
    }
    #endregion

    #region Utilities
    private void ScheduleCharacterToGoHome() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddHours(GameManager.hoursPerDay);
        SchedulingManager.Instance.AddEntry(dueDate, () => CharacterGoesHome());
    }
    private void CharacterGoesHome() {
        if (!isRuined && _assignedCharacter != null) {
            _assignedCharacter.minion.GoBackFromAssignment();
            Messenger.RemoveListener(Signals.HOUR_STARTED, ProduceMana);
        }
    }
    private void ProduceMana() {
        ////Provides the player 20 Mana Stones at the start of each day until the Mana Stones have been exhausted.
        //if (this.objectLocation.tileLocation.data.manaOnTile > 0) {
        //    this.objectLocation.tileLocation.AdjustManaOnTile(-MANA_PER_TICK);
        //}
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.MANA, MANA_PER_TICK);
    }
    #endregion
}
