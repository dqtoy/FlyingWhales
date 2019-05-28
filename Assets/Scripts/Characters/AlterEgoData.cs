using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlterEgoData {

    //Basic Data
    public Character owner { get; private set; }
    public Faction faction { get; private set; }
    public RACE race { get; private set; }
    public CharacterRole role { get; private set; }
    public CharacterClass characterClass { get; private set; }
    public Dwelling homeSturcture { get; private set; }

    //Awareness
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness { get; private set; }

    //Relationships
	public Dictionary<Character, CharacterRelationshipData> relationships { get; private set; }

    public AlterEgoData(Character owner) {
        this.owner = owner;
        faction = null;
        relationships = new Dictionary<Character, CharacterRelationshipData>();
    }
}
