using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterProductionCap {
    public CHARACTER_PRODUCTION_CAP capReq;
    public int cap;

    public bool IsCapReached(CHARACTER_ROLE role, Faction faction, Settlement settlement) {
        int numOfCharacters = 0;
        switch (capReq) {
            case CHARACTER_PRODUCTION_CAP.CITY_COUNT:
                int totalCap = faction.settlements.Count * cap; //ex. 1 per city per Tribe, 3 per Settlement per Faction
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if(numOfCharacters >= totalCap) {
                    return true;
                }
                return false;
            case CHARACTER_PRODUCTION_CAP.MINOR_FACTION:
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if (faction.factionType == FACTION_TYPE.MINOR) {
                    if(numOfCharacters >= cap) {
                        return true;
                    }
                }
                return false; //faction is not minor or cap has not been reached
            case CHARACTER_PRODUCTION_CAP.SMALL_TRIBE:
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if (faction.factionSize == FACTION_SIZE.SMALL) {
                    if (numOfCharacters >= cap) {
                        return true;
                    }
                }
                return false; //faction is not small or cap has not been reached
            case CHARACTER_PRODUCTION_CAP.MEDIUM_TRIBE:
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if (faction.factionSize == FACTION_SIZE.MEDIUM) {
                    if (numOfCharacters >= cap) {
                        return true;
                    }
                }
                return false; //faction is not medium or cap has not been reached
            case CHARACTER_PRODUCTION_CAP.LARGE_TRIBE:
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if (faction.factionSize == FACTION_SIZE.LARGE) {
                    if (numOfCharacters >= cap) {
                        return true;
                    }
                }
                return false; //faction is not large or cap has not been reached
            case CHARACTER_PRODUCTION_CAP.ENTIRE_WORLD:
                numOfCharacters = FactionManager.Instance.GetAllCharactersOfType(role).Count;
                if (numOfCharacters >= cap) {
                    return true;
                }
                return false; //entire world cap has not been reached
            case CHARACTER_PRODUCTION_CAP.PER_TRIBE:
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if (numOfCharacters >= cap) {
                    return true;
                }
                return false; //faction has less characters than cap
            case CHARACTER_PRODUCTION_CAP.PER_SETTLEMENT:
                numOfCharacters = settlement.GetCharactersCreatedInSettlement(role).Count;
                numOfCharacters = faction.GetCharactersOfType(role).Count;
                if (numOfCharacters >= cap) {
                    return true;
                }
                return false; //settlement has less characters than cap
        }
        return false;
    }
}
