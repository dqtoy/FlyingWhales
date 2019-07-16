using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearSpellAbility : CombatAbility {

    private int _fearDurationInMinutes;
    public FearSpellAbility() : base(COMBAT_ABILITY.FEAR_SPELL) {
        abilityTags.Add(ABILITY_TAG.MAGIC);
        abilityTags.Add(ABILITY_TAG.DEBUFF);
        cooldown = 10;
        _currentCooldown = 10;
    }

    #region Overrides
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;
            if (character.faction != PlayerManager.Instance.player.playerFaction) {
                return true;
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            _fearDurationInMinutes = 30;
        } else if (lvl == 2) {
            _fearDurationInMinutes = 60;
        } else if (lvl == 3) {
            _fearDurationInMinutes = 90;
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;

            if (character.currentAction != null) {
                character.currentAction.StopAction();
            } else if (character.currentParty.icon.isTravelling) {
                if (character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovement();
                } else {
                    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                }
            }

            if (!character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                if (character.marker.inVisionPOIs.Count > 0) {
                    List<Character> terrifyingCharacters = new List<Character>();
                    for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                        if (character.marker.inVisionPOIs[i] is Character) {
                            Character characterInVision = character.marker.inVisionPOIs[i] as Character;
                            //AddHostileInRange(characterInVision, CHARACTER_STATE.COMBAT, false);
                            terrifyingCharacters.Add(characterInVision);
                        }
                    }
                    if (terrifyingCharacters.Count > 0) {
                        if ((character.GetNormalTrait("Berserked") != null)
                            || (character.stateComponent.stateToDo != null && character.stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED && !character.stateComponent.stateToDo.isDone)) {
                            //If berserked
                        } else {
                            character.marker.AddAvoidsInRange(terrifyingCharacters, false);
                            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, character);
                        }
                    }
                }
            }
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
