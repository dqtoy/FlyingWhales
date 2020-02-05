using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FamilyTree {

    public PreCharacterData father { get; }
    public PreCharacterData mother { get; }
    public List<PreCharacterData> children { get; }
    public List<PreCharacterData> allFamilyMembers { get; } //this is only for convenience. Contains mother, father and children
    
    public FamilyTree(PreCharacterData _father, PreCharacterData _mother, List<PreCharacterData> _children) {
        father = _father;
        mother = _mother;
        children = _children;
        allFamilyMembers = new List<PreCharacterData>();
        allFamilyMembers.Add(father);
        allFamilyMembers.Add(mother);
        allFamilyMembers.AddRange(children);
    }
}
