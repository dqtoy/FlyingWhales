using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionIdeology {
    public FACTION_IDEOLOGY ideologyType { get; protected set; }

    public FactionIdeology(FACTION_IDEOLOGY ideology) {
        ideologyType = ideology;
    }

    #region Virtuals
    public virtual void SetRequirements(Faction faction) { }
    public virtual bool DoesCharacterFitIdeology(Character character) { return false; }
    #endregion
}
