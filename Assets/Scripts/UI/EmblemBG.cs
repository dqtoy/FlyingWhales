using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EmblemBG {
    public Sprite frame, tint, outline;

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
    public bool Equals(EmblemBG other) {
        if (this.frame.name.Equals(other.frame.name) && this.tint.name.Equals(other.tint.name) && this.outline.name.Equals(other.outline.name)) {
            return true;
        }
        return false;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}
