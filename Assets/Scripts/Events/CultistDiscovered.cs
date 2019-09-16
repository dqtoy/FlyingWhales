using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistDiscovered : WorldEvent {

    public CultistDiscovered() : base(WORLD_EVENT.CULTIST_DISCOVERED) {
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region) {
        base.ExecuteAfterEffect(region);
        CultistDiscoveredData data = (CultistDiscoveredData)region.eventData;
        //- after effect: cultist will be slain.
        data.cultist.Death("attacked", responsibleCharacter: data.nonCultist);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        log.AddToFillers(data.cultist, data.cultist.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(data.nonCultist, data.nonCultist.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(Region region) {
        //- requirement: non-Cultist soldier or adventurer resident + Cultist resident + demon cult is active
        bool hasNonCultist = false;
        bool hasCultist = false;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character currCharacter = region.charactersAtLocation[i];
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
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character currCharacter = region.charactersAtLocation[i];
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
    public override IWorldEventData ConstructEventDataForLandmark(Region region) {
        Character chosenCultist = null;
        Character chosenNonCultist = null;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character currCharacter = region.charactersAtLocation[i];
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

public class CultistDiscoveredData : IWorldEventData {
    public Character cultist;
    public Character nonCultist;

    public Character[] involvedCharacters { get { return new Character[] { cultist, nonCultist }; } }

    public Character interferingCharacter { get; private set; }

    public GameDate endDate { get; private set; }
    public GameDate startDate { get; private set; }

    public void SetEndDate(GameDate date) {
        endDate = date;
    }
    public void SetStartDate(GameDate date) {
        startDate = date;
    }

    public void SetInterferingCharacter(Character character) {
        interferingCharacter = character;
    }
}

public class SaveDataCultistDiscoveredData : SaveDataWorldEventData {
    public int cultistID;
    public int nonCultistID;

    public override void Save(IWorldEventData eventData) {
        base.Save(eventData);
        if (eventData is CultistDiscoveredData) {
            CultistDiscoveredData data = (CultistDiscoveredData) eventData;
            if (data.cultist != null) {
                cultistID = data.cultist.id;
            } else {
                cultistID = -1;
            }

            if (data.nonCultist != null) {
                nonCultistID = data.nonCultist.id;
            } else {
                nonCultistID = -1;
            }
        }
    }

    public override IWorldEventData Load() {
        CultistDiscoveredData worldEventData = new CultistDiscoveredData() {
            cultist = CharacterManager.Instance.GetCharacterByID(cultistID),
            nonCultist = CharacterManager.Instance.GetCharacterByID(nonCultistID),
        };
        worldEventData.SetEndDate(new GameDate(endMonth, endDay, endYear, endTick));
        worldEventData.SetStartDate(new GameDate(currentMonth, currentDay, currentYear, currentTick));

        return worldEventData;
    }
}