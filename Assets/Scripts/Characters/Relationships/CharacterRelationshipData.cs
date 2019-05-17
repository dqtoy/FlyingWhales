using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipData {

    public Character owner { get; private set; }
    public Character targetCharacter { get; private set; }

    public List<RelationshipTrait> rels { get; private set; }
    public int lastEncounter { get; private set; } //counts up
    public float encounterMultiplier { get; private set; }
    public bool isCharacterMissing { get; private set; }
    public bool isCharacterLocated { get; private set; }
    public LocationStructure knownStructure { get; private set; }
    public List<Trait> trouble { get; private set; } //Set this to trait for now, but this could change in future iterations
    public List<Character> knownLovedOnes { get; private set; }
    public bool isDisabled { get; private set; }
    public int flirtationCount { get; private set; }//the number of times the owner and target character flirted with each other.

    public string lastEncounterLog { get; private set; }

    public CharacterRelationshipData(Character owner, Character targetCharacter) {
        this.owner = owner;
        this.targetCharacter = targetCharacter;
        rels = new List<RelationshipTrait>();
        lastEncounter = 0;
        encounterMultiplier = 0f;
        isCharacterMissing = false;
        isCharacterLocated = true;
        SetKnownStructure(targetCharacter.homeStructure);
        trouble = new List<Trait>();
        LoadInitialLovedOnes();
        AddListeners();
    }

    public void AddListeners() {
        //Messenger.AddListener(Signals.TICK_STARTED, LastEncounterTick);
        //Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        ////Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnTraitAddedToCharacter);
        //Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnTraitRemovedFromCharacter);
        //Messenger.AddListener<Character, RelationshipTrait>(Signals.RELATIONSHIP_ADDED, OnCharacterGainedRelationship);
    }
    public void RemoveListeners() {
        //Messenger.RemoveListener(Signals.TICK_STARTED, LastEncounterTick);
        //Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        ////Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnTraitAddedToCharacter);
        //Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnTraitRemovedFromCharacter);
    }

    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (owner.id == character.id && structure == knownStructure) {
            if (targetCharacter.currentStructure != structure) {
                /*
                - character located turns to False whenever the character visits the specific **known character location structure** 
                with the intent to meet up with the target and not encounter the target there    
                    */
                SetIsCharacterLocated(false);

                /*
                    character trouble is set to Null when the character visits the **known character location structure** with the intent to 
                    meet up with the target and was unable to find the character there or was able to resolve the trouble
                    */
                ClearTrouble();
            }
        }
    }
    //private void OnTraitAddedToCharacter(Character character, Trait trait) {
    //    if (character.id == targetCharacter.id) {
    //        if (trait.name.Equals("Charmed") || trait.name.Equals("Abducted") 
    //            || trait.name.Equals("Unconscious") || trait.name.Equals("Injured") || trait.name.Equals("Cursed")) {
    //            AddTrouble(trait);
    //        }
    //    }
    //}
    private void OnTraitRemovedFromCharacter(Character character, Trait trait) {
        if (character.id == targetCharacter.id) {
            if (trait.name.Equals("Charmed") || trait.name.Equals("Abducted")
                || trait.name.Equals("Unconscious") || trait.name.Equals("Injured") || trait.name.Equals("Cursed")) {
                RemoveTrouble(trait);
            }
        }
    }

    #region Relationships
    public void AddRelationship(RelationshipTrait newRel) {
        if (!rels.Contains(newRel)) {
            rels.Add(newRel);
        }
    }
    public void RemoveRelationship(RelationshipTrait newRel) {
        rels.Remove(newRel);
    }
    public bool HasRelationshipTrait(RELATIONSHIP_TRAIT relType) {
        for (int i = 0; i < rels.Count; i++) {
            if(rels[i].relType == relType && !rels[i].isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasRelationshipTrait(RELATIONSHIP_TRAIT relType1, RELATIONSHIP_TRAIT relType2) {
        for (int i = 0; i < rels.Count; i++) {
            if ((rels[i].relType == relType1 || rels[i].relType == relType2) && !rels[i].isDisabled) {
                return true;
            }
        }
        return false;
    }
    public RelationshipTrait GetRelationshipTrait(RELATIONSHIP_TRAIT relType) {
        for (int i = 0; i < rels.Count; i++) {
            if (rels[i].relType == relType && !rels[i].isDisabled) {
                return rels[i];
            }
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipTraits() {
        List<RelationshipTrait> newRels = new List<RelationshipTrait>();
        for (int i = 0; i < rels.Count; i++) {
            if (!rels[i].isDisabled) {
                newRels.Add(rels[i]);
            }
        }
        return newRels;
    }
    public int GetTotalRelationshipWeight() {
        int weight = 0;
        for (int i = 0; i < rels.Count; i++) {
            weight += GetRelationshipWeight(rels[i].relType);
        }
        return weight;
    }
    public int GetRelationshipWeight(RELATIONSHIP_TRAIT relType) {
        if(relType == RELATIONSHIP_TRAIT.FRIEND) {
            return 15;
        } else if (relType == RELATIONSHIP_TRAIT.RELATIVE) {
            return 10;
        } else if (relType == RELATIONSHIP_TRAIT.LOVER) {
            return 15;
        } else if (relType == RELATIONSHIP_TRAIT.PARAMOUR) {
            return 20;
        } else if (relType == RELATIONSHIP_TRAIT.MASTER) {
            return 5;
        } else if (relType == RELATIONSHIP_TRAIT.ENEMY) {
            return 10;
        } else if (relType == RELATIONSHIP_TRAIT.SAVE_TARGET) {
            return 15;
        }
        return 0;
    }
    public bool HasRelationshipOfEffect(TRAIT_EFFECT effect) {
        for (int i = 0; i < rels.Count; i++) {
            RelationshipTrait currTrait = rels[i];
            if (currTrait.effect == effect) {
                return true;
            }
        }
        return false;
    }
    public void SetIsDisabled(bool state) {
        isDisabled = state;
    }
    #endregion

    #region Encounter Multiplier
    public void AdjustEncounterMultiplier(float adjustment) {
        encounterMultiplier += adjustment;
    }
    public void ResetEncounterMultiplier() {
        encounterMultiplier = 0;
    }
    #endregion

    #region Last Encounter
    public void SetLastEncounterTick(int tick) {
        lastEncounter = tick;
        CheckForCharacterMissing();
    }
    public void SetLastEncounterLog(string log) {
        lastEncounterLog = log;
    }
    private void LastEncounterTick() {
        /*
         - countdown from last encounter
          - ticks every day until the characters share the same non-Wilderness or non-Dungeon structure
          - in Wilderness or Dungeon, will reset this if either this character or target becomes part of the same event (either being the actor, target or reacting to it)
         */
        if (owner.currentStructure != null && owner.currentStructure.structureType != STRUCTURE_TYPE.DUNGEON 
            && owner.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            if (owner.currentStructure == targetCharacter.currentStructure) {
                ResetLastEncounter();
            } else {
                lastEncounter++;
            }
        } else {
            //character is at a dungeon or wilderness structure
            lastEncounter++;
        }
        CheckForCharacterMissing();
    }
    public void ResetLastEncounter() {
        lastEncounter = 0;
        ResetCharacterMissing();
        lastEncounterLog = "Last encountered at " + owner.currentStructure.ToString() + " on " + GameManager.Instance.TodayLogString();
    }
    #endregion

    #region Character Missing
    private void CheckForCharacterMissing() {
        /*
         - character missing (this character has not seen the target in quite a while)
          - this turns to True when **countdown from last encounter** is 150 for relationships from the same faction and 300 for those outside
          - this turns back to False when **countdown from last encounter** has been reset
         */
        try {
            if (lastEncounter >= 480 && targetCharacter.faction.id == owner.faction.id) {
                SetIsCharacterMissing(true);
            } else if (lastEncounter >= 960 && targetCharacter.faction.id != owner.faction.id) {
                SetIsCharacterMissing(true);
            }
        } catch (System.Exception e){
            throw new System.Exception(e.Message);
        }
        
    }
    private void ResetCharacterMissing() {
        SetIsCharacterMissing(false);
    }
    public void SetIsCharacterMissing(bool state) {
        if (isCharacterMissing == state) {
            return; //ignore change
        }
        isCharacterMissing = state;
        if (!isCharacterMissing) {
            SetKnownStructure(targetCharacter.homeStructure);
        } else {
            //character located now turns to False when character missing is switched from False to True
            SetIsCharacterLocated(false);
        }
    }
    #endregion

    #region Character Located
    public void SetIsCharacterLocated(bool state) {
        isCharacterLocated = state;
    }
    #endregion

    #region Known Structure
    public void SetKnownStructure(LocationStructure structure) {
        knownStructure = structure;
    }
    #endregion

    #region Trouble
    public void ClearTrouble() {
        trouble.Clear();
    }
    public void AddTrouble(Trait trait) {
        if (trait == null) {
            return;
        }
        if (!trouble.Contains(trait)) {
            trouble.Add(trait);
        }
    }
    public void AddTrouble(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            AddTrouble(traits[i]);
        }
    }
    public void RemoveTrouble(Trait trait) {
        if (trouble.Remove(trait)) {
            SaverCheck();
        }
    }
    private void SaverCheck() {
        if (trouble == null || trouble.Count == 0) {
            if (HasRelationshipTrait(RELATIONSHIP_TRAIT.SAVE_TARGET)) {
                CharacterManager.Instance.RemoveRelationshipBetween(owner, targetCharacter, RELATIONSHIP_TRAIT.SAVE_TARGET);
            }
        }
    }
    #endregion

    #region Known Loved Ones
    private void LoadInitialLovedOnes() {
        knownLovedOnes = new List<Character>();
        //if (targetCharacter.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.RELATIVE)) {
        //    foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in targetCharacter.relationships) {
        //        if (kvp.Value.HasRelationshipTrait(RELATIONSHIP_TRAIT.RELATIVE)) {
        //            AddKnownLovedOne(kvp.Key);
        //        }
        //    }
        //}
    }
    private void OnCharacterGainedRelationship(Character character, RelationshipTrait rel) {
        if (character.id == targetCharacter.id && rel.relType == RELATIONSHIP_TRAIT.RELATIVE) {
            AddKnownLovedOne(rel.targetCharacter);
        }
    }
    public void AddKnownLovedOne(Character character) {
        if (!knownLovedOnes.Contains(character)) {
            knownLovedOnes.Add(character);
        }
    }
    public void RemoveKnownLovedOne(Character character) {
        knownLovedOnes.Remove(character);
    }
    #endregion

    #region Flirtation
    public void IncreaseFlirtationCount() {
        flirtationCount += 1;
    }
    #endregion

    #region For Testing
    public string GetSummary() {
        string text = targetCharacter.name + " (" + targetCharacter.faction?.name ?? "Factionless" + "): ";
        for (int i = 0; i < rels.Count; i++) {
            text += "|" + rels[i].name + "(" + rels[i].severity.ToString() + ")"+ "|";
        }
        text += "\n\t<b>Last Encounter:</b> " + lastEncounter.ToString() + " (" + lastEncounterLog + ")";
        text += "\n\t<b>Encounter Multiplier:</b> " + encounterMultiplier.ToString();
        text += "\n\t<b>Is Missing?:</b> " + isCharacterMissing.ToString();
        text += "\n\t<b>Is Located?:</b> " + isCharacterLocated.ToString();
        text += "\n\t<b>Known Structure:</b> " + knownStructure?.ToString() ?? "Unknown";
        text += "\n\t<b>Trouble:</b> ";
        if (trouble.Count > 0) {
            for (int i = 0; i < trouble.Count; i++) {
                text += "|" + trouble[i].name + "|";
            }
        } else {
            text += "None";
        }
        text += "\n\t<b>Known Loved Ones:</b> ";
        if (knownLovedOnes.Count > 0) {
            for (int i = 0; i < knownLovedOnes.Count; i++) {
                text += "|" + knownLovedOnes[i].name + "|";
            }
        } else {
            text += "None";
        }
        text += "\n\t<b>Flirtation Count:</b> " + flirtationCount.ToString();
        return text;
    }
    #endregion
}
