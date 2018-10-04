using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaExtractor : StructureObj {

    public ManaExtractor() : base() {
        _specificObjectType = LANDMARK_TYPE.MANA_EXTRACTOR;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        ManaExtractor clone = new ManaExtractor();
        SetCommonData(clone);
        return clone;
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        base.OnAddToLandmark(newLocation);
        Messenger.AddListener(Signals.DAY_START, ProduceMana);
    }
    public override void StartState(ObjectState state) {
        base.StartState(state);
        if (state.stateName == "Ruined") {
            Messenger.RemoveListener(Signals.DAY_START, ProduceMana);
        }
    }
    public override void EndState(ObjectState state) {
        base.EndState(state);
        if (state.stateName == "Ruined") {
            Messenger.AddListener(Signals.DAY_START, ProduceMana);
        }
    }
    #endregion

    private void ProduceMana() {
        //Provides the player 20 Mana Stones at the start of each day until the Mana Stones have been exhausted.
        if (this.objectLocation.tileLocation.data.manaOnTile > 0) {
            PlayerManager.Instance.player.AdjustMana(20);
            this.objectLocation.tileLocation.AdjustManaOnTile(-20);
        }
    }
}
