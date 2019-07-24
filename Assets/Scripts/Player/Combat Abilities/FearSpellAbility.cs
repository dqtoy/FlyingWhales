using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearSpellAbility : CombatAbility {

    private int _fearDurationInMinutes;
    public FearSpellAbility() : base(COMBAT_ABILITY.FEAR_SPELL) {
        description = "Makes a character fear any other character";
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
            Spooked spooked = character.GetNormalTrait("Spooked") as Spooked;
            if(spooked == null) {
                Spooked newTrait = new Spooked();
                newTrait.OverrideDuration(GameManager.Instance.GetTicksBasedOnMinutes(_fearDurationInMinutes));
                character.AddTrait(newTrait);
            }
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
