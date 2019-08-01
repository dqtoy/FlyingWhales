using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GoapActionStateDB {

    public static string No_Icon = "None";
    public static string Eat_Icon = "Eat";
    public static string Hostile_Icon = "Hostile";
    public static string Sleep_Icon = "Sleep";
    public static string Social_Icon = "Social";
    public static string Work_Icon = "Work";
    public static string Drink_Icon = "Drink";
    public static string Entertain_Icon = "Entertain";
    public static string Explore_Icon = "Explore";
    public static string FirstAid_Icon = "First Aid";
    public static string Flee_Icon = "Flee";
    public static string Patrol_Icon = "Patrol";

    public static string GetStateResult(INTERACTION_TYPE goapType, string stateName) {
        if (goapActionStates.ContainsKey(goapType)) {
            StateNameAndDuration[] snd = goapActionStates[goapType];
            for (int i = 0; i < snd.Length; i++) {
                StateNameAndDuration currSND = snd[i];
                if (currSND.name == stateName) {
                    return currSND.status;
                }
            }
        }
        return string.Empty;
    }

    public static Dictionary<INTERACTION_TYPE, StateNameAndDuration[]> goapActionStates = new Dictionary<INTERACTION_TYPE, StateNameAndDuration[]>() {
        {INTERACTION_TYPE.EAT_PLANT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_SMALL_ANIMAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_DWELLING_TABLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Poisoned", status = InteractionManager.Goap_State_Success, duration = 6 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_CORPSE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Release Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ASSAULT_ACTION_NPC, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Target Injured", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Knocked Out", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Killed", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Assault Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "In Progress", status = InteractionManager.Goap_State_Fail, duration = -1 },
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
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(6) },
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
        {INTERACTION_TYPE.PLAY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Play Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PLAY_GUITAR, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Play Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CHAT_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Quick Chat", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Share Information", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Friends", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Enemies", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Argument", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Flirt", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Lovers", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Paramours", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Resolve Enmity", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ARGUE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Argue Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Argue Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
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
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(6) },
            new StateNameAndDuration(){ name = "Rest Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.EXPLORE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Explore Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Explore Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REMOVE_POISON_TABLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Remove Poison Success", status = InteractionManager.Goap_State_Success, duration = 0 },
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
        {INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Perform Ritual Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Perform Ritual Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PATROL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Patrol Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Patrol Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PATROL_ROAM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Patrol Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Patrol Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STEAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SCRAP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Scrap Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.GET_SUPPLY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_SUPPLY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TILE_OBJECT_DESTROY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ITEM_DESTROY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TRAVEL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Travel Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Travel Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RETURN_HOME_LOCATION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Return Home Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.HUNT_ACTION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Target Injured", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Killed", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Won", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TRANSFORM_TO_WOLF, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REVERT_TO_NORMAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RESTRAIN_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Restrain Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.FIRST_AID_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "First Aid Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CURE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Cure Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CURSE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Curse Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DISPEL_MAGIC, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Dispel Magic Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.JUDGE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Target Executed", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Released", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Exiled", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.REPORT_CRIME, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Report Crime Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Report Crime Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.FEED, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Feed Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STEAL_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Steal Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_ITEM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STAND, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Stand Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.SIT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Sit Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Sit Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.NAP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Nap Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.BURY_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Bury Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CARRY_CORPSE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REMEMBER_FALLEN, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Remember Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_ITEM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.SPIT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Spit Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REPORT_HOSTILE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Report Hostile Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Report Hostile Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Invite Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Invite Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.MAKE_LOVE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Make Love Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Make Love Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DRINK_BLOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Drink Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REPLACE_TILE_OBJECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Replace Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_FURNITURE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Craft Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.TANTRUM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Tantrum Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_MUSHROOM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Break Friendship Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            //new StateNameAndDuration(){ name = "Break Friendship Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Break Love Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            //new StateNameAndDuration(){ name = "Break Love Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.BREAK_UP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Break Up Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SHARE_INFORMATION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Share Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.WATCH, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Watch Success", status = InteractionManager.Goap_State_Success, duration = -1 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.INSPECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Inspect Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        { INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "In Progress", status = InteractionManager.Goap_State_Success, duration = -1 },
        } },
        { INTERACTION_TYPE.ROAMING_TO_STEAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "In Progress", status = InteractionManager.Goap_State_Success, duration = -1 },
        } },
    };
}

public struct StateNameAndDuration {
    public string name;
    public string status;
    public int duration;
}
