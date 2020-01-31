using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class InnerMapAssetManager : MonoBehaviour {

        [Header("Grassland Tiles")]
        public TileBase outsideTile;
        public TileBase grassTile;
        public TileBase soilTile;
        public TileBase stoneTile;
        public TileBase shrubTile;
        public TileBase flowerTile;
        public TileBase rockTile;
        public TileBase randomGarbTile;

        [Header("Snow Tiles")]
        public TileBase snowOutsideTile;
        public TileBase snowTile;
        public TileBase tundraTile;
        public TileBase snowDirt;
        public TileBase snowFlowerTile;
        public TileBase snowGarbTile;
        
        [Header("Desert Tiles")]
        public TileBase desertOutsideTile;
        public TileBase desertGrassTile;
        public TileBase desertSandTile;
        public TileBase desertStoneGroundTile;
        public TileBase desertFlowerTile;
        public TileBase desertGarbTile;
        public TileBase desertRockTile;
        
        [Header("Inside Detail Tiles")]
        public TileBase crateBarrelTile;

        [Header("Seamless Edges")]
        public SeamlessEdgeAssetsDictionary edgeAssets; //0-north, 1-south, 2-west, 3-east

        [Header("Water Tiles")] 
        public TileBase waterTle;
        public TileBase shoreTle;

        [Header("Cave Tiles")] 
        public TileBase caveWallTile;
        public TileBase caveGroundTile;
        
        [Header("Monster Lair Tiles")]
        public TileBase monsterLairWallTile;
        public TileBase monsterLairGroundTile;
        
        [Header("Corrupted Tiles")] 
        public TileBase corruptedTile;
        public TileBase corruptedDetailTile;
        public Sprite[] corruptedTreeAssets;
        public Sprite[] corruptedBigTreeAssets;
        
        public TileBase GetOutsideFloorTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return snowOutsideTile;
                case BIOMES.DESERT:
                    return desertOutsideTile;
                default:
                    return outsideTile;
            }
        }
    }
}