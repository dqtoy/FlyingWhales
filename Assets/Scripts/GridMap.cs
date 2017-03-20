using UnityEngine;
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

//	public Tile[,] GameBoard;

	void Awake(){
		Instance = this;
	}

	internal void GenerateGrid () {
//		GameBoard = new Tile[(int)width, (int)height];
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
				hex.GetComponent<HexTile> ().name = hex.name;
				listHexes.Add(hex);
//				hex.GetComponent<HexTile>().tile = new Tile (x, y, hex.GetComponent<HexTile>());
//				hex.GetComponent<HexTile>().GenerateResourceValues(); //TODO: Relocate to when biomes are chosen
//				GameBoard [x, y] = hex.GetComponent<HexTile>().tile;
			}
		}
//		listHexes.ForEach(o => o.GetComponent<HexTile>().tile.FindNeighbours(GameBoard));
	}

	public void FindEveryTilesNeighbour(){
//		listHexes.ForEach(o => o.GetComponent<HexTile>().tile.FindNeighbours(GameBoard));
	}

	internal GameObject GetHex(string hexName){
		for(int i = 0; i < listHexes.Count; i++){
			if(hexName == listHexes[i].name){
				return listHexes[i];
			}
		}
		return null;
	}

//	public IEnumerable<Tile> AllTiles
//	{
//		get
//		{
//			for (var x = 0; x < width; x++)
//				for (var y = 0; y < height; y++)
//					yield return GameBoard[x, y];
//		}
//	}
}
