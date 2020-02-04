using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSlot {
    public Vector3Int startPos;
    public Point size;
    public FurnitureSpot[] furnitureSpots;
    public void AdjustStartPos(int x, int y) {
        Vector3Int newPos = startPos;
        newPos.x += x;
        newPos.y += y;
        startPos = newPos;
    }
    public bool TryGetFurnitureSpot(Vector3Int location, out FurnitureSpot furnitureSpot) {
        for (int i = 0; i < furnitureSpots.Length; i++) {
            FurnitureSpot spot = furnitureSpots[i];
            if (spot.location == location) {
                furnitureSpot = spot;
                return true;
            }
        }
        furnitureSpot = default(FurnitureSpot);
        return false;
    }
}
