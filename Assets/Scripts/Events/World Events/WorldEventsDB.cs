using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldEventsDB {

    public static Dictionary<JOB_TYPE, JobWorldEventData> jobEventsDB = new Dictionary<JOB_TYPE, JobWorldEventData>() {
        {
            JOB_TYPE.OBTAIN_FOOD_OUTSIDE,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GET_FOOD },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GET_SUPPLY },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.IMPROVE,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GAIN_POSITIVE_TRAIT, WORLD_EVENT_EFFECT.REMOVE_NEGATIVE_TRAIT },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.EXPLORE,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.GAIN_POSITIVE_TRAIT, WORLD_EVENT_EFFECT.EXPLORE },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.COMBAT_WORLD_EVENT, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.COMBAT },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.DESTROY_PROFANE_LANDMARK, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.DESTROY_LANDMARK },
                validRegionGetter = ProfaneRegionGetter
            }
        },
        {
            JOB_TYPE.PERFORM_HOLY_INCANTATION, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.DIVINE_INTERVENTION_SPEED_UP },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.CORRUPT_CULTIST, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.CORRUPT_CHARACTER },
                validRegionGetter = ProfaneRegionGetter
            }
        },
        {
            JOB_TYPE.SEARCHING_WORLD_EVENT, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.SEARCHING }
            }
        },
        {
            JOB_TYPE.SABOTAGE_FACTION, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.DIVINE_INTERVENTION_SLOW_DOWN },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.RETURN_HOME,
            new JobWorldEventData() {
                validRegionGetter = ReturnHomeRegionGetter
            }
        },
        {
            JOB_TYPE.CLAIM_REGION, 
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.CONQUER_REGION },
                validRegionGetter = DefaultRegionGetter
            }
        },
        {
            JOB_TYPE.CLEANSE_REGION,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.REMOVE_CORRUPTION },
                validRegionGetter = CleanseRegionRegionGetter
            }
        },
        {
            JOB_TYPE.INVADE_REGION,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.INVADE_REGION },
                validRegionGetter = InvadeRegionRegionGetter
            }
        },
        {
            JOB_TYPE.ATTACK_DEMONIC_REGION,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.ATTACK_DEMONIC_REGION },
                validRegionGetter = AttackDemonicRegionRegionGetter
            }
        },
        {
            JOB_TYPE.ATTACK_NON_DEMONIC_REGION,
            new JobWorldEventData() {
                neededEffects = new WORLD_EVENT_EFFECT[]{ WORLD_EVENT_EFFECT.ATTACK_NON_DEMONIC_REGION },
                validRegionGetter = AttackNonDemonicRegionRegionGetter
            }
        },
    };

    #region Region Getters
    public static Region DefaultRegionGetter(Character character, JOB_TYPE jobType) {
        List<Region> choices = new List<Region>();
        //get all valid regions that can spawn the needed effects of the given job
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.CanSpawnNewEvent() && currRegion != character.homeRegion
                && currRegion.mainLandmark.specificLandmarkType !=  LANDMARK_TYPE.THE_PORTAL
                && WorldEventsManager.Instance.CanSpawnEventWithEffects(currRegion, character, WorldEventsManager.Instance.GetNeededEffectsOfJob(jobType))) {
                choices.Add(currRegion);
            }
        }
        if (choices.Count > 0) {
            return Utilities.GetRandomElement(choices);
        }
        return null;
    }
    public static Region ProfaneRegionGetter(Character character, JOB_TYPE jobType) {
        BaseLandmark profane = LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.THE_PROFANE);
        if (profane != null) {
            Region profaneRegion = profane.tileLocation.region;
            if (profaneRegion.CanSpawnNewEvent()
                && WorldEventsManager.Instance.CanSpawnEventWithEffects(profaneRegion, character, WorldEventsManager.Instance.GetNeededEffectsOfJob(jobType))) {
                return profaneRegion;
            }
        }
        //no profane yet
        return null;
    }
    public static Region ReturnHomeRegionGetter(Character character, JOB_TYPE jobType) {
        return character.homeRegion;
    }
    public static Region CleanseRegionRegionGetter(Character character, JOB_TYPE jobType) {
        //Reference: https://trello.com/c/WhHfBH7t/2964-cleanse-region-job
        List<Region> baseChoices = new List<Region>();
        //first only get the regions that can spawn the needed event
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.CanSpawnNewEvent() && currRegion != character.homeRegion
                && currRegion.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL
                && WorldEventsManager.Instance.CanSpawnEventWithEffects(currRegion, character, WorldEventsManager.Instance.GetNeededEffectsOfJob(jobType))) {
                baseChoices.Add(currRegion);
            }
        }
        if (baseChoices.Count > 0) {
            //check if any are adjacent to a region owned by the actor's settlement's ruling faction
            for (int i = 0; i < baseChoices.Count; i++) {
                Region region = baseChoices[i];
                if (region.IsConnectedToRegionOwnedBy(character.faction)) {
                    return region;
                }
            }

            //if none, check for regions that are adjacent to a non-corrupted region
            for (int i = 0; i < baseChoices.Count; i++) {
                Region region = baseChoices[i];
                for (int j = 0; j < region.connections.Count; j++) {
                    Region connection = region.connections[j];
                    if (connection.coreTile.isCorrupted == false) {
                        return region;
                    }
                }
            }

            //if still none, just pick from the base choices
            return Utilities.GetRandomElement(baseChoices);
        }
        return null;
    }
    public static Region InvadeRegionRegionGetter(Character character, JOB_TYPE jobType) {
        List<Region> baseChoices = new List<Region>();
        //first only get the regions that can spawn the needed event
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.owner != null && currRegion.owner != PlayerManager.Instance.player.playerFaction && currRegion.owner != character.homeRegion.owner
                && currRegion.IsConnectedToRegionOwnedBy(character.homeRegion.owner) && currRegion.owner.GetRelationshipWith(character.homeRegion.owner).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE
                && WorldEventsManager.Instance.CanSpawnEventWithEffects(currRegion, character, WorldEventsManager.Instance.GetNeededEffectsOfJob(jobType))) {
                baseChoices.Add(currRegion);
            }
        }
        if (baseChoices.Count > 0) {
            return baseChoices[UnityEngine.Random.Range(0, baseChoices.Count)];
        }
        return null;
    }
    public static Region AttackDemonicRegionRegionGetter(Character character, JOB_TYPE jobType) {
        //TODO:
        //List<Region> baseChoices = new List<Region>();
        ////first only get the regions that can spawn the needed event
        //for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //    Region currRegion = GridMap.Instance.allRegions[i];
        //    if (currRegion.owner != null && currRegion.owner != PlayerManager.Instance.player.playerFaction && currRegion.owner != character.homeRegion.owner
        //        && currRegion.IsConnectedToRegionOwnedBy(character.homeRegion.owner) && currRegion.owner.GetRelationshipWith(character.homeRegion.owner).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE
        //        && WorldEventsManager.Instance.CanSpawnEventWithEffects(currRegion, character, WorldEventsManager.Instance.GetNeededEffectsOfJob(jobType))) {
        //        baseChoices.Add(currRegion);
        //    }
        //}
        //if (baseChoices.Count > 0) {
        //    return baseChoices[UnityEngine.Random.Range(0, baseChoices.Count)];
        //}
        return null;
    }
    public static Region AttackNonDemonicRegionRegionGetter(Character character, JOB_TYPE jobType) {
        //TODO:
        //List<Region> baseChoices = new List<Region>();
        ////first only get the regions that can spawn the needed event
        //for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //    Region currRegion = GridMap.Instance.allRegions[i];
        //    if (currRegion.owner != null && currRegion.owner != PlayerManager.Instance.player.playerFaction && currRegion.owner != character.homeRegion.owner
        //        && currRegion.IsConnectedToRegionOwnedBy(character.homeRegion.owner) && currRegion.owner.GetRelationshipWith(character.homeRegion.owner).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE
        //        && WorldEventsManager.Instance.CanSpawnEventWithEffects(currRegion, character, WorldEventsManager.Instance.GetNeededEffectsOfJob(jobType))) {
        //        baseChoices.Add(currRegion);
        //    }
        //}
        //if (baseChoices.Count > 0) {
        //    return baseChoices[UnityEngine.Random.Range(0, baseChoices.Count)];
        //}
        return null;
    }
    #endregion

}

public class JobWorldEventData {
    public WORLD_EVENT_EFFECT[] neededEffects;
    public System.Func<Character, JOB_TYPE, Region> validRegionGetter;
}
