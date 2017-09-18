using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridMap : MonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;
    [Space(10)]
    [Header("Map Settings")]
    public float width;
	public float height;

    public float xOffset;
    public float yOffset;

    public int tileSize;

    public float elevationFrequency;
    public float moistureFrequency;

    [Space(10)]
    [Header("Region Settings")]
    public int numOfRegions;
    public int refinementLevel;

    [Space(10)]
	public List<GameObject> listHexes;
    public List<Region> allRegions;
	public HexTile[,] map;

	void Awake(){
		Instance = this;
	}

	internal void GenerateGrid () {
        float newX = xOffset * (width / 2);
        float newY = yOffset * (height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        //CameraMove.Instance.minimapCamera.transform.position
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
				hex.transform.localPosition = new Vector3(xPosition, yPosition,0f);
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
        CubeCoordinate cube = OddRToCube(new HexCoordinate(center.xCoordinate, center.yCoordinate));
        Debug.Log("Center in cube coordinates: " + cube.x.ToString() + "," + cube.y.ToString() + "," + cube.z.ToString());
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx-range); dy <= Mathf.Min(range, -dx+range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
                //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
                if(hex.col >= 0 && hex.row >= 0 && !(hex.col == center.xCoordinate && hex.row == center.yCoordinate)) {
                    tilesInRange.Add(map[hex.col, hex.row]);
                }
            }
        }
        return tilesInRange;
    }

    public HexCoordinate CubeToOddR(CubeCoordinate cube) {
        int modifier = 0;
        if(cube.z % 2 == 1) {
            modifier = 1;
        }
        int col = cube.x + (cube.z - (modifier)) / 2;
        int row = cube.z;
        return new HexCoordinate(col, row);
    }

    public CubeCoordinate OddRToCube(HexCoordinate hex) {
        int modifier = 0;
        if (hex.row % 2 == 1) {
            modifier = 1;
        }

        int x = hex.col - (hex.row - (modifier)) / 2;
        int z = hex.row;
        int y = -x - z;
        return new CubeCoordinate(x, y, z);
    }

    public void GenerateRegions(int numOfRegions, int refinementLevel) {
        List<HexTile> allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
        List<HexTile> possibleCenterTiles = new List<HexTile>(allHexTiles);
        HexTile[] initialCenters = new HexTile[numOfRegions];
        allRegions = new List<Region>();
        for (int i = 0; i < numOfRegions; i++) {
            if(possibleCenterTiles.Count <= 0) {
                throw new System.Exception("All tiles have been used up!");
            }
            HexTile chosenHexTile = possibleCenterTiles[Random.Range(0, possibleCenterTiles.Count)];
            possibleCenterTiles.Remove(chosenHexTile);
            allHexTiles.Remove(chosenHexTile);
            initialCenters[i] = chosenHexTile;
            Region newRegion = new Region(chosenHexTile);
            allRegions.Add(newRegion);
            //Color centerOfMassColor = newRegion.regionColor;
            //centerOfMassColor.a = 75.6f / 255f;
            //chosenHexTile.SetTileHighlightColor(centerOfMassColor);
            //chosenHexTile.ShowTileHighlight();
            foreach (HexTile hex in chosenHexTile.GetTilesInRange(5)) {
                possibleCenterTiles.Remove(hex);
            }
        }
        Debug.Log("Successfully got " + initialCenters.Length.ToString() + " center of masses!");

        for (int i = 0; i < refinementLevel; i++) {
            if(i != 0) {
                allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
                for (int j = 0; j < allRegions.Count; j++) {
                    allRegions[j].ReComputeCenterOfMass();
                    allRegions[j].ResetTilesInRegion();
                    allHexTiles.Remove(allRegions[j].centerOfMass);
                }
            }
            for (int j = 0; j < allHexTiles.Count; j++) {
                HexTile currHexTile = allHexTiles[j];
                Region regionClosestTo = null;
                float closestDistance = 999999f;
                for (int k = 0; k < allRegions.Count; k++) {
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

        for (int i = 0; i < allRegions.Count; i++) {
            allRegions[i].RevalidateCenterOfMass();
            allRegions[i].CheckForAdjacency();
        }
        
    }

    public void GenerateResourcesPerRegion() {
        List<RESOURCE> allSpecialResources = Utilities.GetEnumValues<RESOURCE>().ToList();
        allSpecialResources.Remove(RESOURCE.NONE);
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if(Random.Range(0,100) < 50) {
                //Region has a special resource
                if(allSpecialResources.Count <= 0) {
                    allSpecialResources = Utilities.GetEnumValues<RESOURCE>().ToList();
                    allSpecialResources.Remove(RESOURCE.MANA_STONE);
                    allSpecialResources.Remove(RESOURCE.COBALT);
                    allSpecialResources.Remove(RESOURCE.MITHRIL);
                    allSpecialResources.Remove(RESOURCE.NONE);
                }
                RESOURCE specialResource = allSpecialResources[Random.Range(0, allSpecialResources.Count)];
                allSpecialResources.Remove(specialResource);
                currRegion.SetSpecialResource(specialResource);
            }
            currRegion.ComputeNaturalResourceLevel(); //Compute For Natural Resource Level of current region
        }
        //Debug.Log("All Special Resources Per Region:");
        //for (int i = 0; i < allRegions.Count; i++) {
        //    Debug.Log("Region " + i.ToString() + ": " + allRegions[i].specialResource.ToString());
        //}
    }

    public void UpdateAllRegionsDiscoveredKingdoms() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if(currRegion.occupant != null) {
                currRegion.CheckForDiscoveredKingdoms();
            }
        }
    }
}
