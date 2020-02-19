using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Provoke : PlayerSpell {

    public Provoke() : base(SPELL_TYPE.PROVOKE) {
        tier = 2;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
        //abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        Character minionActor = null;
        if(PlayerManager.Instance.player.minions.Count > 0) {
            minionActor = PlayerManager.Instance.player.minions[0].character;
        }
        PlayerUI.Instance.OpenProvoke(minionActor, targetCharacter);
        base.ActivateAction(targetCharacter);
    }

    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(targetCharacter.race) || targetCharacter.isFactionless) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Unconscious")) {
            return false;
        }
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(targetCharacter.race) || targetCharacter.isFactionless) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Unconscious")) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class ProvokeData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.PROVOKE;
    public override string name { get { return "Provoke"; } }
    public override string description { get { return "Makes a character undermine his/her enemies."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }

    public ProvokeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}