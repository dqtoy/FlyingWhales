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

    #region General
    public void Repaired(IObject iobject) {
        ObjectState defaultState = iobject.GetState("Default");
        iobject.ChangeState(defaultState);
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

    #region Torture Chamber
    public void RepairedTortureChamber(IObject iobject) {
        if (iobject is StructureObj) {
            StructureObj structure = iobject as StructureObj;
            string occupiedOrEmpty = "Empty";
            if (structure.resourceInventory[RESOURCE.CIVILIAN] > 0) {
                occupiedOrEmpty = "Occupied";
            }
            ObjectState defaultState = iobject.GetState(occupiedOrEmpty);
            iobject.ChangeState(defaultState);
        }
    }
    public void ChanceToReduceCivilianUponTorture(IObject iobject) {
        int chance = UnityEngine.Random.Range(0, 2);
        if(chance == 0) {
            if (iobject is StructureObj) {
                StructureObj structure = iobject as StructureObj;
                structure.AdjustResource(RESOURCE.CIVILIAN, -1);
            }
        }
    }
    #endregion
}
