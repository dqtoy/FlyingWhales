using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Table : TileObject {
    //private Character[] users;
    public TileBase usedAsset { get; private set; }
    public int food { get; private set; }
    //private int slots {
    //    get { return users.Length;}
    //}

    public Table(LocationStructure location) {
        SetStructureLocation(location);
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_AT_TABLE, INTERACTION_TYPE.DRINK, INTERACTION_TYPE.TABLE_REMOVE_POISON, INTERACTION_TYPE.TABLE_POISON, INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        SetFood(UnityEngine.Random.Range(20, 81));
        Initialize(TILE_OBJECT_TYPE.TABLE);
        //int slots = 4;
        //if (usedAsset.name.Contains("2")) {
        //    slots = 2;
        //} else if (usedAsset.name.Contains("Bartop")) {
        //    slots = 1;
        //}
        //users = new Character[slots];
    }

    public Table(SaveDataTileObject data) {
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_AT_TABLE, INTERACTION_TYPE.DRINK, INTERACTION_TYPE.TABLE_REMOVE_POISON, INTERACTION_TYPE.TABLE_POISON, INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(data);
    }
    public void SetUsedAsset(TileBase usedAsset) {
        this.usedAsset = usedAsset;
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        //if (IsSlotAvailable()) {
        //    //if (GetActiveUserCount() > 0) {
        //    UpdateUsedTableAsset();
        //    //} else {
        //    //    gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
        //    //}
        //}
    }
    public override string ToString() {
        return "Table " + id.ToString();
    }
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT_AT_TABLE:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                AddUser(action.actor);
                break;

        }
    }
    public override void OnDoneActionToObject(GoapAction action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT_AT_TABLE:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override void OnCancelActionTowardsObject(GoapAction action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT_AT_TABLE:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override bool CanBeReplaced() {
        return true;
    }
    #endregion

    #region Users
    //private void AddUser(Character character) {
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] == null) {
    //            users[i] = character;
    //            UpdateUsedTableAsset();
    //            if (!IsSlotAvailable()) {
    //                SetPOIState(POI_STATE.INACTIVE); //if all slots in the table are occupied, set it as inactive
    //            }
    //            ////disable the character's marker
    //            //character.marker.SetActiveState(false);
    //            //place the character's marker his/her appropriate slot
    //            Vector3 pos = GetPositionForUser(GetActiveUserCount());
    //            character.marker.pathfindingAI.AdjustDoNotMove(1);

    //            Vector3 worldPos = character.marker.transform.TransformPoint(pos);
    //            //Debug.Log("Setting " + character.marker.name + "'s position to " + pos.ToString() + " world pos: " + worldPos.ToString());
    //            if (usedAsset.name.Contains("Bartop")) {
    //                character.marker.PlaceMarkerAt(pos, tile.parentAreaMap.objectsTilemap.GetTransformMatrix(tile.localPlace).rotation);
    //            } else {
    //                character.marker.PlaceMarkerAt(pos, gridTileLocation.centeredWorldLocation);
    //            }
    //            //Debug.Log(character.marker.name + "'s position is " + character.marker.transform.position.ToString());
    //            //character.marker.LookAt(this.gridTileLocation.worldLocation);
    //            break;
    //        }
    //    }
    //}
    //private void RemoveUser(Character character) {
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] == character) {
    //            users[i] = null;
    //            if (IsSlotAvailable()) {
    //                SetPOIState(POI_STATE.ACTIVE); //if a slot in the table is unoccupied, set it as active
    //            }
    //            character.marker.pathfindingAI.AdjustDoNotMove(-1);
    //            ////enable the character's marker
    //            //character.marker.SetActiveState(true);
    //            if (GetActiveUserCount() > 0) {
    //                UpdateAllActiveUsersPosition();
    //            }
    //            UpdateUsedTableAsset();
    //            break;
    //        }
    //    }
    //}
    //private int GetActiveUserCount() {
    //    int count = 0;
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] != null) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    //private Vector3 GetPositionForUser(int positionInTable) {
    //    Vector3 pos = gridTileLocation.localPlace;
    //    if (slots == 1) {
    //        //concerned with rotation in the 1 slot variant
    //        Matrix4x4 m = structureLocation.location.areaMap.objectsTilemap.GetTransformMatrix(gridTileLocation.localPlace);
    //        int rotation = (int)m.rotation.eulerAngles.z;
    //        if (usedAsset.name.Contains("Bartop")) {
    //            if (usedAsset.name.Contains("Left")) {
    //                if (rotation == 0 || rotation == 360) {
    //                    pos.x += 0.55f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.55f;
    //                } else if (rotation == 180) {
    //                    pos.x += 0.45f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 270 || rotation == -90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.5f;
    //                }
    //            } else {
    //                if (rotation == 0 || rotation == 360) {
    //                    pos.x += 0.45f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.45f;
    //                } else if (rotation == 180) {
    //                    pos.x += 0.55f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 270 || rotation == -90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.55f;
    //                }
    //            }
    //        } else {
    //            if (rotation == 0 || rotation == 360) {
    //                pos.x += 0.49f;
    //                pos.y += 0.2f;
    //            } else if (rotation == 90) {
    //                pos.x += 0.8f;
    //                pos.y += 0.5f;
    //            } else if (rotation == 180) {
    //                pos.x += 0.51f;
    //                pos.y += 0.8f;
    //            } else if (rotation == 270) {
    //                pos.x += 0.2f;
    //                pos.y += 0.51f;
    //            }
    //        }
    //    } else if (slots == 2) {
    //        //concerned with rotation in the 2 slot variant
    //        Matrix4x4 m = structureLocation.location.areaMap.objectsTilemap.GetTransformMatrix(gridTileLocation.localPlace);
    //        float rotation = m.rotation.eulerAngles.z / 90f;
    //        if (Utilities.IsEven((int)rotation)) {
    //            //table is vertical, I assume that if the table is vertical, it has a rotation of 0 degrees
    //            if (positionInTable == 1) {
    //                pos.x += 0.48f;
    //            } else {
    //                pos.x += 0.45f;
    //                pos.y += 1f;
    //            }
    //        } else {
    //            //table is horizontal, I assume that if the table is horizontal, it only has a rotation of 90 degrees
    //            if (positionInTable == 1) {
    //                pos.x += 1f;
    //                pos.y += 0.45f;
    //            } else {
    //                pos.y += 0.48f;
    //            }
    //        }
    //    } else if (slots == 4) {
    //        switch (positionInTable) {
    //            case 1:
    //                //left side
    //                pos.y += 0.53f;
    //                break;
    //            case 2:
    //                //right side
    //                pos.y += 0.53f;
    //                pos.x += 1f;
    //                break;
    //            case 3:
    //                //top
    //                pos.y += 1f;
    //                pos.x += 0.48f;
    //                break;
    //            case 4:
    //                //bottom
    //                pos.x += 0.48f;
    //                break;
    //            default:
    //                break;
    //        }
    //    }

    //    pos.z = 0;

    //    return pos;
    //}
    //private void UpdateAllActiveUsersPosition() {
    //    if (gridTileLocation == null) {
    //        return;
    //    }
    //    int userCount = 0;
    //    for (int i = 0; i < users.Length; i++) {
    //        Character currUser = users[i];
    //        if (currUser != null) {
    //            userCount++;
    //            Vector3 pos = GetPositionForUser(userCount);
    //            currUser.marker.PlaceMarkerAt(pos, gridTileLocation.centeredWorldLocation);
    //            //currUser.marker.LookAt(this.gridTileLocation.worldLocation);
    //        }
    //    }
    //}
    #endregion

    #region Food
    public void AdjustFood(int amount) {
        food += amount;
        if (food < 0) {
            food = 0;
        }
    }
    public void SetFood(int amount) {
        food = amount;
        if (food < 0) {
            food = 0;
        }
    }
    #endregion

    //private void UpdateUsedTableAsset() {
    //    if (gridTileLocation == null) {
    //        return;
    //    }
    //    int userCount = GetActiveUserCount();
    //    if (userCount == 1) {
    //        if (usedAsset.name.Contains("0")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table01);
    //        } else if (usedAsset.name.Contains("1")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table11);
    //        } else if (usedAsset.name.Contains("2")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table21);
    //        } else if (usedAsset.name.Contains("Left")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bartopLeft1);
    //        } else if (usedAsset.name.Contains("Right")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bartopRight1);
    //        }
    //    } else if (userCount == 2) {
    //        if (usedAsset.name.Contains("0")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table02);
    //        } else if (usedAsset.name.Contains("1")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table12);
    //        } else if (usedAsset.name.Contains("2")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table22);
    //        }
    //    } else if (userCount == 3) {
    //        if (usedAsset.name.Contains("0")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table03);
    //        } else if (usedAsset.name.Contains("1")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table13);
    //        }
    //    } else if (userCount == 4) {
    //        if (usedAsset.name.Contains("0")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table04);
    //        } else if (usedAsset.name.Contains("1")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table14);
    //        }
    //    } else {
    //        if (usedAsset.name.Contains("0")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table00);
    //        } else if (usedAsset.name.Contains("1")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table10);
    //        } else if (usedAsset.name.Contains("2")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.table20);
    //        } else if (usedAsset.name.Contains("Left")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bartopLeft0);
    //        } else if (usedAsset.name.Contains("Right")) {
    //            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bartopRight0);
    //        }
    //    }
    //    //the asset will revert to no one using once the table is set to active again
    //}
}

public class SaveDataTable : SaveDataTileObject {
    public int food;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Table obj = tileObject as Table;
        food = obj.food;
    }

    public override TileObject Load() {
        Table obj = base.Load() as Table;
        obj.SetFood(food);
        return obj;
    }
}