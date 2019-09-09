using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandmark {

    public BaseLandmark location { get; private set; }

    public PlayerLandmark(BaseLandmark location) {
        this.location = location;
    }

    #region Virtuals
    public virtual void AssignMinion(Minion minion) { }
    public virtual void OnLandmarkDestroyed() {
        location = null;
    }
    public virtual void OnLandmarkPlaced() { }
    #endregion

    public override string ToString() {
        return GetType().ToString();
    }
}
