using UnityEngine;
using System.Collections;

public class HexTile : MonoBehaviour {
	public int xCoordinate;
	public int yCoordinate;

	public string name;

	public float elevationNoise;
	public float moistureNoise;
	public float temperature;

	public RESOURCE defaultResource;
	public RESOURCE specialResource;

	public BIOMES biomeType;
	public ELEVATION elevationType;

	public City city;

	public bool isHabitable = false;
	public bool isRoad = false;
	public bool isOccupied = false;


	public GameObject topLeft, topRight, right, bottomRight, bottomLeft, left;

	public GameObject leftGround;
	public GameObject bottomLeftGround;
	public GameObject rightGround;
	public GameObject bottomRightGround;
	public GameObject centerPiece;

	public GameObject leftBorder;
	public GameObject rightBorder;
	public GameObject topLeftBorder;
	public GameObject topRightBorder;
	public GameObject bottomLeftBorder;
	public GameObject bottomRightBorder;



	int[] allResourceValues;



//	public void GenerateTileDetails(){
//		List<Tile> neighbours = tile.AllNeighbours.ToList ();
//		for (int i = 0; i < neighbours.Count; i++) {
//
//			int neighbourX = neighbours [i].X;
//			int neighbourY = neighbours [i].Y;
//
//			Point difference = new Point((neighbours [i].X - this.tile.X), (neighbours [i].Y - this.tile.Y));
//			if (this.tile.Y % 2 == 0) {
//				if (difference.X == -1 && difference.Y == 1) {
//					//top left
//					if (neighbours[i].hexTile.biomeType != this.biomeType && neighbours[i].hexTile.elevationType != ELEVATION.WATER) {
//						this.topLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == 0 && difference.Y == 1) {
//					//top right
//					if (neighbours [i].hexTile.biomeType != this.biomeType && neighbours [i].hexTile.elevationType != ELEVATION.WATER) {
//						this.topRightBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == 0) {
//					//right
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.rightGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.rightBorder.SetActive (true);
//					}
//				} else if (difference.X == 0 && difference.Y == -1){
//					//bottom right
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.bottomRightGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.bottomRightBorder.SetActive (true);
//					}
//				} else if (difference.X == -1 && difference.Y == -1){
//					//bottom left
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.bottomLeftGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.bottomLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == -1 && difference.Y == 0){
//					//left
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.leftGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.leftBorder.SetActive (true);
//					}
//				}
//			} else {
//				if (difference.X == 0 && difference.Y == 1) {
//					//top left
//					if (neighbours [i].hexTile.biomeType != this.biomeType && neighbours [i].hexTile.elevationType != ELEVATION.WATER) {
//						this.topLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == 1) {
//					//top right
//					if (neighbours [i].hexTile.biomeType != this.biomeType && neighbours [i].hexTile.elevationType != ELEVATION.WATER) {
//						this.topRightBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == 0) {
//					//right
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.rightGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.rightBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == -1){
//					//bottom right
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.bottomRightGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.bottomRightBorder.SetActive (true);
//					}
//				} else if (difference.X == 0 && difference.Y == -1){
//					//bottom left
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.bottomLeftGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.bottomLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == -1 && difference.Y == 0){
//					//left
//					if (neighbours [i].hexTile.elevationType == ELEVATION.WATER) {
//						this.leftGround.SetActive (true);
//					} else if (neighbours [i].hexTile.biomeType != this.biomeType) {
//						this.leftBorder.SetActive (true);
//					}
//				}
//			}
//		}
//	}

//	public void SetTileSprites(Sprite baseSprite, Sprite leftSprite, Sprite rightSprite, Sprite leftCornerSprite, Sprite rightCornerSprite, Sprite[] centerSprite){
//		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
//		this.leftGround.GetComponent<SpriteRenderer>().sprite = leftSprite;
//		this.rightGround.GetComponent<SpriteRenderer>().sprite = rightSprite;
//		this.bottomLeftGround.GetComponent<SpriteRenderer>().sprite = leftCornerSprite;
//		this.bottomRightGround.GetComponent<SpriteRenderer>().sprite = rightCornerSprite;
//		if (this.elevationType == ELEVATION.MOUNTAIN) {
//			this.centerPiece.SetActive(true);
//		} else {
//			if (this.biomeType == BIOMES.GRASSLAND) {
//				return;
//			} else if (this.biomeType == BIOMES.WOODLAND || this.biomeType == BIOMES.FOREST) {
//				this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite[Random.Range(0, centerSprite.Length)];
//				this.centerPiece.SetActive(true);
//			} else {
//				int chanceForDetail = Random.Range (0, 100);
//				if (chanceForDetail < 25) {
//					this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite[Random.Range(0, centerSprite.Length)];
//					this.centerPiece.SetActive(true);
//					float randomXPosition = Random.Range(-1.30f, 1.30f);
//					float randomYPosition = Random.Range(-0.40f, 0.70f);
//					if (randomXPosition <= 0.45f && randomXPosition >= -0.45f) {
//						int chanceToModify = Random.Range(0, 100);
//						if (chanceToModify < 25) {
//							if (Mathf.Sign (randomYPosition) == 0) {
//								//negative
//								randomYPosition -= Random.Range(0.20f,0.40f);
//							} else {
//								//positive
//								randomYPosition += Random.Range(0.20f,0.40f);
//							}
//						}
//					}
//					this.centerPiece.transform.localPosition = new Vector3(randomXPosition, randomYPosition, 0f);
//				}
//			}
//
//		}
//	}
}
