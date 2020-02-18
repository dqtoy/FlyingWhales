using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class RaidBehaviour : CharacterBehaviourComponent {
    public RaidBehaviour() {
        priority = 0;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will raid";
        if (character.gridTileLocation.buildSpotOwner.hexTileOwner != null && character.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile == character.behaviourComponent.harassInvadeRaidTarget) {
            log += "\n-Already in the target settlement";
            if (character.marker.inVisionTileObjects.Count > 0) {
                TileObject target = null;
                for (int i = 0; i < character.marker.inVisionTileObjects.Count; i++) {
                    TileObject tileObject = character.marker.inVisionTileObjects[i];
                    if(tileObject is Artifact || tileObject is ResourcePile) {
                        target = tileObject;
                        break;
                    }
                }
                if(target != null) {
                    log += "\n-Has artifact or resource pile in vision, 50% to destroy tile object";
                    int roll = UnityEngine.Random.Range(0, 100);
                    log += $"\n-Roll: {roll}";
                    if (roll < 50) {
                        log += "\n-Destroying a random tile object";
                        if (target is Artifact) {
                            log += "\n-Tile object is an artifact, will carry to portal instead";
                            character.jobComponent.CreateTakeArtifactJob(target, PlayerManager.Instance.player.portalTile.locationGridTiles[0].structure);
                        } else {
                            log += "\n-Tile object is a resource pile, will destroy 100 amount";
                            character.jobComponent.CreateDestroyResourceAmountJob(target as ResourcePile, 100);
                        }
                    } else {
                        log += "\n-Roam";
                        character.jobComponent.TriggerRoamAroundTile();
                    }
                } else {
                    log += "\n-No artifact or resource pile in vision";
                    log += "\n-Roam";
                    character.jobComponent.TriggerRoamAroundTile();
                }
            } else {
                log += "\n-No tile object in vision";
                log += "\n-Roam";
                character.jobComponent.TriggerRoamAroundTile();
            }
        } else {
            log += "\n-Is not in the target settlement";
            log += "\n-Roam there";
            HexTile targetHex = character.behaviourComponent.harassInvadeRaidTarget.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.harassInvadeRaidTarget.tiles.Count)];
            LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
            character.jobComponent.TriggerRoamAroundTile(targetTile);

        }
        return true;
    }
}