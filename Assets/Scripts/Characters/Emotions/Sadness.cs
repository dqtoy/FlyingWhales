﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sadness : Emotion {

    public Sadness() : base(EMOTION.Sadness) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        witness.needsComponent.AdjustHappiness(-10);
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}