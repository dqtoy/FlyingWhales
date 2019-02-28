using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Precondition {
    public GoapEffect goapEffect { get; private set; }
    public Func<bool> condition { get; private set; }

    public Precondition(GoapEffect goapEffect, Func<bool> condition) {
        this.goapEffect = goapEffect;
        this.condition = condition;
    }

    public bool CanSatisfyCondition() {
        return condition();
    }
}