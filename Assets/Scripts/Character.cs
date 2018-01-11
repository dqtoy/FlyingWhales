using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : QuestCreator {
    public string _name;
    public GENDER _gender;
    public List<Trait> _traits;
    public RACE _race;
    public CharacterRole _role;
    public CHARACTER_CLASS _characterClass;
    public Faction _faction;
    public Party _party;

    public Character(RACE race) {
        _gender = Utilities.GetRandomGender();
        _race = race;
        _name = RandomNameGenerator.Instance.GenerateRandomName(_race, _gender);
    }

    #region Roles
    public void AssignRole(CHARACTER_ROLE role) {
        switch (role) {
            case CHARACTER_ROLE.CHIEFTAIN:
                _role = new Chieftain();
                break;
            case CHARACTER_ROLE.VILLAGE_HEAD:
                _role = new VillageHead();
                break;
            case CHARACTER_ROLE.WARLORD:
                _role = new Warlord();
                break;
            case CHARACTER_ROLE.HERO:
                _role = new Hero();
                break;
            case CHARACTER_ROLE.TRADER:
                _role = new Trader();
                break;
            case CHARACTER_ROLE.ADVENTURER:
                _role = new Adventurer();
                break;
            case CHARACTER_ROLE.COLONIST:
                _role = new Colonist();
                break;
            case CHARACTER_ROLE.SPY:
                _role = new Spy();
                break;
            case CHARACTER_ROLE.MEDIATOR:
                _role = new Mediator();
                break;
            case CHARACTER_ROLE.NECROMANCER:
                _role = new Necromancer();
                break;
            case CHARACTER_ROLE.DRAGON_TAMER:
                _role = new DragonTamer();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Character Class
    public void AssignClass(CHARACTER_CLASS charClass) {
        _characterClass = charClass;
    }
    #endregion

    #region Traits
    public bool HasTrait(TRAIT trait) {
        for (int i = 0; i < _traits.Count; i++) {
            if(_traits[i].trait == trait) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Faction
    public void SetFaction(Faction faction) {
        _faction = faction;
    }
    #endregion

    #region Party
    public void SetParty(Party party) {
        _party = party;
    }
    #endregion
}
