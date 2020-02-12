using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Archetype {
    public class Normal : PlayerArchetype {
        public Normal() : base(PLAYER_ARCHETYPE.Normal) {
            minionClasses = CharacterManager.sevenDeadlySinsClassNames.ToList();
            afflictions = new List<SPELL_TYPE>() { SPELL_TYPE.PARALYSIS, SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.KLEPTOMANIA, SPELL_TYPE.AGORAPHOBIA, SPELL_TYPE.PSYCHOPATHY, SPELL_TYPE.PESTILENCE, SPELL_TYPE.LYCANTHROPY, SPELL_TYPE.VAMPIRISM, SPELL_TYPE.ZOMBIE_VIRUS, SPELL_TYPE.CURSED_OBJECT, SPELL_TYPE.LULLABY, };
            spells = new List<string>() { PlayerManager.Tornado, PlayerManager.Meteor, PlayerManager.Poison_Cloud, PlayerManager.Lightning, PlayerManager.Ravenous_Spirit, PlayerManager.Feeble_Spirit
                , PlayerManager.Forlorn_Spirit, PlayerManager.Locust_Swarm, PlayerManager.Spawn_Boulder, PlayerManager.Landmine, PlayerManager.Manifest_Food
                , PlayerManager.Brimstones, PlayerManager.Acid_Rain, PlayerManager.Rain, PlayerManager.Heat_Wave, PlayerManager.Wild_Growth
                , PlayerManager.Spider_Rain, PlayerManager.Blizzard, PlayerManager.Earthquake, PlayerManager.Fertility, PlayerManager.Spawn_Bandit_Camp
                , PlayerManager.Spawn_Monster_Lair, PlayerManager.Spawn_Haunted_Grounds };
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_ANVIL, LANDMARK_TYPE.THE_EYE, LANDMARK_TYPE.THE_KENNEL, LANDMARK_TYPE.THE_CRYPT, LANDMARK_TYPE.THE_SPIRE, LANDMARK_TYPE.THE_NEEDLES, LANDMARK_TYPE.THE_PROFANE, LANDMARK_TYPE.THE_PIT, LANDMARK_TYPE.GOADER };
            SetCanTriggerFlaw(true);
            SetCanRemoveTraits(true);
        }

        #region Overrides
        public override bool CanAfflict(SPELL_TYPE type) {
            return true;
        }
        public override bool CanDoAction(string actionName) {
            return true;
        }
        public override bool CanSummonMinion(Minion minion) {
            return true;
        }
        public override bool CanBuildDemonicStructure(LANDMARK_TYPE type) {
            return true;
        }
        public override bool CanCastSpell(string spellName) {
            return true;
        }
        #endregion
    }
}