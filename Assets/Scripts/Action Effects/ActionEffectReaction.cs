using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEffectReaction {

    public virtual string GetReactionFrom(Character character, Intel intel, GoapEffect effect) {
        return string.Empty;
    }
}
