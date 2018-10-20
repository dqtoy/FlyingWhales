using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Buff {
    public STAT buffedStat;
    public float percentage;

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
    public bool Equals(Buff otherBuff) {
        return otherBuff.buffedStat == this.buffedStat && otherBuff.percentage == this.percentage;
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
}
