using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jolted : Trait {
    public Jolted() {
        name = "Jolted";
        description = "This character is pumped.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        base.OnAddTrait(sourcePOI);
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.marker.AdjustSpeedModifier(2f);
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.marker.AdjustSpeedModifier(-2f);
        }
        base.OnRemoveTrait(sourcePOI);
    }
    #endregion
}
