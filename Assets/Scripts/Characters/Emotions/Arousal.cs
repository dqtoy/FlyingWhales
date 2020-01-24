using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arousal : Emotion {

    public Arousal() : base(EMOTION.Arousal) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.opinionComponent.AdjustOpinion(targetCharacter, "Arousal", 4);
        }
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}