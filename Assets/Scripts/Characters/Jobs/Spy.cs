using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spy : Job {

    INTERACTION_TYPE[] spySpawnEvents = new INTERACTION_TYPE[] {
        INTERACTION_TYPE.SPY_SPAWN_INTERACTION_1,
        INTERACTION_TYPE.SPY_SPAWN_INTERACTION_2,
        INTERACTION_TYPE.SPY_SPAWN_INTERACTION_3,
        INTERACTION_TYPE.SPY_SPAWN_INTERACTION_4,
    };

    private int _currentInteractionTick;
    private int _usedMonthTick;

    public Spy(Character character) : base(character, JOB.SPY) {
        _actionDuration = -1;
        _hasCaptureEvent = true;
        _useInteractionTimer = false;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        string jobSummary = GameManager.Instance.TodayLogString() + " " + _character.name + " job summary: ";
        //Once the duration expires, check first if there are any new intel that can still be unlocked. Order of priority below:
        List<Token> tokenChoices = new List<Token>();
        Area area = _character.specificLocation.tileLocation.areaOfTile;
        if (!area.locationToken.isObtained) {
            tokenChoices.Add(area.locationToken);
        } else if (area.owner != null && !area.owner.factionToken.isObtained) {
            tokenChoices.Add(area.owner.factionToken);
        } else {
            if (!area.defenderToken.isObtained) {
                tokenChoices.Add(area.defenderToken);
            }
            for (int i = 0; i < area.areaResidents.Count; i++) {
                Character currCharacter = area.areaResidents[i];
                if (!currCharacter.characterToken.isObtained) {
                    tokenChoices.Add(currCharacter.characterToken);
                }
            }
        }

        if (tokenChoices.Count > 0) {
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
                Token chosenToken = tokenChoices[Random.Range(0, tokenChoices.Count)];
                jobSummary += "\nRate result: " + chosenResult.ToString() + ". Chosen token " + chosenToken.ToString();
                Interaction interaction = null;
                switch (chosenResult) {
                    case RESULT.SUCCESS:
                        Success(chosenToken);
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
            jobSummary += "\nNo more token to gain."; 
            StartJobAction();
        }
        Debug.Log(jobSummary);
    }
    public override void ApplyActionDuration() {
        //_actionDuration = 80 - (2 * (Mathf.Max(_character.level - 5, 0)));
        SetCurrentInteractionTick();
    }
    public override int GetSuccessRate() {
        int baseRate = 60;
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        return baseRate + multiplier;
    }
    public override void CaptureRandomLandmarkEvent() {
        if(_currentInteractionTick == GameManager.Instance.days) {
            if(_usedMonthTick == GameManager.Instance.month) {
                return;
            }
            _usedMonthTick = GameManager.Instance.month;
            GenerateSpawnedInteraction();
            SetCurrentInteractionTick();
        }
    }
    #endregion

    private void SetCurrentInteractionTick() {
        int currMonth = _currentInteractionTick;
        currMonth++;
        if (currMonth > 12) {
            currMonth = 1;
        }
        _currentInteractionTick = UnityEngine.Random.Range(1, GameManager.daysInMonth[currMonth] + 1);
    }
    private void GenerateSpawnedInteraction() {
        List<INTERACTION_TYPE> choices = GetValidSpySpawnEvents();
        if(choices.Count > 0) {
            Area area = _character.specificLocation.tileLocation.areaOfTile;
            area.SetStopDefaultInteractionsState(true);
            SetJobActionPauseState(true);
            INTERACTION_TYPE chosenInteractionType = choices[UnityEngine.Random.Range(0, choices.Count)];
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(chosenInteractionType, _character.specificLocation as BaseLandmark);
            interaction.AddEndInteractionAction(() => SetJobActionPauseState(false));
            interaction.AddEndInteractionAction(() => ForceDefaultAllExistingInteractions());
            _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
            SetCreatedInteraction(interaction);
            InteractionUI.Instance.OpenInteractionUI(_createdInteraction);
        }
    }
    private List<INTERACTION_TYPE> GetValidSpySpawnEvents() {
        List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
        for (int i = 0; i < spySpawnEvents.Length; i++) {
            INTERACTION_TYPE type = spySpawnEvents[i];
            if (InteractionManager.Instance.CanCreateInteraction(type, _character.specificLocation as BaseLandmark)) {
                validTypes.Add(type);
            }
        }
        return validTypes;
    }
    private void Success(Token chosenToken) {
        Interaction interaction = null;
        if (chosenToken is FactionToken) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.FACTION_DISCOVERED, character.specificLocation.tileLocation.landmarkOnTile);
        } else if (chosenToken is LocationToken) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.LOCATION_OBSERVED, character.specificLocation.tileLocation.landmarkOnTile);
        } else if (chosenToken is CharacterToken) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_ENCOUNTERED, character.specificLocation.tileLocation.landmarkOnTile);
        } else if (chosenToken is DefenderToken) {
            interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.DEFENDERS_REVEALED, character.specificLocation.tileLocation.landmarkOnTile);
            //_createdInteraction.SetOtherData(new object[] { chosenToken });
        }
        if (interaction != null) {
            interaction.AddEndInteractionAction(() => StartJobAction());
            interaction.ScheduleSecondTimeOut();
            if (interaction.type == INTERACTION_TYPE.CHARACTER_ENCOUNTERED) {
                ((chosenToken as CharacterToken).character).AddInteraction(interaction);
            } else if (interaction.type == INTERACTION_TYPE.LOCATION_OBSERVED
                || interaction.type == INTERACTION_TYPE.DEFENDERS_REVEALED
                || interaction.type == INTERACTION_TYPE.FACTION_DISCOVERED) {
                character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(interaction);
                //(chosenToken as LocationToken).location.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
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
