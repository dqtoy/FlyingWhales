using System.Collections;
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
            new StateNameAndDuration(){ name = "Eat Poisoned", status = InteractionManager.Goap_State_Success, duration = AttributeManager.Instance.allTraits["Eating"].daysDuration },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Release Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ASSAULT_ACTION_NPC, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Target Injured", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Knocked Out", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Killed", status = InteractionManager.Goap_State_Fail, duration = 0 },
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
        {INTERACTION_TYPE.PLAY_GUITAR, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Play Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CHAT_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Chat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ARGUE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Argue Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STROLL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Stroll Success", status = InteractionManager.Goap_State_Success, duration = 6 },
            new StateNameAndDuration(){ name = "Stroll Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RETURN_HOME, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Return Home Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DRINK, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Drink Poisoned", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SLEEP_OUTSIDE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = AttributeManager.Instance.allTraits["Resting"].daysDuration },
            new StateNameAndDuration(){ name = "Rest Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EXPLORE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Explore Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Explore Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TABLE_REMOVE_POISON, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Remove Poison Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Remove Poison Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TABLE_POISON, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Poison Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Poison Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PRAY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Pray Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CHOP_WOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Chop Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
    };
}

public struct StateNameAndDuration {
    public string name;
    public string status;
    public int duration;
}
