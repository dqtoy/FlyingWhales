﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldEventsDB {

    public static Dictionary<JOB_TYPE, WORLD_EVENT_EFFECT[]> jobEventsDB = new Dictionary<JOB_TYPE, WORLD_EVENT_EFFECT[]>() {
        {
            JOB_TYPE.OBTAIN_FOOD_OUTSIDE, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GET_FOOD }
        },
        {
            JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GET_SUPPLY }
        },
        {
            JOB_TYPE.IMPROVE, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GAIN_POSITIVE_TRAIT, WORLD_EVENT_EFFECT.REMOVE_NEGATIVE_TRAIT }
        },
        {
            JOB_TYPE.EXPLORE, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GAIN_POSITIVE_TRAIT, WORLD_EVENT_EFFECT.EXPLORE }
        },
        {
            JOB_TYPE.COMBAT, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.COMBAT }
        },
        {
            JOB_TYPE.DESTROY_PROFANE_LANDMARK, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.DESTROY_LANDMARK }
        },
        {
            JOB_TYPE.PERFORM_HOLY_INCANTATION, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.DIVINE_INTERVENTION_SPEED_UP }
        },
        {
            JOB_TYPE.CORRUPT_CULTIST, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.CORRUPT_CHARACTER }
        },
        {
            JOB_TYPE.SEARCHING_WORLD_EVENT, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.SEARCHING }
        },
        {
            JOB_TYPE.SABOTAGE_FACTION, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.DIVINE_INTERVENTION_SLOW_DOWN }
        },
        {
            JOB_TYPE.CLAIM_REGION, new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.CONQUER_REGION }
        },
    };
}
