using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureObjectComponent : ObjectComponent {
    public StructureObj structureObject;

    #region getters/setters
    public override string name {
        get { return structureObject.objectName; }
    }
    #endregion

    #region Wild Pigs
    public void ChangeToDepletedState(IObject iobject) {
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 15) {
            ObjectState depletedState = iobject.GetState("Depleted");
            iobject.ChangeState(depletedState);
        }
    }
    public void ChangeToTeemingState(IObject iobject) {
        if(GameManager.Instance.days == 1) {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 25) {
                ObjectState teemingState = iobject.GetState("Teeming");
                iobject.ChangeState(teemingState);
            }
        }
    }
    #endregion
}
