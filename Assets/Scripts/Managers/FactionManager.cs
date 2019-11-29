using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    public List<Faction> allFactions = new List<Faction>();
    public Faction neutralFaction { get; private set; }
    public Faction disguisedFaction { get; private set; }
    private Faction _zombieFaction;

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<Sprite> _factionEmblems;

    private List<Sprite> usedEmblems = new List<Sprite>();

    #region getters
    public Faction zombieFaction {
        get {
            if (_zombieFaction == null) {
                _zombieFaction = CreateNewFaction(factionName: "Zombies");
            }
            return _zombieFaction;
        }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }

    #region Faction Generation
    public void CreateNeutralFaction() {
        Faction newFaction = new Faction();
        newFaction.SetName("Neutral");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(GetFactionEmblem(4));
        allFactions.Add(newFaction);
        SetNeutralFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        //CreateFavorsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void CreateDisguisedFaction() {
        Faction newFaction = new Faction();
        newFaction.SetName("Disguised");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(GetFactionEmblem(4));
        allFactions.Add(newFaction);
        SetDisguisedFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        //CreateFavorsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void SetNeutralFaction(Faction faction) {
        neutralFaction = faction;
    }
    public void SetDisguisedFaction(Faction faction) {
        disguisedFaction = faction;
    }
    public Faction CreateNewFaction(bool isPlayerFaction = false, string factionName = "") {
        Faction newFaction = new Faction(isPlayerFaction);
        allFactions.Add(newFaction);
        CreateRelationshipsForFaction(newFaction);
        //CreateFavorsForFaction(newFaction);
        if (!string.IsNullOrEmpty(factionName)) {
            newFaction.SetName(factionName);
        }
        if (!isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    public Faction CreateNewFaction(SaveDataFaction data) {
        Faction newFaction = new Faction(data);
        if(data.name == "Neutral") {
            SetNeutralFaction(newFaction);
        }
        allFactions.Add(newFaction);
        if (!newFaction.isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    public void DeleteFaction(Faction faction) {
        //for (int i = 0; i < faction.ownedRegions.Count; i++) {
        //    Area ownedArea = faction.ownedRegions[i];
        //    LandmarkManager.Instance.UnownArea(ownedArea);
        //}
        //RemoveRelationshipsWith(faction);
        //Messenger.Broadcast(Signals.FACTION_DELETED, faction);
        //allFactions.Remove(faction);
    }
    #endregion

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal Sprite GenerateFactionEmblem(Faction faction) {
        for (int i = 0; i < _factionEmblems.Count; i++) {
            Sprite currSprite = _factionEmblems[i];
            if (usedEmblems.Contains(currSprite)) {
                continue;
            }
            usedEmblems.Add(currSprite);
            return currSprite;
        }
        throw new System.Exception("There are no more emblems for faction: " + faction.name);
    }
    public Sprite GetFactionEmblem(int emblemIndex) {
        return _factionEmblems[emblemIndex];
        //for (int i = 0; i < _emblemBGs.Count; i++) {
        //    EmblemBG currBG = _emblemBGs[i];
        //    if (currBG.id.Equals(emblemID)) {
        //        return currBG;
        //    }
        //}
        //throw new System.Exception("There is no emblem bg with id " + emblemID);
    }
    public int GetFactionEmblemIndex(Sprite emblem) {
        for (int i = 0; i < _factionEmblems.Count; i++) {
            Sprite currSetting = _factionEmblems[i];
            if (currSetting == emblem) {
                return i;
            }
            //foreach (KeyValuePair<int, Sprite> kvp in currSetting.emblems) {
            //    if (kvp.Value.name == emblem.name) {
            //        return i;
            //    }
            //}
        }
        return -1;
    }
    #endregion

    #region Utilities
    public Faction GetFactionBasedOnID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].id == id) {
                return allFactions[i];
            }
        }
        return null;
    }
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].name.ToLower() == name.ToLower()) {
                return allFactions[i];
            }
        }
        return null;
    }
    public void TransferCharacter(Character character, Faction faction, Region newHome) {
        if (character.faction != null) {
            character.faction.LeaveFaction(character);
        }
        faction.JoinFaction(character);
        character.MigrateHomeTo(newHome);
        //character.homeLandmark.RemoveCharacterHomeOnLandmark(character);
        //newHome.AddCharacterHomeOnLandmark(character);
        //Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, _characterInvolved.specificLocation.coreTile.landmarkOnTile);
        //character.SetForcedInteraction(interaction);
    }
    public List<Character> GetViableRulers(Character previousRuler, GENDER gender, params RELATIONSHIP_TRAIT[] type) {
        List<Character> characters = new List<Character>();
        List<Relatable> relatables = previousRuler.relationshipContainer.GetRelatablesWithRelationship(type);
        for (int i = 0; i < relatables.Count; i++) {
            Relatable r = relatables[i];
            if (r is AlterEgoData) {
                Character character = (r as AlterEgoData).owner;
                if (character.isDead || character.gender != gender || character.faction.IsHostileWith(previousRuler.faction) || character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) || character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL) || characters.Contains(character)) {
                    continue;
                }
                characters.Add(character);
            }
        }
        return characters;
    }
    #endregion

    #region Relationships
    public void CreateRelationshipsForFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
            }
        }
    }
    /*
     Create a new relationship between 2 factions,
     then add add a reference to that relationship, to both of the factions.
         */
    public FactionRelationship CreateNewRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        faction1.AddNewRelationship(faction2, newRel);
        faction2.AddNewRelationship(faction1, newRel);
        if(faction1.isPlayerFaction || faction2.isPlayerFaction) {
            if(faction1.name != "Disguised" && faction2.name != "Disguised") {
                faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.HOSTILE);
                faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.HOSTILE);
            }
        }
        return newRel;
    }
    /*
     Utility Function for getting the relationship between 2 factions,
     this just adds a checking for data consistency if, the 2 factions have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each factions GetRelationshipWith() instead.
         */
    public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship faction1Rel = faction1.GetRelationshipWith(faction2);
        FactionRelationship faction2Rel = faction2.GetRelationshipWith(faction1);
        if (faction1Rel == faction2Rel) {
            return faction1Rel;
        }
        throw new System.Exception(faction1.name + " does not have the same relationship object as " + faction2.name + "!");
    }
    public int GetAverageFactionLevel() {
        int activeFactionsCount = allFactions.Where(x => x.isActive).Count();
        int totalFactionLvl = allFactions.Where(x => x.isActive).Sum(x => x.level);
        return totalFactionLvl / activeFactionsCount;
    }
    #endregion
}

[System.Serializable]
public class FactionEmblemSetting {
    public FactionEmblemDictionary emblems;

    public Sprite GetSpriteForSize(Image image) {
        if (image.rectTransform.sizeDelta.x <= 24) {
            return emblems[24];
        } else {
            return emblems[96];
        }
    }
    public Sprite GetSpriteForSize(int size) {
        if (size <= 24) {
            return emblems[24];
        } else {
            return emblems[96];
        }
    }
}
