using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistDiscovered : WorldEvent {

    public CultistDiscovered() : base(WORLD_EVENT.CULTIST_DISCOVERED) {
        duration = 1 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        CultistDiscoveredData data = (CultistDiscoveredData)landmark.eventData;
        //- after effect: cultist will be slain.
        data.cultist.Death("attacked", responsibleCharacter: data.nonCultist);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        log.AddToFillers(data.cultist, data.cultist.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(data.nonCultist, data.nonCultist.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        //- requirement: non-Cultist soldier or adventurer resident + Cultist resident + demon cult is active
        bool hasNonCultist = false;
        bool hasCultist = false;
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currCharacter = landmark.charactersHere[i];
            if (currCharacter.GetNormalTrait("Cultist") != null) {
                //cultist
                hasCultist = true;
            } else {
                //non cultist
                if (currCharacter.role.roleType == CHARACTER_ROLE.SOLDIER || currCharacter.role.roleType == CHARACTER_ROLE.ADVENTURER) {
                    hasNonCultist = true;
                }
            }
            if (hasCultist && hasNonCultist) {
                break;
            }
        }
        return hasCultist && hasNonCultist && StoryEventsManager.Instance.isCultActive;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currCharacter = landmark.charactersHere[i];
            if (currCharacter.GetNormalTrait("Cultist") != null) {
                //cultist
                return currCharacter;
            } 
            //else {
            //    //non cultist
            //    if (currCharacter.role.roleType == CHARACTER_ROLE.SOLDIER || currCharacter.role.roleType == CHARACTER_ROLE.ADVENTURER) {
            //        return currCharacter;
            //    }
            //}
        }
        return null;
    }
    public override IWorldEventData ConstructEventDataForLandmark(BaseLandmark landmark) {
        Character chosenCultist = null;
        Character chosenNonCultist = null;
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currCharacter = landmark.charactersHere[i];
            if (currCharacter.GetNormalTrait("Cultist") != null) {
                //cultist
                if (chosenCultist == null) {
                    chosenCultist = currCharacter;
                }
            } else {
                //non cultist
                if (currCharacter.role.roleType == CHARACTER_ROLE.SOLDIER || currCharacter.role.roleType == CHARACTER_ROLE.ADVENTURER) {
                    if (chosenNonCultist == null) {
                        chosenNonCultist = currCharacter;
                    }
                }
            }
            if (chosenCultist != null && chosenNonCultist != null) {
                break;
            }
        }
        IWorldEventData data = new CultistDiscoveredData() {
            cultist = chosenCultist,
            nonCultist = chosenNonCultist,
        };
        return data;
    }
    #endregion
}

public struct CultistDiscoveredData : IWorldEventData {
    public Character cultist;
    public Character nonCultist;

    public Character[] involvedCharacters { get { return new Character[] { cultist, nonCultist }; } }
}