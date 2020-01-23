using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Approval : Emotion {

    public Approval() : base(EMOTION.Approval) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.opinionComponent.AdjustOpinion(targetCharacter, "Approval", 5);
        }
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}
