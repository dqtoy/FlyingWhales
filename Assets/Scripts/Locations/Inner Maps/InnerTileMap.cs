using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Inner_Maps {
    public abstract class InnerTileMap : MonoBehaviour {
        
        public static int WestEdge = 7;
        public static int NorthEdge = 1;
        public static int SouthEdge = 1;
        public static int EastEdge = 2;
        
        [Header("Tile Maps")]
        public Tilemap groundTilemap;
        public Tilemap detailsTilemap;
        public Tilemap structureTilemap;
        
        [Header("Seamless Edges")]
        public Tilemap northEdgeTilemap;
        public Tilemap southEdgeTilemap;
        public Tilemap westEdgeTilemap;
        public Tilemap eastEdgeTilemap;
        
        [Header("Parents")]
        public Transform objectsParent;
        public Transform structureParent;
        
        public int width { get; set; }
        public int height { get; set; }
        public LocationGridTile[,] map { get; private set; }
        public List<LocationGridTile> allTiles { get; private set; }
        public List<LocationGridTile> allEdgeTiles { get; private set; }
        public ILocation location { get; private set; }
        public GridGraph pathfindingGraph { get; set; }
        public Vector3 worldPos { get; private set; }

        protected void GenerateGrid(int width, int height, ILocation location) {
            this.width = width;
            this.height = height;
            this.location = location;

            map = new LocationGridTile[width, height];
            allTiles = new List<LocationGridTile>();
            allEdgeTiles = new List<LocationGridTile>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), GetOutsideFloorTile(location));
                    LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this);
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                }
            }
            allTiles.ForEach(x => x.FindNeighbours(map));
        }

        #region Loading
        protected void LoadGrid(SaveDataAreaInnerTileMap data) {
            map = new LocationGridTile[width, height];
            allTiles = new List<LocationGridTile>();
            allEdgeTiles = new List<LocationGridTile>();

            Dictionary<string, TileBase> tileDb = InnerMapManager.Instance.GetTileAssetDatabase();

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    //groundTilemap.SetTile(new Vector3Int(x, y, 0), GetOutsideFloorTileForArea(area));
                    LocationGridTile tile = data.map[x][y].Load(groundTilemap, this, tileDb);
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                }
            }
            allTiles.ForEach(x => x.FindNeighbours(map));

            groundTilemap.RefreshAllTiles();
        }
        #endregion
        
        #region Visuals
        public void ClearAllTilemaps() {
            Tilemap[] maps = GetComponentsInChildren<Tilemap>();
            for (var i = 0; i < maps.Length; i++) {
                maps[i].ClearAllTiles();
            }
        }
        protected TileBase GetOutsideFloorTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowOutsideTile;
                default:
                    return InnerMapManager.Instance.assetManager.outsideTile;
            }
        }
        protected TileBase GetBigTreeTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowBigTreeTile;
                default:
                    return InnerMapManager.Instance.assetManager.bigTreeTile;
            }
        }
        protected TileBase GetTreeTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowTreeTile;
                default:
                    return InnerMapManager.Instance.assetManager.treeTile;
            }
        }
        protected TileBase GetFlowerTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowFlowerTile;
                default:
                    return InnerMapManager.Instance.assetManager.flowerTile;
            }
        }
        protected TileBase GetGarbTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowGarbTile;
                default:
                    return InnerMapManager.Instance.assetManager.randomGarbTile;
            }
        }
        #endregion

        #region Data Getting
        public List<LocationGridTile> GetUnoccupiedTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            int mapSizeX = map.GetUpperBound(0);
            int mapSizeY = map.GetUpperBound(1);
            int x = centerTile.localPlace.x;
            int y = centerTile.localPlace.y;
            if (includeCenterTile) {
                tiles.Add(centerTile);
            }

            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;

            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if (dx == x && dy == y) {
                            continue;
                        }
                        if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = map[dx, dy];
                        if ((!includeTilesInDifferentStructure && result.structure != centerTile.structure) || result.isOccupied || result.charactersHere.Count > 0) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        public List<LocationGridTile> GetTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            int mapSizeX = map.GetUpperBound(0);
            int mapSizeY = map.GetUpperBound(1);
            int x = centerTile.localPlace.x;
            int y = centerTile.localPlace.y;
            if (includeCenterTile) {
                tiles.Add(centerTile);
            }
            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;


            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if(dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if(dx == x && dy == y) {
                            continue;
                        }
                        if(radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = map[dx, dy];
                        if(result.structure == null) { continue; } //do not include tiles with no structures
                        if(!includeTilesInDifferentStructure && result.structure != centerTile.structure) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        #endregion
        
        #region Points of Interest
        public void PlaceObject(IPointOfInterest obj, LocationGridTile tile, bool placeAsset = true) {
            switch (obj.poiType) {
                case POINT_OF_INTEREST_TYPE.CHARACTER:
                    OnPlaceCharacterOnTile(obj as Character, tile);
                    break;
                default:
                    tile.SetObjectHere(obj);
                    break;
            }
        }
        public void RemoveObject(LocationGridTile tile, Character removedBy = null) {
            tile.RemoveObjectHere(removedBy);
        }
        public void RemoveObjectWithoutDestroying(LocationGridTile tile) {
            tile.RemoveObjectHereWithoutDestroying();
        }
        private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
            if (character.marker.gameObject.transform.parent != objectsParent) {
                //This means that the character travelled to a different area
                character.marker.gameObject.transform.SetParent(objectsParent);
                character.marker.gameObject.transform.localPosition = tile.centeredLocalLocation;
                character.marker.UpdatePosition();
            }

            if (!character.marker.gameObject.activeSelf) {
                character.marker.gameObject.SetActive(true);
            }
        }
        public void OnCharacterMovedTo(Character character, LocationGridTile to, LocationGridTile from) {
            if (from == null) { 
                //from is null (Usually happens on start up, should not happen otherwise)
                to.AddCharacterHere(character);
                to.structure.AddCharacterAtLocation(character);
            } else {
                if (to.structure == null) {
                    return; //quick fix for when the character is pushed to a tile with no structure
                }
                from.RemoveCharacterHere(character);
                to.AddCharacterHere(character);
                if (from.structure != to.structure) {
                    if (from.structure != null) {
                        from.structure.RemoveCharacterAtLocation(character);
                    }
                    if (to.structure != null) {
                        to.structure.AddCharacterAtLocation(character);
                    } else {
                        throw new System.Exception(character.name + " is going to tile " + to.ToString() + " which does not have a structure!");
                    }
                
                }
            }
        
        }
        #endregion

        #region Data Setting
        public void UpdateTilesWorldPosition() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    map[x, y].UpdateWorldLocation();
                }
            }
            SetWorldPosition();
        }
        public void SetWorldPosition() {
            worldPos = transform.position;
        }
        #endregion
    }
}
