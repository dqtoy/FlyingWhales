using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class BaseRelationshipContainer : IRelationshipContainer {
    public const string Close_Friend = "Close Friend";
    public const string Friend = "Friend";
    public const string Acquaintance = "Acquaintance";
    public const string Enemy = "Enemy";
    public const string Rival = "Rival";
    
    private const int Friend_Requirement = 1; //opinion requirement to consider someone a friend
    private const int Enemy_Requirement = -1; //opinion requirement to consider someone an enemy
    
    public Dictionary<int, IRelationshipData> relationships { get; }
    public List<Character> charactersWithOpinion { get; }
    
    public BaseRelationshipContainer() {
        relationships = new Dictionary<int, IRelationshipData>();
        charactersWithOpinion = new List<Character>();
    }

    #region Adding
    public void AddRelationship(Relatable owner, Relatable relatable, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(owner, relatable);
        }
        relationships[relatable.id].AddRelationship(relType);
    }
    public void CreateNewRelationship(Relatable owner, Relatable relatable) {
        relationships.Add(relatable.id, new BaseRelationshipData());
        if (relatable is Character) {
            charactersWithOpinion.Add(relatable as Character);
        }
        Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, owner, relatable);
    }
    public void CreateNewRelationship(Relatable owner, int id) {
        relationships.Add(id, new BaseRelationshipData());
        Messenger.Broadcast<Relatable, Relatable>(Signals.RELATIONSHIP_ADDED, owner, null);
    }
    private IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, Relatable relatable) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(owner, relatable);
        }
        return relationships[relatable.id];
    }
    private bool TryGetRelationshipDataWith(Relatable relatable, out IRelationshipData data) {
        return relationships.TryGetValue(relatable.id, out data);
    }
    #endregion

    #region Removing
    public void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel) {
        relationships[relatable.id].RemoveRelationship(rel);
    }
    #endregion

    #region Inquiry
    public bool HasRelationshipWith(Relatable relatable) {
        return relationships.ContainsKey(relatable.id);
    }
    public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            return data.relationships.Contains(relType);
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable relatable, params RELATIONSHIP_TYPE[] relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            for (int i = 0; i < relType.Length; i++) {
                RELATIONSHIP_TYPE rel = relType[i];
                for (int j = 0; j < data.relationships.Count; j++) {
                    RELATIONSHIP_TYPE dataRel = data.relationships[j];
                    if(rel == dataRel) {
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }
    public bool IsFamilyMember(Character target) {
        if (HasRelationshipWith(target)) {
            IRelationshipData data = GetRelationshipDataWith(target);
            return data.HasRelationship(RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.SIBLING);
        }
        return false;
    }
    #endregion

    #region Getting
    public int GetFirstRelatableIDWithRelationship(params RELATIONSHIP_TYPE[] type) {
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                return kvp.Key;
            }
        }
        return -1;
    }
    public int GetRelatablesWithRelationshipCount(params RELATIONSHIP_TYPE[] type) {
        int count = 0;
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                count++;
            }
        }
        return count;
    }
    public IRelationshipData GetRelationshipDataWith(Relatable relatable) {
        if (HasRelationshipWith(relatable)) {
            return relationships[relatable.id];
        }
        return null;
    }
    public RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, params RELATIONSHIP_TYPE[] relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            for (int i = 0; i < relType.Length; i++) {
                RELATIONSHIP_TYPE rel = relType[i];
                for (int j = 0; j < data.relationships.Count; j++) {
                    RELATIONSHIP_TYPE dataRel = data.relationships[j];
                    if (rel == dataRel) {
                        return rel;
                    }
                }
            }
            return RELATIONSHIP_TYPE.NONE;
        }
        return RELATIONSHIP_TYPE.NONE;
    }
    #endregion

    #region Opinions
    public void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, target);
        if (owner.traitContainer.HasTrait("Serial Killer")) {
            SerialKiller serialKiller = owner.traitContainer.GetNormalTrait<SerialKiller>("Serial Killer");
            serialKiller.AdjustOpinion(target, opinionText, opinionValue);
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
            owner.logComponent.PrintLogIfActive(owner.name + " wants to adjust " + opinionText + " opinion towards " + target.name + " by " + opinionValue + " but " + owner.name + " is a Serial Killer");
            opinionValue = 0;
        }
        relationshipData.opinions.AdjustOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target, lastStrawReason);
        }
        if (target.relationshipContainer.HasRelationshipWith(owner) == false) {
            target.relationshipContainer.CreateNewRelationship(target, owner);
        }
    }
    public void SetOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, target);
        if (owner.traitContainer.HasTrait("Serial Killer")) {
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
            owner.logComponent.PrintLogIfActive(owner.name + " wants to adjust " + opinionText + " opinion towards " + target.name + " by " + opinionValue + " but " + owner.name + " is a Serial Killer, setting the value to zero...");
            opinionValue = 0;
        }
        relationshipData.opinions.SetOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target, lastStrawReason);
        }
        if (target.relationshipContainer.HasRelationshipWith(owner) == false) {
            target.relationshipContainer.CreateNewRelationship(target, owner);
        }
    }
    public void RemoveOpinion(Character target, string opinionText) {
        if (TryGetRelationshipDataWith(target, out var relationshipData)) {
            relationshipData.opinions.RemoveOpinion(opinionText);
        }
    }
    public bool HasOpinion(Character target, string opinionText) {
        if (TryGetRelationshipDataWith(target, out var relationshipData)) {
            return relationshipData.opinions.HasOpinion(opinionText);
        }
        return false;
    }
    public int GetTotalOpinion(Character target) {
        return relationships[target.id].opinions.totalOpinion;
    }
    public OpinionData GetOpinionData(Character target) {
        return relationships[target.id].opinions;
    }
    public string GetOpinionLabel(Character target) {
        if (HasRelationshipWith(target)) {
            int totalOpinion = GetTotalOpinion(target);
            if (totalOpinion > 70) {
                return Close_Friend;
            } else if (totalOpinion > 20 && totalOpinion <= 70) {
                return Friend;
            } else if (totalOpinion > -21 && totalOpinion <= 20) {
                return Acquaintance;
            } else if (totalOpinion > -71 && totalOpinion <= -21) {
                return Enemy;
            } else {
                return Rival;
            }
        }
        return string.Empty;
    }
    public bool IsFriendsWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == Friend || opinionLabel == Close_Friend;
    }
    public bool IsEnemiesWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == Enemy || opinionLabel == Rival;
    }
    public List<Character> GetCharactersWithPositiveOpinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (GetTotalOpinion(otherCharacter) > 0) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithNeutralOpinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            int opinion = GetTotalOpinion(otherCharacter); 
            if (opinion < Friend_Requirement && opinion > Enemy_Requirement) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithNegativeOpinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (GetTotalOpinion(otherCharacter) < 0) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetEnemyCharacters() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetFriendCharacters() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsFriendsWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithOpinionLabel(params string[] labels) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            string opinionLabel = GetOpinionLabel(otherCharacter);
            for (int j = 0; j < labels.Length; j++) {
                if (labels[j] == opinionLabel) {
                    characters.Add(otherCharacter);
                }
            }
        }
        return characters;
    }
    public bool HasCharacterWithOpinionLabel(params string[] labels) {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            string opinionLabel = GetOpinionLabel(otherCharacter);
            for (int j = 0; j < labels.Length; j++) {
                if (labels[j] == opinionLabel) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasOpinionLabelWithCharacter(Character character, params string[] labels) {
        if (HasRelationshipWith(character)) {
            string opinionLabel = GetOpinionLabel(character);
            for (int j = 0; j < labels.Length; j++) {
                if (labels[j] == opinionLabel) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasEnemyCharacter() {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                return true;
            }
        }
        return false;
    }
    public int GetNumberOfFriendCharacters() {
        int count = 0;
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsFriendsWith(otherCharacter)) {
                count++;
            }
        }
        return count;
    }
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character character) {
        if (HasRelationshipWith(character)) {
            int totalOpinion = GetTotalOpinion(character);
            if (totalOpinion > 0) {
                return RELATIONSHIP_EFFECT.POSITIVE;
            } else if (totalOpinion < 0) {
                return RELATIONSHIP_EFFECT.NEGATIVE;
            }    
        }
        return RELATIONSHIP_EFFECT.NONE;
    }
    public int GetCompatibility(Character target) {
        if (TryGetRelationshipDataWith(target, out var relationshipData)) {
            return relationshipData.opinions.compatibilityValue;
        }
        return -1;
    }
    public string GetRelationshipNameWith(Character target) {
        if (TryGetRelationshipDataWith(target, out var data)) {
            RELATIONSHIP_TYPE relType = data.GetFirstMajorRelationship();
            switch (relType) {
                case RELATIONSHIP_TYPE.CHILD:
                    return target.gender == GENDER.MALE ? "Son" : "Daughter";
                case RELATIONSHIP_TYPE.PARENT:
                    return target.gender == GENDER.MALE ? "Father" : "Mother";
                case RELATIONSHIP_TYPE.SIBLING:
                    return target.gender == GENDER.MALE ? "Brother" : "Sister";
                case RELATIONSHIP_TYPE.LOVER:
                    return target.gender == GENDER.MALE ? "Husband" : "Wife";
                case RELATIONSHIP_TYPE.NONE:
                    string opinionLabel = GetOpinionLabel(target);
                    return string.IsNullOrEmpty(opinionLabel) == false ? opinionLabel : Acquaintance;
                default:
                    return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(relType.ToString());
            }
        }
        return Acquaintance;
    }
    #endregion
}
