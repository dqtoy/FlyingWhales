using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shock : Emotion {

    public Shock() : base(EMOTION.Shock) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        witness.needsComponent.AdjustComfort(-10);
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}