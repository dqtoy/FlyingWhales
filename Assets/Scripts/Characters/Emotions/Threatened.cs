using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threatened : Emotion {

    public Threatened() : base(EMOTION.Threatened) {
        mutuallyExclusive = new string[] { "Fear" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        //Fight or Flight
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}