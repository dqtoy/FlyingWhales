﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Archetype {
    public class Normal : PlayerArchetype {
        public Normal() : base(PLAYER_ARCHETYPE.Normal) {
            minionClasses = CharacterManager.sevenDeadlySinsClassNames.ToList();
            afflictions = PlayerDB.afflictions;
            spells = PlayerDB.spells;
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_ANVIL, LANDMARK_TYPE.THE_EYE, LANDMARK_TYPE.THE_KENNEL, LANDMARK_TYPE.THE_CRYPT, LANDMARK_TYPE.THE_SPIRE, LANDMARK_TYPE.THE_NEEDLES, LANDMARK_TYPE.THE_PROFANE, LANDMARK_TYPE.THE_PIT, LANDMARK_TYPE.GOADER, LANDMARK_TYPE.TORTURE_CHAMBER };
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