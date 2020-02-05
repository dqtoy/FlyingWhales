using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreCharacterData {
    
    public int id { get; }
    public RACE race { get; }
    public GENDER gender { get; }
    public string name => $"{firstName} {surName}";
    public string firstName { get; private set; }
    public string surName { get; private set; }
    public SEXUALITY sexuality { get; }
    public Dictionary<PreCharacterData, PreCharacterRelationship> relationships { get; }
    public bool hasBeenSpawned { get; private set; }
    
    public PreCharacterData(RACE _race, GENDER _gender, WeightedDictionary<SEXUALITY> _sexualityWeights) {
        id = UtilityScripts.Utilities.SetID(this);
        race = _race;
        gender = _gender;
        SetName(RandomNameGenerator.GenerateRandomName(_race, _gender));
        sexuality = _sexualityWeights.PickRandomElementGivenWeights();
        relationships = new Dictionary<PreCharacterData, PreCharacterRelationship>();
    }
    private void SetName(string name) {
        firstName = name.Split(' ')[0];
        surName = name.Split(' ')[1];
        RandomNameGenerator.RemoveNameAsAvailable(gender, race, firstName);
    }
    public void SetSurName(string _surName) {
        surName = _surName;
    }

    /// <summary>
    /// Randomize Compatibility of this character with another character.
    /// NOTE: This creates new relationship data if none exists yet for the target character.
    /// </summary>
    /// <param name="lowerBound">lower bound of random compatibility [inclusive].</param>
    /// <param name="upperBound">upper bound of random compatibility [inclusive].</param>
    /// <param name="characterData">the target character.</param>
    public void RandomizeCompatibility(int lowerBound, int upperBound, PreCharacterData characterData) {
        PreCharacterRelationship relationship = GetOrInitializeRelationshipWith(characterData);
        relationship.SetCompatibility(Random.Range(lowerBound, upperBound + 1));
    }
    
    /// <summary>
    /// Randomize Opinion of this character with another character.
    /// NOTE: This creates new relationship data if none exists yet for the target character.
    /// </summary>
    /// <param name="lowerBound">lower bound of random compatibility [inclusive].</param>
    /// <param name="upperBound">upper bound of random compatibility [inclusive].</param>
    /// <param name="characterData">the target character.</param>
    public void RandomizeOpinion(int lowerBound, int upperBound, PreCharacterData characterData) {
        PreCharacterRelationship relationship = GetOrInitializeRelationshipWith(characterData);
        relationship.SetOpinion(Random.Range(lowerBound, upperBound + 1));
    }

    public void AddRelationship(RELATIONSHIP_TYPE relationshipType, PreCharacterData characterData) {
        PreCharacterRelationship relationship = GetOrInitializeRelationshipWith(characterData);
        relationship.AddRelationship(relationshipType);
    }
    
    private PreCharacterRelationship GetOrInitializeRelationshipWith(PreCharacterData characterData) {
        if (relationships.ContainsKey(characterData) == false) {
            relationships.Add(characterData, new PreCharacterRelationship());
        }
        return relationships[characterData];
    }
    public PreCharacterData GetCharacterWithRelationship(RELATIONSHIP_TYPE relationshipType) {
        foreach (var relationship in relationships) {
            if (relationship.Value.relationships.Contains(relationshipType)) {
                return relationship.Key;
            }
        }
        return null;
    }

    public int GetCompatibilityWith(PreCharacterData character) {
        return relationships[character].compatibility;
    }

    public void SetHasBeenSpawned() {
        hasBeenSpawned = true;
    }
}
