using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureObjectComponent : ObjectComponent {
    public SPECIFIC_OBJECT_TYPE specificObjectType;
    public bool isInvisible;
    public int maxHP;
    public ActionEvent onHPReachedZero;
    public ActionEvent onHPReachedFull;

    public void CopyDataToStructureObject(StructureObj structureObject) {
        structureObject.SetSpecificObjectType(specificObjectType);
        structureObject.SetIsInvisible(isInvisible);
        structureObject.SetOnHPFullFunction(onHPReachedFull);
        structureObject.SetOnHPZeroFunction(onHPReachedZero);
        structureObject.SetMaxHP(maxHP);
        structureObject.SetHP(maxHP);
    }

    #region getters/setters
    //public override string name {
    //    get { return structureObject.objectName; }
    //}
    #endregion

    #region General
    public void Repaired(IObject iobject) {
        ObjectState defaultState = iobject.GetState("Default");
        iobject.ChangeState(defaultState);
    }
    public void Destroyed(IObject iobject) {
        ObjectState defaultState = iobject.GetState("Ruined");
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
            if (structure.GetTotalCivilians() > 0) {
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
                //TODO: Make this flexible
                RESOURCE chosenResource = RESOURCE.HUMAN_CIVILIAN;
                if(structure.resourceInventory[RESOURCE.ELF_CIVILIAN] > 0 && structure.resourceInventory[RESOURCE.HUMAN_CIVILIAN] > 0) {
                    int raceChance = UnityEngine.Random.Range(0, 2);
                    if(raceChance == 0) {
                        chosenResource = RESOURCE.ELF_CIVILIAN;
                    }
                } else {
                    if(structure.resourceInventory[RESOURCE.ELF_CIVILIAN] > 0) {
                        chosenResource = RESOURCE.ELF_CIVILIAN;
                    }
                }
                structure.AdjustResource(chosenResource, -1);
            }
        }
    }
    #endregion

    #region Resources
    public void RepairedResourceStructure(IObject iobject) {
        if (iobject is StructureObj) {
            StructureObj structure = iobject as StructureObj;
            RESOURCE resource = Utilities.GetResourceTypeByObjectType(structure.specificObjectType);
            string defaultOrDepleted = "Depleted";
            if (structure.resourceInventory[resource] > 0) {
                defaultOrDepleted = "Default";
            }
            ObjectState defaultState = iobject.GetState(defaultOrDepleted);
            iobject.ChangeState(defaultState);
        }
    }
    #endregion

    #region Sealed Tomb
    public void RepairedSealedTomb(IObject iobject) {
        if (iobject.objectType == OBJECT_TYPE.STRUCTURE) {
            StructureObj structure = iobject as StructureObj;
            //if(structure.specificObjectType == SPECIFIC_OBJECT_TYPE.SEALED_TOMB) {
            //    SealedTomb sealedTomb = structure as SealedTomb;
            //    string unsealedOrEmpty = "Empty";
            //    if (sealedTomb.content != null) {
            //        unsealedOrEmpty = "Unsealed";
            //    }
            //    ObjectState state = iobject.GetState(unsealedOrEmpty);
            //    iobject.ChangeState(state);
            //}
        }
    }
    #endregion
}
