using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class Lich : PlayerArchetype {
        public Lich() : base(PLAYER_ARCHETYPE.Lich) {
            actions = new List<string>() { PlayerManager.Afflict_Action, PlayerManager.Poison_Action
                , PlayerManager.Seize_Object_Action, PlayerManager.Animate_Action, PlayerManager.Summon_Minion_Action, PlayerManager.Stop_Action, PlayerManager.Return_To_Portal_Action };
            monsters = new List<RaceClass> { new RaceClass(RACE.SKELETON, "Archer"), new RaceClass(RACE.SKELETON, "Marauder") };
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_PROFANE, LANDMARK_TYPE.THE_KENNEL, LANDMARK_TYPE.TORTURE_CHAMBER };
            minionClasses = new List<string>() { "Pride", "Sloth", "Lust", "Gluttony" };
            afflictions = new List<SPELL_TYPE>() { SPELL_TYPE.PESTILENCE, SPELL_TYPE.LYCANTHROPY, SPELL_TYPE.VAMPIRISM, SPELL_TYPE.ZOMBIE_VIRUS, SPELL_TYPE.CURSED_OBJECT, SPELL_TYPE.LULLABY, /*Befoul, Imp Seed*/ };
            spells = new List<string>() { PlayerManager.Forlorn_Spirit, PlayerManager.Brimstones, PlayerManager.Spider_Rain, PlayerManager.Blizzard, PlayerManager.Spawn_Haunted_Grounds };
            SetCanTriggerFlaw(false);
            SetCanRemoveTraits(false);
        }
    }
}