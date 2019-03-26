using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Explorer : Job {

    INTERACTION_TYPE[] explorerEvents = new INTERACTION_TYPE[] {
        INTERACTION_TYPE.EXPLORER_SPAWN_INTERACTION_1,
        //INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS,
    };

    private int _currentInteractionTick;
    private int _usedMonthTick;

    public Explorer(Character character) : base(character, JOB.EXPLORER) {
        _actionDuration = -1;
        _hasCaptureEvent = true;
        _useInteractionTimer = false;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        Area area = _character.specificLocation;
        string jobSummary = GameManager.Instance.TodayLogString() + " " + _character.name + " job summary: ";

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
        rateWeights.AddElement(RESULT.FAIL, baseFailRate);
        rateWeights.AddElement(RESULT.CRITICAL_FAIL, criticalFailRate);
        jobSummary += "\n" + rateWeights.GetWeightsSummary("Rates summary ");
        if (rateWeights.GetTotalOfWeights() > 0) {
            RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
            jobSummary += "\nRate result: " + chosenResult.ToString() + ".";
            Interaction interaction = null;
            switch (chosenResult) {
                case RESULT.SUCCESS:
                    Interaction createdInteraction = CreateExplorerEvent();
                    if (createdInteraction != null) {
                        createdInteraction.AddEndInteractionAction(() => StartJobAction());
                        createdInteraction.ScheduleSecondTimeOut();
                        _character.AddInteraction(createdInteraction);
                        SetCreatedInteraction(createdInteraction);
                    } else {
                        StartJobAction();
                    }
                    break;
                case RESULT.FAIL:
                    interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation);
                    interaction.AddEndInteractionAction(() => StartJobAction());
                    interaction.ScheduleSecondTimeOut();
                    character.specificLocation.coreTile.landmarkOnTile.AddInteraction(interaction);
                    SetCreatedInteraction(interaction);
                    break;
                case RESULT.CRITICAL_FAIL:
                    interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation);
                    interaction.AddEndInteractionAction(() => StartJobAction());
                    interaction.ScheduleSecondTimeOut();
                    character.specificLocation.coreTile.landmarkOnTile.AddInteraction(interaction);
                    SetCreatedInteraction(interaction);
                    break;
                default:
                    break;
            }
        } else {
            StartJobAction();
        }
        Debug.Log(jobSummary);
    }
    public override void ApplyActionDuration() {
        //_actionDuration = 120 - (3 * (Mathf.Max(_character.level - 5, 0)));
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
        if (_currentInteractionTick == GameManager.Instance.days) {
            if (_usedMonthTick == GameManager.Instance.month) {
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
        _currentInteractionTick = UnityEngine.Random.Range(1, GameManager.daysPerMonth + 1);
    }
    private void GenerateSpawnedInteraction() {
        List<INTERACTION_TYPE> choices = GetValidExplorerEvents();
        if (choices.Count > 0) {
            Area area = _character.specificLocation;
            area.SetStopDefaultInteractionsState(true);
            SetJobActionPauseState(true);
            INTERACTION_TYPE chosenInteractionType = choices[UnityEngine.Random.Range(0, choices.Count)];
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(chosenInteractionType, _character.specificLocation);
            interaction.AddEndInteractionAction(() => SetJobActionPauseState(false));
            interaction.AddEndInteractionAction(() => ForceDefaultAllExistingInteractions());
            _character.specificLocation.coreTile.landmarkOnTile.AddInteraction(interaction);
            SetCreatedInteraction(interaction);
            InteractionUI.Instance.OpenInteractionUI(_createdInteraction);
        }
    }

    private List<INTERACTION_TYPE> GetValidExplorerEvents() {
        List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
        for (int i = 0; i < explorerEvents.Length; i++) {
            INTERACTION_TYPE type = explorerEvents[i];
            if (InteractionManager.Instance.CanCreateInteraction(type, _character.specificLocation)) {
                validTypes.Add(type);
            }
        }
        return validTypes;
    }

    //public Interaction CreateExplorerEvent() {
    //    List<INTERACTION_TYPE> choices = GetValidExplorerEvents();
    //    if (choices.Count > 0) {
    //        Area area = _character.specificLocation;
    //        INTERACTION_TYPE chosenType = choices[Random.Range(0, choices.Count)];
    //        //Get Random Explorer Event
    //        return InteractionManager.Instance.CreateNewInteraction(chosenType, area);
    //    }
    //    return null;
    //}
}
