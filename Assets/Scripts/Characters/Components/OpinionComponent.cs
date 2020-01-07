using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;

public class OpinionComponent {

    private const int Friend_Requirement = 1; //opinion requirement to consider someone a friend
    private const int Enemy_Requirement = -1; //opinion requirement to consider someone an enemy
    
    public Character owner { get; private set; }
    public Dictionary<Character, Dictionary<string, int>> opinions { get; private set; }
    public List<Character> charactersWithOpinion { get; private set; } //Made a list of all characters with opinion to lessen CPU load

    public OpinionComponent(Character owner) {
        this.owner = owner;
        opinions = new Dictionary<Character, Dictionary<string, int>>();
        charactersWithOpinion = new List<Character>();
    }

    public void AdjustOpinion(Character target, string opinionText, int opinionValue) {
        if (!HasOpinion(target)) {
            opinions.Add(target, new Dictionary<string, int>() { { "Base", 0 } });
            charactersWithOpinion.Add(target);
            Messenger.Broadcast(Signals.OPINION_ADDED, owner, target);
        }
        if (opinions[target].ContainsKey(opinionText)) {
            opinions[target][opinionText] += opinionValue;
        } else {
            opinions[target].Add(opinionText, opinionValue);
        }
        if (opinionValue > 0) {
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target);
        } else if (opinionValue < 0) {
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target);
        }
        if (!target.opinionComponent.HasOpinion(owner)) {
            target.opinionComponent.AdjustOpinion(owner, "Base", 0);
        }
    }

    public void RemoveOpinion(Character target, string opinionText) {
        if (HasOpinion(target)) {
            if (opinions[target].ContainsKey(opinionText)) {
                opinions[target].Remove(opinionText);
            }
        }
    }
    public void RemoveOpinion(Character target) {
        if (HasOpinion(target)) {
            opinions.Remove(target);
            charactersWithOpinion.Remove(target);
            Messenger.Broadcast(Signals.OPINION_REMOVED, owner, target);
        }
    }
    public bool HasOpinion(Character target) {
        return opinions.ContainsKey(target);
    }
    public bool HasOpinion(Character target, string opinionText) {
        return opinions.ContainsKey(target) && opinions[target].ContainsKey(opinionText);
    }
    public int GetTotalOpinion(Character target) {
        return opinions[target].Sum(x => x.Value);
    }
    public int GetTotalPositiveOpinionWith(Character character) {
        if (HasOpinion(character)) {
            int total = 0;
            Dictionary<string, int> _opinions = GetOpinion(character);
            foreach (int value in _opinions.Values) {
                if (value >= 0) {
                    total += value;
                }
            }
            return total;
        }
        return 0;
    }
    public int GetTotalNegativeOpinionWith(Character character) {
        if (HasOpinion(character)) {
            int total = 0;
            Dictionary<string, int> _opinions = GetOpinion(character);
            foreach (int value in _opinions.Values) {
                if (value < 0) {
                    total += value;
                }
            }
            return total;
        }
        return 0;
    }
    public Dictionary<string, int> GetOpinion(Character target) {
        return opinions[target];
    }
    public string GetOpinionLabel(Character target) {
        if (HasOpinion(target)) {
            int totalOpinion = GetTotalOpinion(target);
            if (totalOpinion > 70) {
                return "Close Friend";
            } else if (totalOpinion > 20 && totalOpinion <= 70) {
                return "Friend";
            } else if (totalOpinion > -21 && totalOpinion <= 20) {
                return "Acquaintance";
            } else if (totalOpinion > -71 && totalOpinion <= -21) {
                return "Enemy";
            } else {
                return "Rival";
            }
        }
        return string.Empty;
    }

    #region Inquiry
    public bool IsFriendsWith(Character character) {
        if (HasOpinion(character)) {
            return GetTotalOpinion(character) >= Friend_Requirement;
        }
        return false;
    }
    public bool IsEnemiesWith(Character character) {
        if (HasOpinion(character)) {
            return GetTotalOpinion(character) <= Enemy_Requirement;
        }
        return false;
    }
    #endregion

    #region Data Getting
    public List<Character> GetCharactersWithPositiveOpinion() {
        List<Character> characters = new List<Character>();
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
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
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
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
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
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
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character character) {
        if (HasOpinion(character)) {
            int totalOpinion = GetTotalOpinion(character);
            if (totalOpinion > 0) {
                return RELATIONSHIP_EFFECT.POSITIVE;
            } else if (totalOpinion < 0) {
                return RELATIONSHIP_EFFECT.NEGATIVE;
            }    
        }
        return RELATIONSHIP_EFFECT.NONE;
    }
    #endregion
}