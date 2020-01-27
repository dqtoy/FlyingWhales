using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fear : Emotion {

    public Fear() : base(EMOTION.Fear) {
        mutuallyExclusive = new string[] { "Threatened" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        witness.traitContainer.AddTrait(witness, "Spooked");
        //Fight or Flight
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}