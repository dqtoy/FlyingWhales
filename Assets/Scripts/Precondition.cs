using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Precondition {
    public CHARACTER keyTarget { get; private set; }
    public InteractionCharacterEffect keyEffect { get; private set; }

    public Precondition(CHARACTER keyTarget, INTERACTION_CHARACTER_EFFECT characterEffect, string effectString) {
        this.keyTarget = keyTarget;
        this.keyEffect = new InteractionCharacterEffect() {
            effect = characterEffect,
            effectString = effectString,
        };
    }

    public bool CanSatisfyCondition(Character character) {
        //TODO
        return true;
    }
}