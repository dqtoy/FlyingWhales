using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Spy : Job {

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
            for (int i = 0; i < area.areaResidents.Count; i++) {
                Character currCharacter = area.areaResidents[i];
                if (!currCharacter.characterIntel.isObtained) {
                    intelChoices.Add(currCharacter.characterIntel);
                }
            }
            for (int i = 0; i < area.defenderGroups.Count; i++) {
                DefenderGroup currGroup = area.defenderGroups[i];
                if (!currGroup.intel.isObtained) {
                    intelChoices.Add(currGroup.intel);
                }
            }
        }

        if (intelChoices.Count > 0) {
            int baseSuccessRate = 50;
            int baseFailRate = 40;
            int criticalFailRate = 12;

            //Success Rate +1 per level starting at Level 6
            baseSuccessRate += (character.level - 5);
            //Critical Fail Rate -1 per mult of 4 level starting at Level 6
            if (character.level > 6) {
                criticalFailRate -= Mathf.FloorToInt(character.level / 4);
            }

            WeightedDictionary<JOB_RESULT> rateWeights = new WeightedDictionary<JOB_RESULT>();
            rateWeights.AddElement(JOB_RESULT.SUCCESS, baseSuccessRate);
            rateWeights.AddElement(JOB_RESULT.FAIL, baseFailRate);
            rateWeights.AddElement(JOB_RESULT.CRITICAL_FAIL, criticalFailRate);
            jobSummary += "\n" + rateWeights.GetWeightsSummary("Rates summary ");
            if (rateWeights.GetTotalOfWeights() > 0) {
                JOB_RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
                Intel chosenIntel = intelChoices[Random.Range(0, intelChoices.Count)];
                jobSummary += "\nRate result: " + chosenResult.ToString() + ". Chosen intel " + chosenIntel.ToString();
                switch (chosenResult) {
                    case JOB_RESULT.SUCCESS:
                        Success(chosenIntel);
                        break;
                    case JOB_RESULT.FAIL:
                        SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile));
                        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
                        _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                        _createdInteraction.ScheduleSecondTimeOut();
                        character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
                        //Interaction minionFailed = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile);
                        //minionFailed.SetEndInteractionAction(() => StartJobAction());
                        //minionFailed.ScheduleSecondTimeOut();
                        //character.specificLocation.tileLocation.areaOfTile.areaInvestigation.SetCurrentInteraction(minionFailed);
                    //StartJobAction();
                    break;
                    case JOB_RESULT.CRITICAL_FAIL:
                        SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile));
                        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
                        _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                        _createdInteraction.ScheduleSecondTimeOut();
                        character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
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
    #endregion

    private void Success(Intel chosenIntel) {
        if (chosenIntel is FactionIntel) {
            SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.FACTION_DISCOVERED, character.specificLocation.tileLocation.landmarkOnTile));
        } else if (chosenIntel is LocationIntel) {
            SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.LOCATION_OBSERVED, character.specificLocation.tileLocation.landmarkOnTile));
        } else if (chosenIntel is CharacterIntel) {
            SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_ENCOUNTERED, character.specificLocation.tileLocation.landmarkOnTile));
        } else if (chosenIntel is DefenderIntel) {
            SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.DEFENDERS_REVEALED, character.specificLocation.tileLocation.landmarkOnTile));
            _createdInteraction.SetOtherData(new object[] { chosenIntel });
        }
        if (_createdInteraction != null) {
            _createdInteraction.SetEndInteractionAction(() => StartJobAction());
            _createdInteraction.ScheduleSecondTimeOut();
            if (_createdInteraction.type == INTERACTION_TYPE.CHARACTER_ENCOUNTERED) {
                ((chosenIntel as CharacterIntel).character as Character).AddInteraction(_createdInteraction);
            } else if (_createdInteraction.type == INTERACTION_TYPE.LOCATION_OBSERVED
                || _createdInteraction.type == INTERACTION_TYPE.DEFENDERS_REVEALED
                || _createdInteraction.type == INTERACTION_TYPE.FACTION_DISCOVERED) {
                character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
                //(chosenIntel as LocationIntel).location.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
            }
            //character.specificLocation.tileLocation.areaOfTile.areaInvestigation.SetCurrentInteraction(interaction);
        }
        
    }
}
