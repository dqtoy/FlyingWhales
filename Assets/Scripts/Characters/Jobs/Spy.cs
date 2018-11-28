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
            if (rateWeights.GetTotalOfWeights() > 0) {
                JOB_RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
                Intel chosenIntel = intelChoices[Random.Range(0, intelChoices.Count)];
                switch (chosenResult) {
                    case JOB_RESULT.SUCCESS:
                        Success(chosenIntel);
                        break;
                    case JOB_RESULT.FAIL:
                        Interaction minionFailed = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile);
                        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
                        minionFailed.ScheduleSecondTimeOut();
                        //StartJobAction();
                        break;
                    case JOB_RESULT.CRITICAL_FAIL:
                        Interaction minionCriticalFail = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile);
                        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
                        minionCriticalFail.ScheduleSecondTimeOut();
                        //StartJobAction();
                        break;
                    default:
                        break;
                }
            } else {
                StartJobAction();
            }
        } else {
            StartJobAction();
        }
    }
    public override void ApplyActionDuration() {
        _actionDuration = 80 - (2 * (_character.level - 5));
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
        }
        if (interaction != null) {
            interaction.SetEndInteractionAction(() => StartJobAction());
            interaction.ScheduleSecondTimeOut();
        }
        
    }
}
