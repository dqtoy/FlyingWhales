using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    public List<CharacterProductionWeight> characterProductionWeights;

    private void Awake() {
        Instance = this;
    }

    /*
     Create a new landmark on a specified tile.
     */
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BASE_LANDMARK_TYPE baseLandmarkType = Utilities.GetBaseLandmarkType(landmarkType);
        BaseLandmark newLandmark = location.CreateLandmarkOfType(baseLandmarkType, landmarkType);
        if(baseLandmarkType == BASE_LANDMARK_TYPE.SETTLEMENT && landmarkType != LANDMARK_TYPE.CITY) {
            if(landmarkType == LANDMARK_TYPE.GOBLIN_CAMP) {
                //Create a new faction to occupy the new settlement
                Faction newFaction = FactionManager.Instance.CreateNewFaction(typeof(Camp), RACE.GOBLIN);
                newLandmark.OccupyLandmark(newFaction);
            }
        }
        return newLandmark;
    }
    /*
     Occupy a specified landmark.
         */
    public void OccupyLandmark(BaseLandmark landmark, Faction occupant) {
        landmark.OccupyLandmark(occupant);
    }
    /*
     Occupy the main settlement in a region
         */
    public void OccupyLandmark(Region region, Faction occupant) {
        region.centerOfMass.landmarkOnTile.OccupyLandmark(occupant);
    }

    #region ECS.Character Production
    /*
     Get the character role weights for a faction.
     This will not include roles that the faction has already reached the cap of.
         */
    public WeightedDictionary<CHARACTER_ROLE> GetCharacterRoleProductionDictionary(Faction faction) {
        WeightedDictionary<CHARACTER_ROLE> characterWeights = new WeightedDictionary<CHARACTER_ROLE>();
        for (int i = 0; i < characterProductionWeights.Count; i++) {
            CharacterProductionWeight currWeight = characterProductionWeights[i];
            bool shouldIncludeWeight = true;
            for (int j = 0; j < currWeight.productionCaps.Count; j++) {
                CharacterProductionCap currCap = currWeight.productionCaps[j];
                if(currCap.IsCapReached(currWeight.role, faction)) {
                    shouldIncludeWeight = false; //The current faction has already reached the cap for the current role, do not add to weights.
                    break;
                }
            }
            if (shouldIncludeWeight) {
                characterWeights.AddElement(currWeight.role, currWeight.weight);
            }
        }
        return characterWeights;
    }
    /*
     Get the character class weights for a settlement.
     This will eliminate any character classes that the settlement cannot
     produce due to a lack of technologies.
         */
    public WeightedDictionary<CHARACTER_CLASS> GetCharacterClassProductionDictionary(Settlement settlement) {
        WeightedDictionary<CHARACTER_CLASS> classes = new WeightedDictionary<CHARACTER_CLASS>();
        CHARACTER_CLASS[] allClasses = Utilities.GetEnumValues<CHARACTER_CLASS>();
        for (int i = 0; i < allClasses.Length; i++) {
            CHARACTER_CLASS charClass = allClasses[i];
            if (settlement.CanProduceClass(charClass)) { //Does the settlement have the required technologies to produce this class
                classes.AddElement(charClass, 200);
            }
        }
        return classes;
    }
    #endregion
}
