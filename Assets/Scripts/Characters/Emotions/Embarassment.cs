using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Embarassment : Emotion {

    public Embarassment() : base(EMOTION.Embarassment) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        witness.needsComponent.AdjustComfort(-15);
        witness.traitContainer.AddTrait(witness, "Ashamed");
        //Fight or Flight
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}