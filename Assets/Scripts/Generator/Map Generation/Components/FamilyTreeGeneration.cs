using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class FamilyTreeGeneration : MapGenerationComponent {

    public override IEnumerator Execute(MapGenerationData data) {
        data.familyTrees = new Dictionary<RACE, List<FamilyTree>>();

        data.familyTrees.Add(RACE.HUMANS, new List<FamilyTree>());
        data.familyTrees.Add(RACE.ELVES, new List<FamilyTree>());
        
        //human family trees
        for (int i = 0; i < 15; i++) {
            FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(RACE.HUMANS);
            data.familyTrees[RACE.HUMANS].Add(familyTree);    
        }
        //elven family trees
        for (int i = 0; i < 15; i++) {
            FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(RACE.ELVES);
            data.familyTrees[RACE.ELVES].Add(familyTree);    
        }

        GenerateAdditionalCouples(RACE.HUMANS, data);
        GenerateAdditionalCouples(RACE.ELVES, data);
        yield return null;
    }
    
    private void GenerateAdditionalCouples(RACE race, MapGenerationData data) {
        List<FamilyTree> families = data.familyTrees[race];
        int pairCount = families.Count / 2;

        for (int i = 0; i < pairCount; i++) {
            int index = i * 2;
            FamilyTree firstFamily = families[index];
            FamilyTree secondFamily = families.ElementAt(index + 1);
            
            if (firstFamily.children.Count == 0 || secondFamily.children.Count == 0) {
                continue;
            }
            PreCharacterData randomChildFromFirst = CollectionUtilities.GetRandomElement(firstFamily.children);
            PreCharacterData compatibleChildFromSecond =
                GetCompatibleChildFromFamily(randomChildFromFirst, secondFamily);

            if (compatibleChildFromSecond != null) {
                randomChildFromFirst.AddRelationship(RELATIONSHIP_TYPE.LOVER, compatibleChildFromSecond);
                compatibleChildFromSecond.AddRelationship(RELATIONSHIP_TYPE.LOVER, randomChildFromFirst);
                
                randomChildFromFirst.RandomizeCompatibility(3, 5, compatibleChildFromSecond);
                compatibleChildFromSecond.RandomizeCompatibility(3, 5, randomChildFromFirst);
                
                randomChildFromFirst.RandomizeOpinion(30, 100, compatibleChildFromSecond);
                compatibleChildFromSecond.RandomizeOpinion(30, 100, randomChildFromFirst);
            }
        }
    }

    private PreCharacterData GetCompatibleChildFromFamily(PreCharacterData target, FamilyTree familyTree) {
        for (int i = 0; i < familyTree.children.Count; i++) {
            PreCharacterData child = familyTree.children[i];
            if (RelationshipManager.IsSexuallyCompatible(target.sexuality, child.sexuality, 
                target.gender, child.gender) 
                && child.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER) == null) {
                return child;
            }
        }
        return null;
    }
}
