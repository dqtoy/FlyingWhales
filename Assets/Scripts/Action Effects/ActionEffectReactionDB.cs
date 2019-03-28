using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActionEffectReactionDB {

    public static Dictionary<GoapEffect, ActionEffectReaction> eventIntelReactions = new Dictionary<GoapEffect, ActionEffectReaction>(new ActionEffectReactionComparer()) {
        { new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained"), new AddTraitReaction() },
        { new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY), new RemoveFromPartyReaction() },
    };


}

public class ActionEffectReactionComparer : IEqualityComparer<GoapEffect> {

    public bool Equals(GoapEffect x, GoapEffect y) {
        if (x.conditionType == y.conditionType) {
            if (string.IsNullOrEmpty(x.conditionString()) || string.IsNullOrEmpty(y.conditionString())) {
                return true;
            } else {
                return x.conditionString() == y.conditionString();
            }
        }
        return false;
    }

    public int GetHashCode(GoapEffect obj) {
        return obj.GetHashCode();
    }
}
