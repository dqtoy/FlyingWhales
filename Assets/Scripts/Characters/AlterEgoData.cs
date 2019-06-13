using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlterEgoData {

    //Basic Data
    public Character owner { get; private set; }
    public string name { get; private set; }
    public Faction faction { get; private set; }
    public RACE race { get; private set; }
    public CharacterRole role { get; private set; }
    public CharacterClass characterClass { get; private set; }
    public Dwelling homeSturcture { get; private set; }

    //Awareness
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness { get; private set; }

    //Relationships
	public Dictionary<AlterEgoData, CharacterRelationshipData> relationships { get; private set; }

    //Traits
    public List<Trait> traits { get; private set; }

    public AlterEgoData(Character owner, string name) {
        this.owner = owner;
        this.name = name;
        faction = null;
        race = RACE.NONE;
        role = null;
        characterClass = null;
        homeSturcture = null;
        awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>>();
        relationships = new Dictionary<AlterEgoData, CharacterRelationshipData>();
        traits = new List<Trait>();
    }

    public void SetFaction(Faction faction) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.faction = faction;
    }
    public void SetRace(RACE race) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.race = race;
    }
    public void SetRole(CharacterRole role) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.role = role;
    }
    public void SetCharacterClass(CharacterClass characterClass) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.characterClass = characterClass;
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.homeSturcture = homeStructure;
    }

    #region Awareness
    public void SetAwareness(Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.awareness = awareness;
    }
    public IAwareness AddAwareness(IPointOfInterest pointOfInterest) {
        IAwareness iawareness = GetAwareness(pointOfInterest);
        if (iawareness == null) {
            iawareness = CreateNewAwareness(pointOfInterest);
            if (iawareness != null) {
                if (!awareness.ContainsKey(pointOfInterest.poiType)) {
                    awareness.Add(pointOfInterest.poiType, new List<IAwareness>());
                }
                awareness[pointOfInterest.poiType].Add(iawareness);
                iawareness.OnAddAwareness(owner);
            }

            if (pointOfInterest is TreeObject) {
                List<IAwareness> treeAwareness = GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE.TREE);
                if (treeAwareness.Count >= Character.TREE_AWARENESS_LIMIT) {
                    RemoveAwareness(treeAwareness[0].poi);
                }
            }
        } else {
            if (pointOfInterest.gridTileLocation != null) {
                //if already has awareness for that poi, just update it's known location. 
                //Except if it's null, because if it's null, it ususally means the poi is travelling 
                //and setting it's location to null should be ignored to prevent unexpected behaviour.
                iawareness.SetKnownGridLocation(pointOfInterest.gridTileLocation);
            }

        }
        return iawareness;
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        if (awareness.ContainsKey(pointOfInterest.poiType)) {
            List<IAwareness> awarenesses = awareness[pointOfInterest.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IAwareness iawareness = awarenesses[i];
                if (iawareness.poi == pointOfInterest) {
                    awarenesses.RemoveAt(i);
                    iawareness.OnRemoveAwareness(owner);
                    break;
                }
            }
        }
    }
    public void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType) {
        if (awareness.ContainsKey(poiType)) {
            List<IAwareness> awarenesses = new List<IAwareness>(awareness[poiType]);
            for (int i = 0; i < awarenesses.Count; i++) {
                IAwareness iawareness = awarenesses[i];
                awareness[poiType].Remove(iawareness);
                iawareness.OnRemoveAwareness(owner);
            }
            awareness.Remove(poiType);
        }
    }
    public IAwareness GetAwareness(IPointOfInterest poi) {
        if (awareness.ContainsKey(poi.poiType)) {
            List<IAwareness> awarenesses = awareness[poi.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IAwareness iawareness = awarenesses[i];
                if (iawareness.poi == poi) {
                    return iawareness;
                }
            }
            return null;
        }
        return null;
    }
    public List<IAwareness> GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE tileObjectType) {
        List<IAwareness> objects = new List<IAwareness>();
        if (awareness.ContainsKey(POINT_OF_INTEREST_TYPE.TILE_OBJECT)) {
            List<IAwareness> awarenesses = awareness[POINT_OF_INTEREST_TYPE.TILE_OBJECT];
            for (int i = 0; i < awarenesses.Count; i++) {
                TileObjectAwareness iawareness = awarenesses[i] as TileObjectAwareness;
                if (iawareness.tileObject.tileObjectType == tileObjectType) {
                    objects.Add(iawareness);
                }
            }
        }
        return objects;
    }
    private IAwareness CreateNewAwareness(IPointOfInterest poi) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return new CharacterAwareness(poi as Character);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            return new ItemAwareness(poi as SpecialToken);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return new TileObjectAwareness(poi);
        }//TODO: Structure Awareness
        return null;
    }
    public void ClearAllAwareness() {
        POINT_OF_INTEREST_TYPE[] types = Utilities.GetEnumValues<POINT_OF_INTEREST_TYPE>();
        for (int i = 0; i < types.Length; i++) {
            RemoveAwareness(types[i]);
        }
    }
    #endregion

    #region Relationships
    public void SetRelationships(Dictionary<AlterEgoData, CharacterRelationshipData> relationships) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.relationships = relationships;
    }
    public void AddRelationship(AlterEgoData alterEgo, RelationshipTrait newRel) {
        if (!relationships.ContainsKey(alterEgo)) {
            relationships.Add(alterEgo, new CharacterRelationshipData(owner, alterEgo.owner, alterEgo));
        }
        relationships[alterEgo].AddRelationship(newRel);
        owner.OnRelationshipWithCharacterAdded(alterEgo.owner, newRel);
        Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, this, newRel);
    }
    public void RemoveRelationship(AlterEgoData alterEgo, RELATIONSHIP_TRAIT rel) {
        if (relationships.ContainsKey(alterEgo)) {
            relationships[alterEgo].RemoveRelationship(rel);
        }
    }
    public RelationshipTrait GetRelationshipTraitWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT type, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            return relationships[alterEgo].GetRelationshipTrait(type);
        }
        return null;
    }
    public bool HasRelationshipWith(AlterEgoData alterEgo, bool useDisabled = false) {
        if (useDisabled) {
            if (relationships.ContainsKey(alterEgo)) {
                //if there is relationship data present, check if there are actual relationships in their data
                return relationships[alterEgo].rels.Count > 0;
            }
            return false;
        }
        return relationships.ContainsKey(alterEgo) && relationships[alterEgo].rels.Count > 0 && !relationships[alterEgo].isDisabled;
    }
    #endregion

    #region Traits
    public void CopySpecialTraits() {
        this.traits = new List<Trait>();
        for (int i = 0; i < owner.normalTraits.Count; i++) {
            Trait currTrait = owner.normalTraits[i];
            if (!currTrait.isPersistent && currTrait.type == TRAIT_TYPE.SPECIAL) {
                this.traits.Add(currTrait);
            }
        }
    }
    #endregion

    #region For Testing
    public string GetAlterEgoSummary() {
        string summary = owner.name + "'s alter ego " + name + " summary:";
        summary += "\nFaction: " + faction?.name ?? "Null";
        summary += "\nRace: " + race.ToString();
        summary += "\nRole: " + role?.name ?? "Null";
        summary += "\nCharacter Class: " + characterClass?.className ?? "Null";
        summary += "\nHome Structure: " + homeSturcture?.ToString() ?? "Null";
        return summary;
    }
    #endregion
}
