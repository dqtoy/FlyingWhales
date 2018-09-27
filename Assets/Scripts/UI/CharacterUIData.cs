using ECS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterUIData {

    public int level { get; private set; }
    public string className { get; private set; }
    public float healthValue { get; private set; }
    public float manaValue { get; private set; }
    public int strength { get; private set; }
    public int intelligence { get; private set; }
    public int agility { get; private set; }
    public int vitality { get; private set; }
    public List<Attribute> attributes { get; private set; }
    public List<Item> equippedItems { get; private set; }
    public List<Item> inventory { get; private set; }
    public List<Relationship> relationships { get; private set; }
    public Faction faction;

    public CharacterUIData() {
        attributes = new List<Attribute>();
        equippedItems = new List<Item>();
        inventory = new List<Item>();
        relationships = new List<Relationship>();
    }

    public void UpdateData(Character character) {
        level = character.level;
        className = character.characterClass.className;
        healthValue = (float)character.currentHP / (float)character.maxHP;
        manaValue = (float)character.currentSP / (float)character.maxSP;
        strength = character.strength;
        intelligence = character.intelligence;
        agility = character.agility;
        vitality = character.vitality;

        attributes.Clear();
        attributes.AddRange(character.attributes);

        equippedItems.Clear();
        equippedItems.AddRange(character.equippedItems);

        inventory.Clear();
        inventory.AddRange(character.inventory);

        relationships.Clear();
        for (int i = 0; i < character.relationships.Count; i++) {
            relationships.Add(character.relationships.Values.ElementAt(i).CreateCopy()); //create copy instead of instance
        }
    }
}
