using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAwareness : IAwareness {
    public IPointOfInterest poi { get { return _character; } }
    public Character character { get { return _character; } }
    public List<CharacterRelationshipData> knownRelationships { get; private set; }
    public List<Trait> knownTraits { get; private set; }
    public List<GoapPlan> knownPlans { get; private set; }
    public Area knownLocation { get { return knownGridLocation.parentAreaMap.area; } }
    public LocationGridTile knownGridLocation { get; private set; }

    private Character _character;

    public CharacterAwareness(Character character) {
        _character = character;
        knownRelationships = new List<CharacterRelationshipData>();
        knownTraits = new List<Trait>();
        knownPlans = new List<GoapPlan>();
        SetKnownGridLocation(character.gridTileLocation);
    }

    public void AddKnownRelationship(CharacterRelationshipData relationshipData) {
        knownRelationships.Add(relationshipData);
    }
    public void RemoveKnownRelationship(CharacterRelationshipData relationshipData) {
        knownRelationships.Remove(relationshipData);
    }

    public void AddKnownTrait(Trait trait) {
        knownTraits.Add(trait);
    }
    public void RemoveKnownTrait(Trait trait) {
        knownTraits.Remove(trait);
    }

    public void AddKnownPlan(GoapPlan plan) {
        knownPlans.Add(plan);
    }
    public void RemoveKnownPlan(GoapPlan plan) {
        knownPlans.Remove(plan);
    }

    public void SetKnownGridLocation(LocationGridTile tile) {
        knownGridLocation = tile;
    }

    public void OnAddAwareness(Character character) { }
    public void OnRemoveAwareness(Character character) { }
}
