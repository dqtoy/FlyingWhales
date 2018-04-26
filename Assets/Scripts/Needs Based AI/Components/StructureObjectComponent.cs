using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureObjectComponent : ObjectComponent {
    public StructureObj structureObject;

    #region Wild Pigs
    public void ChangeToDepletedState() {
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 15) {
            ObjectState depletedState = structureObject.GetState("Depleted");
            structureObject.ChangeState(depletedState);
        }
    }
    public void ChangeToTeemingState() {
        if(GameManager.Instance.days == 1) {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 25) {
                ObjectState teemingState = structureObject.GetState("Teeming");
                structureObject.ChangeState(teemingState);
            }
        }
    }
    #endregion
}
