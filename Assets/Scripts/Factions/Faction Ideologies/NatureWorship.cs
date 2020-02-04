using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureWorship : FactionIdeology {

    public NatureWorship() : base(FACTION_IDEOLOGY.NATURE_WORSHIP) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    #endregion
}
