using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public interface IParty {
    float computedPower { get; }
    int numOfAttackers { get; set; }
    Faction attackedByFaction { get; set; }
    Faction faction { get; }
    Combat currentCombat { get; set; }
    CharacterIcon icon { get; }
    Region currentRegion { get; }
    ICharacterObject icharacterObject { get; }
    ILocation specificLocation { get; }
    List<ICharacter> icharacters { get; }

    void GoHome();
    void AddCharacter(ICharacter icharacter);
    void RemoveCharacter(ICharacter icharacter);
    void AdvertiseSelf(ActionThread actionThread);
    void SetSpecificLocation(ILocation location);
}
