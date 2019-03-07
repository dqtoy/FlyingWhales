﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GoapActionStateDB {

    public static Dictionary<INTERACTION_TYPE, StateNameAndDuration[]> goapActionStates = new Dictionary<INTERACTION_TYPE, StateNameAndDuration[]>() {
        {INTERACTION_TYPE.EAT_PLANT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = AttributeManager.Instance.allTraits["Eating"].daysDuration },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_SMALL_ANIMAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = AttributeManager.Instance.allTraits["Eating"].daysDuration },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_DWELLING_TABLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = AttributeManager.Instance.allTraits["Eating"].daysDuration },
            new StateNameAndDuration(){ name = "Eat Poisoned", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Release Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ASSAULT_ACTION_NPC, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Target Injured", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Knocked Out", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Killed", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ABDUCT_ACTION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Abduct Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CARRY_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.MINE_ACTION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SLEEP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = AttributeManager.Instance.allTraits["Resting"].daysDuration },
            new StateNameAndDuration(){ name = "Rest Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PICK_ITEM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DAYDREAM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Daydream Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Daydream Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
    };
}

public struct StateNameAndDuration {
    public string name;
    public string status;
    public int duration;
}