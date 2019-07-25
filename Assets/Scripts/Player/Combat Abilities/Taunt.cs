using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taunt : CombatAbility {

    private int _hpGain;
    public Taunt() : base(COMBAT_ABILITY.TAUNT) {
        description = "Taunts enemies into attacking this character.";
        cooldown = 10;
        _currentCooldown = 10;
    }

    #region Overrides
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;
            if (character.faction == PlayerManager.Instance.player.playerFaction) {
                return true;
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            _hpGain = 1000;
        } else if (lvl == 2) {
            _hpGain = 1500;
        } else if (lvl == 3) {
            _hpGain = 2000;
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;

            character.AdjustHP(1000);
            List<LocationGridTile> tilesInRadius = character.gridTileLocation.parentAreaMap.GetTilesInRadius(character.gridTileLocation, 3, includeCenterTile: true, includeTilesInDifferentStructure: true);
            List<Character> affectedByTaunt = new List<Character>();
            for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
                Character currCharacter = character.specificLocation.charactersAtLocation[i];
                if (currCharacter != character && currCharacter.faction != PlayerManager.Instance.player.playerFaction) {
                    if (tilesInRadius.Contains(currCharacter.gridTileLocation) && character.marker.IsCharacterInLineOfSightWith(currCharacter)) {
                        affectedByTaunt.Add(currCharacter);
                    }
                }
            }

            for (int i = 0; i < affectedByTaunt.Count; i++) {
                if(affectedByTaunt[i].stateComponent.currentState != null && affectedByTaunt[i].stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    CombatState combatState = affectedByTaunt[i].stateComponent.currentState as CombatState;
                    combatState.SwitchTarget(character);
                } else {
                    affectedByTaunt[i].marker.AddHostileInRange(character, checkHostility: false);
                }
            }
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
