using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Explorer : Job {

    INTERACTION_TYPE[] explorerEvents = new INTERACTION_TYPE[] { //TODO: Put this somwhere else
        INTERACTION_TYPE.INDUCE_WAR,
        INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS,
    };

    public Explorer(Character character) : base(character, JOB.EXPLORER) {
        _actionDuration = 120;
        _hasCaptureEvent = false;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        Area area = _character.specificLocation.tileLocation.areaOfTile;
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
                    List<INTERACTION_TYPE> choices = GetValidExplorerEvents();
                    if (choices.Count > 0) {
                        INTERACTION_TYPE chosenType = choices[Random.Range(0, choices.Count)];
                        //Get Random Chaos Event
                        interaction = InteractionManager.Instance.CreateNewInteraction(chosenType, area.coreTile.landmarkOnTile);
                        //SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER, area.coreTile.landmarkOnTile)); //NOT FINAL!
                        interaction.AddEndInteractionAction(() => StartJobAction());
                        interaction.ScheduleSecondTimeOut();
                        _character.AddInteraction(interaction);
                        SetCreatedInteraction(interaction);
                    } else {
                        StartJobAction();
                    }
                    break;
                case RESULT.FAIL:
                    interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile);
                    interaction.AddEndInteractionAction(() => StartJobAction());
                    interaction.ScheduleSecondTimeOut();
                    character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(interaction);
                    SetCreatedInteraction(interaction);
                    break;
                case RESULT.CRITICAL_FAIL:
                    interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile);
                    interaction.AddEndInteractionAction(() => StartJobAction());
                    interaction.ScheduleSecondTimeOut();
                    character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(interaction);
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
        _actionDuration = 120 - (3 * (Mathf.Max(_character.level - 5, 0)));
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

    private List<INTERACTION_TYPE> GetValidExplorerEvents() {
        List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
        for (int i = 0; i < explorerEvents.Length; i++) {
            INTERACTION_TYPE type = explorerEvents[i];
            if (InteractionManager.Instance.CanCreateInteraction(type, _character)) {
                validTypes.Add(type);
            }
        }
        return validTypes;
    }
}
