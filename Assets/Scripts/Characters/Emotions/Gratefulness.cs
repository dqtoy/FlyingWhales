using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gratefulness : Emotion {

    public Gratefulness() : base(EMOTION.Gratefulness) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.opinionComponent.AdjustOpinion(targetCharacter, "Gratefulness", 7);
        }
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}