using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Militarist : FactionIdeology {

    public Militarist() : base(FACTION_IDEOLOGY.MILITARIST) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    #endregion
}
