﻿using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridMap : MonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;

	public float width;
	public float height;

	public float xOffset;
	public float yOffset;

	public int tileSize;

	public float elevationFrequency;
	public float moistureFrequency;

	public List<GameObject> listHexes;

	public HexTile[,] map;

	void Awake(){
		Instance = this;
	}

	internal void GenerateGrid () {
		map = new HexTile[(int)width, (int)height];
		listHexes = new List<GameObject>();
        int id = 1;
		for (int x = 0;  x < width; x++){
			for(int y = 0; y < height; y++){
				float xPosition = x * xOffset;

				float yPosition = y * yOffset;
				if (y % 2 == 1) {
					xPosition += xOffset / 2;
				}
				GameObject hex = GameObject.Instantiate(goHex) as GameObject;
				hex.transform.parent = this.transform;
				hex.transform.position = new Vector3(xPosition, yPosition,0f);
				hex.transform.localScale = new Vector3(tileSize,tileSize,0f);
				hex.name = x + "," + y;
                HexTile currHex = hex.GetComponent<HexTile>();
                currHex.id = id;
                currHex.tileName = hex.name;
                currHex.xCoordinate = x;
                currHex.yCoordinate = y;
//				int sortingOrder = x - y;
//				hex.GetComponent<HexTile>().SetSortingOrder(sortingOrder);
				listHexes.Add(hex);
				map[x, y] = hex.GetComponent<HexTile>();
                id++;
			}
		}
		listHexes.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(map));
	}

	internal GameObject GetHex(string hexName){
		for(int i = 0; i < listHexes.Count; i++){
			if(hexName == listHexes[i].name){
				return listHexes[i];
			}
		}
		return null;
	}

    public void GenerateNeighboursWithSameTag() {
        for (int i = 0; i < listHexes.Count; i++) {
            HexTile currHex = listHexes[i].GetComponent<HexTile>();
            currHex.sameTagNeighbours = currHex.AllNeighbours.Where(x => x.tileTag == currHex.tileTag).ToList();
        }
    }

    
    public List<HexTile> GetTilesInRange(HexTile center, int range) {
        List<HexTile> tilesInRange = new List<HexTile>();
        Vector3 cube = OddRToCube(new Vector2(center.xCoordinate, center.yCoordinate));
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx -range); dy <= Mathf.Min(range, -dx + range); dy++) {
				Debug.Log (((int)cube.x + dx) + " , " + ((int)cube.y + dy));
//                tilesInRange.Add(map[(int)cube.x + dx, (int)cube.y + dy]);
            }
        }
        return tilesInRange.Distinct().ToList();
    }

    private Vector2 CubeToOddR(Vector3 cube) {
        int col = (int)(cube.x + (cube.z - ((int)cube.z & 1)) / 2);
        int row = (int)cube.z;
        return new Vector2(col, row);
    }

    public Vector3 OddRToCube(Vector2 hex) {
        int x = (int)(hex.x - (hex.y - ((int)hex.y & 1)) / 2);
        int z = (int)hex.y;
        int y = -x - z;
        return new Vector3(x, y, z);
    }

    public void GenerateRegions(int numOfRegions, int refinementLevel) {
        List<HexTile> allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
        List<HexTile> possibleCenterTiles = new List<HexTile>(allHexTiles);
        HexTile[] initialCenters = new HexTile[numOfRegions];
        Region[] allRegions = new Region[numOfRegions];
        for (int i = 0; i < numOfRegions; i++) {
            if(allHexTiles.Count <= 0) {
                throw new System.Exception("All tiles have been used up!");
            }
            HexTile chosenHexTile = possibleCenterTiles[Random.Range(0, possibleCenterTiles.Count)];
            possibleCenterTiles.Remove(chosenHexTile);
            allHexTiles.Remove(chosenHexTile);
            initialCenters[i] = chosenHexTile;
            Region newRegion = new Region(chosenHexTile);
            allRegions[i] = newRegion;
            //Color centerOfMassColor = newRegion.regionColor;
            //centerOfMassColor.a = 75.6f / 255f;
            //chosenHexTile.SetTileHighlightColor(centerOfMassColor);
            //chosenHexTile.ShowTileHighlight();
            foreach (HexTile hex in chosenHexTile.GetTilesInRange(8)) {
                possibleCenterTiles.Remove(hex);
            }
        }
        Debug.Log("Successfully got " + initialCenters.Length.ToString() + " center of masses!");

        for (int i = 0; i < refinementLevel; i++) {
            if(i != 0) {
                allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
                for (int j = 0; j < allRegions.Length; j++) {
                    allRegions[j].ReComputeCenterOfMass();
                    allRegions[j].ResetTilesInRegion();
                    allHexTiles.Remove(allRegions[j].centerOfMass);
                }
            }
            for (int j = 0; j < allHexTiles.Count; j++) {
                HexTile currHexTile = allHexTiles[j];
                Region regionClosestTo = null;
                float closestDistance = 999999f;
                for (int k = 0; k < allRegions.Length; k++) {
                    Region currRegion = allRegions[k];
                    HexTile currCenter = currRegion.centerOfMass;
                    float distance = Vector2.Distance(currHexTile.transform.position, currCenter.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        regionClosestTo = currRegion;
                    }
                }
                if (regionClosestTo != null) {
                    regionClosestTo.AddTile(currHexTile);
                    //currHexTile.SetTileHighlightColor(regionClosestTo.regionColor);
                    //currHexTile.ShowTileHighlight();
                } else {
                    throw new System.Exception("Could not find closest distance for tile " + currHexTile.name);
                }

            }
        }

        for (int i = 0; i < allRegions.Length; i++) {
            allRegions[i].RevalidateCenterOfMass();
        }
        
    }
}
