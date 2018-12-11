﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spy : Job {

    //INTERACTION_TYPE[] explorerEvents = new INTERACTION_TYPE[] { //TODO: Put this somwhere else
        
    //};

    public Spy(Character character) : base(character, JOB.SPY) {
        _actionDuration = 80;
        _hasCaptureEvent = false;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        string jobSummary = GameManager.Instance.TodayLogString() + " " + _character.name + " job summary: ";
        //Once the duration expires, check first if there are any new intel that can still be unlocked. Order of priority below:
        List<Intel> intelChoices = new List<Intel>();
        Area area = _character.specificLocation.tileLocation.areaOfTile;
        if (!area.locationIntel.isObtained) {
            intelChoices.Add(area.locationIntel);
        } else if (area.owner != null && !area.owner.factionIntel.isObtained) {
            intelChoices.Add(area.owner.factionIntel);
        } else {
            if (!area.defenderIntel.isObtained) {
                intelChoices.Add(area.defenderIntel);
            }
            for (int i = 0; i < area.areaResidents.Count; i++) {
                Character currCharacter = area.areaResidents[i];
                if (!currCharacter.characterIntel.isObtained) {
                    intelChoices.Add(currCharacter.characterIntel);
                }
            }
        }

        if (intelChoices.Count > 0) {
            int baseSuccessRate = 50;
            int baseFailRate = 40;
            int criticalFailRate = 12;

            //Success Rate +1 per level starting at Level 6
            baseSuccessRate += (Mathf.Max(character.level - 5, 0));
            //Critical Fail Rate -1 per mult of 4 level starting at Level 6
            if (character.level > 6) {
                criticalFailRate -= Mathf.FloorToInt(character.level / 4);
            }

            WeightedDictionary<RESULT> rateWeights = new WeightedDictionary<RESULT>();
            rateWeights.AddElement(RESULT.SUCCESS, baseSuccessRate);
            //rateWeights.AddElement(RESULT.FAIL, baseFailRate);
            //rateWeights.AddElement(RESULT.CRITICAL_FAIL, criticalFailRate);
            jobSummary += "\n" + rateWeights.GetWeightsSummary("Rates summary ");
            if (rateWeights.GetTotalOfWeights() > 0) {
                RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
                Intel chosenIntel = intelChoices[Random.Range(0, intelChoices.Count)];
                jobSummary += "\nRate result: " + chosenResult.ToString() + ". Chosen intel " + chosenIntel.ToString();
                Interaction interaction = null;
                switch (chosenResult) {
                    case RESULT.SUCCESS:
                        Success(chosenIntel);
                        break;
                    case RESULT.FAIL:
                        interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile);
                        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
                        interaction.AddEndInteractionAction(() => StartJobAction());
                        interaction.ScheduleSecondTimeOut();
                        character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(interaction);
                        SetCreatedInteraction(interaction);
                        //Interaction minionFailed = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile);
                        //minionFailed.SetEndInteractionAction(() => StartJobAction());
                        //minionFailed.ScheduleSecondTimeOut();
                        //character.specificLocation.tileLocation.areaOfTile.areaInvestigation.SetCurrentInteraction(minionFailed);
                        //StartJobAction();
                        break;
                    case RESULT.CRITICAL_FAIL:
                        interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile);
                        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
                        interaction.AddEndInteractionAction(() => StartJobAction());
                        interaction.ScheduleSecondTimeOut();
                        character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(interaction);
                        SetCreatedInteraction(interaction);

                        //Interaction minionCriticalFail = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile);
                        //minionCriticalFail.SetEndInteractionAction(() => StartJobAction());
                        //minionCriticalFail.ScheduleSecondTimeOut();
                        //character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(minionCriticalFail);
                        //character.specificLocation.tileLocation.areaOfTile.areaInvestigation.SetCurrentInteraction(minionCriticalFail);
                        //StartJobAction();
                        break;
                    default:
                        break;
                }
            } else {
                StartJobAction();
            }
        } else {
            jobSummary += "\nNo more intel to gain."; 
            StartJobAction();
        }
        Debug.Log(jobSummary);
    }
    public override void ApplyActionDuration() {
        _actionDuration = 80 - (2 * (Mathf.Max(_character.level - 5, 0)));
    }
    public override int GetSuccessRate() {
        int baseRate = 60;
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        return baseRate + multiplier;
    }
    #endregion

    private void Success(Intel chosenIntel) {
        Interaction interaction = null;
        if (chosenIntel is FactionIntel) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.FACTION_DISCOVERED, character.specificLocation.tileLocation.landmarkOnTile);
        } else if (chosenIntel is LocationIntel) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.LOCATION_OBSERVED, character.specificLocation.tileLocation.landmarkOnTile);
        } else if (chosenIntel is CharacterIntel) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_ENCOUNTERED, character.specificLocation.tileLocation.landmarkOnTile);
        } else if (chosenIntel is DefenderIntel) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.DEFENDERS_REVEALED, character.specificLocation.tileLocation.landmarkOnTile);
            //_createdInteraction.SetOtherData(new object[] { chosenIntel });
        }
        if (interaction != null) {
            interaction.AddEndInteractionAction(() => StartJobAction());
            interaction.ScheduleSecondTimeOut();
            if (interaction.type == INTERACTION_TYPE.CHARACTER_ENCOUNTERED) {
                ((chosenIntel as CharacterIntel).character).AddInteraction(interaction);
            } else if (interaction.type == INTERACTION_TYPE.LOCATION_OBSERVED
                || interaction.type == INTERACTION_TYPE.DEFENDERS_REVEALED
                || interaction.type == INTERACTION_TYPE.FACTION_DISCOVERED) {
                character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(interaction);
                //(chosenIntel as LocationIntel).location.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
            }
            SetCreatedInteraction(interaction);
            //character.specificLocation.tileLocation.areaOfTile.areaInvestigation.SetCurrentInteraction(interaction);
        }
        
    }

    //private List<INTERACTION_TYPE> GetValidExplorerEvents() {
    //    List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
    //    for (int i = 0; i < explorerEvents.Length; i++) {
    //        INTERACTION_TYPE type = explorerEvents[i];
    //        if (InteractionManager.Instance.CanCreateInteraction(type, _character)) {
    //            validTypes.Add(type);
    //        }
    //    }
    //    return validTypes;
    //}

    //public Interaction CreateExplorerEvent() {
    //    List<INTERACTION_TYPE> choices = GetValidExplorerEvents();
    //    if (choices.Count > 0) {
    //        Area area = _character.specificLocation.tileLocation.areaOfTile;
    //        INTERACTION_TYPE chosenType = choices[Random.Range(0, choices.Count)];
    //        //Get Random Explorer Event
    //        return InteractionManager.Instance.CreateNewInteraction(chosenType, area.coreTile.landmarkOnTile);
    //    }
    //    return null;
    //}
}
