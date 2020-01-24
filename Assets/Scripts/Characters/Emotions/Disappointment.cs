using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappointment : Emotion {

    public Disappointment() : base(EMOTION.Disappointment) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.opinionComponent.AdjustOpinion(targetCharacter, "Disappointment", -4);
            witness.traitContainer.AddTrait(witness, "Annoyed");
        }
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}