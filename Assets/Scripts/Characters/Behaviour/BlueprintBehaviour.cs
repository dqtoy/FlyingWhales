using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintBehaviour : CharacterBehaviourComponent {
    public BlueprintBehaviour() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.INSIDE_SETTLEMENT_ONLY, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will try to place blueprint";
        if (character.homeArea.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT) < 2 && HasCharacterWithPlaceBlueprintJobInSettlement(character.homeArea) == false) {
            log += $"\n-{character.name} will roll for blueprint placement.";
            int chance = 35;
            int roll = Random.Range(0, 100);
            log += $"\n-Roll is {roll.ToString()}, chance is {chance.ToString()}";
            if (roll < chance) {
                log += $"\n-Roll successful";
                STRUCTURE_TYPE neededStructure = character.buildStructureComponent.GetCurrentStructureToBuild();
                log += $"\n-Structure Type to build is {neededStructure.ToString()}";

                List<GameObject> choices = InteriorMapManager.Instance.GetStructurePrefabsForStructure(neededStructure);
                GameObject chosenStructurePrefab = Utilities.GetRandomElement(choices);
                log += $"\n-Structure Prefab chosen is {chosenStructurePrefab.name}";

                LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
                BuildSpotTileObject chosenBuildingSpot;
                if (character.specificLocation.areaMap.TryGetValidBuildSpotTileObjectForStructure(lso, out chosenBuildingSpot) == false) {
                    log += $"\n-Could not find spot that can house new structure. Abandoning...";
                    return false;
                }

                log += $"\n-Creating new Place Blueprint job targetting {chosenBuildingSpot.ToString()} at {chosenBuildingSpot.gridTileLocation.ToString()}";
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, chosenBuildingSpot, character);
                job.AddOtherData(INTERACTION_TYPE.PLACE_BLUEPRINT, new object[] { neededStructure });
                character.jobQueue.AddJobInQueue(job);

                ////disable component for character
                //DisableFor(character);

                ////schedule enable for start of next day
                //GameDate today = GameManager.Instance.Today();
                //GameDate nextDay = today.AddDays(1);
                //nextDay.SetTicks(1);
                //SchedulingManager.Instance.AddEntry(nextDay, () => EnableFor(character), this);

                return true;
            }
        }
        log += $"\n-{character.name} failed to place blueprint";
        return false;
    }

    private bool HasCharacterWithPlaceBlueprintJobInSettlement(Area area) {
        for (int i = 0; i < area.region.residents.Count; i++) {
            Character resident = area.region.residents[i];
            if (resident.jobQueue.HasJob(JOB_TYPE.PLACE_BLUEPRINT)) {
                return true;
            }
        }
        return false;
    }
}
