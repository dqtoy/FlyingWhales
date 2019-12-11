using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class InnerMapAssetManager : MonoBehaviour {
        
        [Header("Tiles")]
        public TileBase outsideTile;
        public TileBase snowOutsideTile;

        [Header("Outside Tiles")]
        public TileBase grassTile;
        public TileBase soilTile;
        public TileBase stoneTile;

        [Header("Snow Outside Tiles")]
        public TileBase snowTile;
        public TileBase tundraTile;
        public TileBase snowDirt;

        [Header("Outside Detail Tiles")]
        public TileBase bigTreeTile;
        public TileBase treeTile;
        public TileBase shrubTile;
        public TileBase flowerTile;
        public TileBase rockTile;
        public TileBase randomGarbTile;

        [Header("Snow Detail Tiles")]
        public TileBase snowBigTreeTile;
        public TileBase snowTreeTile;
        public TileBase snowFlowerTile;
        public TileBase snowGarbTile;

        [Header("Inside Detail Tiles")]
        public TileBase crateBarrelTile;

        [Header("Seamless Edges")]
        public SeamlessEdgeAssetsDictionary edgeAssets; //0-north, 1-south, 2-west, 3-east
    }
}