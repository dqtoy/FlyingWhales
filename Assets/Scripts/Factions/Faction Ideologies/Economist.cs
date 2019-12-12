using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Economist : FactionIdeology {
    public Economist() : base(FACTION_IDEOLOGY.ECONOMIST) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    #endregion
}
