using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineWorship : FactionIdeology {
    public DivineWorship() : base(FACTION_IDEOLOGY.DIVINE_WORSHIP) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    #endregion
}
