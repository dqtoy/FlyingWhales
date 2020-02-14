using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class Ravager : PlayerArchetype {
        public Ravager() : base(PLAYER_ARCHETYPE.Ravager) {
            actions = new List<string>() { PlayerDB.Afflict_Action, PlayerDB.Seize_Object_Action, PlayerDB.Destroy_Action, PlayerDB.Ignite_Action, PlayerDB.Summon_Minion_Action
                    , PlayerDB.Corrupt_Action, PlayerDB.Stop_Action, PlayerDB.Return_To_Portal_Action, PlayerDB.Build_Demonic_Structure_Action, PlayerDB.Breed_Monster_Action};
            monsters = new List<RaceClass> { new RaceClass(RACE.WOLF, "Ravager"), new RaceClass(RACE.GOLEM, "Golem") };
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_PIT, LANDMARK_TYPE.THE_KENNEL };
            minionClasses = new List<string>() { "Pride", "Envy", "Greed", "Wrath" };
            afflictions = new List<SPELL_TYPE>() { }; //No intial afflictions
            spells = new List<string>() { PlayerDB.Tornado, PlayerDB.Poison_Cloud, PlayerDB.Meteor, PlayerDB.Lightning
                , PlayerDB.Feeble_Spirit, PlayerDB.Locust_Swarm, PlayerDB.Locust_Swarm, PlayerDB.Spawn_Boulder, PlayerDB.Landmine
                , PlayerDB.Acid_Rain, PlayerDB.Rain, PlayerDB.Heat_Wave, PlayerDB.Earthquake, PlayerDB.Spawn_Monster_Lair };
            SetCanTriggerFlaw(false);
            SetCanRemoveTraits(false);
        }
    }
}