using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class Ravager : PlayerArchetype {
        public Ravager() : base(PLAYER_ARCHETYPE.Ravager) {
            actions = new List<string>() { PlayerManager.Afflict_Action, PlayerManager.Seize_Object_Action, PlayerManager.Destroy_Action
                , PlayerManager.Ignite_Action, PlayerManager.Summon_Minion_Action, PlayerManager.Stop_Action, PlayerManager.Return_To_Portal_Action };
            monsters = new List<RaceClass> { new RaceClass(RACE.WOLF, "Ravager"), new RaceClass(RACE.GOLEM, "Golem") };
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_PIT, LANDMARK_TYPE.THE_KENNEL, };
            minionClasses = new List<string>() { "Pride", "Envy", "Greed", "Wrath" };
            afflictions = new List<SPELL_TYPE>() { }; //No intial afflictions
            spells = new List<string>() { PlayerManager.Tornado, PlayerManager.Poison_Cloud, PlayerManager.Meteor, PlayerManager.Lightning
                , PlayerManager.Feeble_Spirit, PlayerManager.Locust_Swarm, PlayerManager.Locust_Swarm, PlayerManager.Spawn_Boulder, PlayerManager.Landmine
                , PlayerManager.Acid_Rain, PlayerManager.Rain, PlayerManager.Heat_Wave, PlayerManager.Earthquake, PlayerManager.Spawn_Monster_Lair };
            SetCanTriggerFlaw(false);
            SetCanRemoveTraits(false);
        }
    }
}