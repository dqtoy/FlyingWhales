using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class PuppetMaster : PlayerArchetype {
        public PuppetMaster() : base(PLAYER_ARCHETYPE.Puppet_Master) {
            actions = new List<string>() { PlayerManager.Afflict_Action, PlayerManager.Bless_Action, PlayerManager.Zap_Action, PlayerManager.Booby_Trap, };
            monsters = new List<RaceClass> { }; //No initial monsters
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_EYE, LANDMARK_TYPE.GOADER, };
            minionClasses = new List<string>() { "Lust", "Envy", "Greed", "Gluttony" };
            afflictions = new List<SPELL_TYPE>() { SPELL_TYPE.PARALYSIS, SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.KLEPTOMANIA, SPELL_TYPE.AGORAPHOBIA, SPELL_TYPE.PSYCHOPATHY, /*Pyromania, Cowardice, Bewitch*/ };
            spells = new List<string>() { PlayerManager.Ravenous_Spirit, PlayerManager.Manifest_Food, PlayerManager.Spider_Rain, PlayerManager.Wild_Growth, PlayerManager.Fertility, PlayerManager.Spawn_Bandit_Camp };
            SetCanTriggerFlaw(true);
            SetCanRemoveTraits(true);
        }
    }
}