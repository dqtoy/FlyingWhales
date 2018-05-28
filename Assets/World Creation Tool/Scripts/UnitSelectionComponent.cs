using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace worldcreator {
    public class UnitSelectionComponent : MonoBehaviour {
        bool isSelecting = false;
        Vector3 originDragPosition;
        [SerializeField] private List<HexTile> highlightedTiles = new List<HexTile>();

        #region getters/setters
        public List<HexTile> selection {
            get { return highlightedTiles; }
        }
        public List<Region> selectedRegions {
            get { return GetSelectedRegions(); }
        }
        #endregion

        private void Awake() {
            Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnTileLeftClicked);
            Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, OnTileRightClicked);
            Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
            Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);
        }

        private void Update() {
            if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.RECTANGLE) {
                // If we press the left mouse button, save mouse location and begin selection
                if (Input.GetMouseButtonDown(0) && !WorldCreatorUI.Instance.IsMouseOnUI()) {
                    isSelecting = true;
                    originDragPosition = Input.mousePosition;
                }
                // If we let go of the left mouse button, end selection
                if (Input.GetMouseButtonUp(0)) {
                    isSelecting = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                ClearSelectedTiles();
            }

            if (highlightedTiles != null) {
                for (int i = 0; i < highlightedTiles.Count; i++) {
                    HexTile currTile = highlightedTiles[i];
                    currTile.HighlightTile(Color.gray, 128f/255f);
                }
            }
        }

        private void OnGUI() {
            if (isSelecting) {
                // Create a rect from both mouse positions
                var rect = Utilities.GetScreenRect(originDragPosition, Input.mousePosition);
                Utilities.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utilities.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

                ClearSelectedTiles();

                //RaycastHit2D[] hits = Physics2D.BoxCastAll(rect.min, Vector2.one, 0f, Vector2.right, 0f);
                //for (int i = 0; i < hits.Length; i++) {
                //    HexTile currTile = hits[i].collider.gameObject.GetComponent<HexTile>();
                //    if (currTile != null) {
                //        highlightedTiles.Add(currTile);
                //        currTile.HighlightTile(Color.gray);
                //    }
                //}
                for (int i = 0; i < WorldCreatorManager.Instance.hexTiles.Count; i++) {
                    HexTile currTile = WorldCreatorManager.Instance.hexTiles[i];
                    if (IsWithinSelectionBounds(currTile.gameObject)) {
                        AddToHighlightedTiles(currTile);
                        currTile.HighlightTile(Color.gray, 128f/255f);
                    }
                }
            } 
            //else {
            //    if (highlightedTiles != null) {
            //        for(int i = 0; i < highlightedTiles.Count; i++) {
            //            HexTile currTile = highlightedTiles[i];
            //            currTile.UnHighlightTile();
            //        }
            //        highlightedTiles.Clear();
            //    }
            //}
        }

        public bool IsWithinSelectionBounds(GameObject gameObject) {
            if (!isSelecting)
                return false;

            var camera = Camera.main;
            var viewportBounds =
                Utilities.GetViewportBounds(camera, originDragPosition, Input.mousePosition);

            return viewportBounds.Contains(
                camera.WorldToViewportPoint(gameObject.transform.position));
        }

        private void OnTileLeftClicked(HexTile clickedTile) {
            if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.TILE) {
                //if (highlightedTiles.Contains(clickedTile)) {
                //    highlightedTiles.Remove(clickedTile);
                //    clickedTile.UnHighlightTile();
                //} else {
                    AddToHighlightedTiles(clickedTile);
                    clickedTile.HighlightTile(Color.gray, 128f/255f);
                //}
            } else if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.REGION) {
                AddToHighlightedTiles(clickedTile.region.tilesInRegion);
                clickedTile.region.HighlightRegion(Color.gray, 128f/255f);
            }
        }
        private void OnTileRightClicked(HexTile clickedTile) {
            if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.TILE) {
                highlightedTiles.Remove(clickedTile);
                clickedTile.UnHighlightTile();
            } else if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.REGION) {
                RemoveFromHighlightedTiles(clickedTile.region.tilesInRegion);
                clickedTile.region.UnhighlightRegion();
            }
        }
        private void OnHoverOverTile(HexTile tile) {
            switch (WorldCreatorManager.Instance.selectionMode) {
                case SELECTION_MODE.RECTANGLE:
                case SELECTION_MODE.TILE:
                    tile.HighlightTile(Color.grey, 128f/255f);
                    break;
                case SELECTION_MODE.REGION:
                    tile.region.HighlightRegion(Color.grey, 128/255f);
                    break;
                default:
                    break;
            }
        }
        private void OnHoverOutTile(HexTile tile) {
            switch (WorldCreatorManager.Instance.selectionMode) {
                case SELECTION_MODE.RECTANGLE:
                case SELECTION_MODE.TILE:
                    if (!selection.Contains(tile)) {
                        tile.UnHighlightTile();
                    }
                    break;
                case SELECTION_MODE.REGION:
                    tile.region.UnhighlightRegion();
                    break;
                default:
                    break;
            }
        }
        public void ClearSelectedTiles() {
            if (highlightedTiles != null) {
                for (int i = 0; i < highlightedTiles.Count; i++) {
                    HexTile currTile = highlightedTiles[i];
                    currTile.UnHighlightTile();
                }
                highlightedTiles.Clear();
            }
        }

        private void AddToHighlightedTiles(List<HexTile> tile) {
            for (int i = 0; i < tile.Count; i++) {
                HexTile currTile = tile[i];
                AddToHighlightedTiles(currTile);
            }
        }
        private void AddToHighlightedTiles(HexTile tile) {
            if (!highlightedTiles.Contains(tile)) {
                highlightedTiles.Add(tile);
            }
        }

        private void RemoveFromHighlightedTiles(List<HexTile> tile) {
            for (int i = 0; i < tile.Count; i++) {
                HexTile currTile = tile[i];
                RemoveFromHighlightedTiles(currTile);
            }
        }
        private void RemoveFromHighlightedTiles(HexTile tile) {
            highlightedTiles.Remove(tile);
        }

        private List<Region> GetSelectedRegions() {
            List<Region> regions = new List<Region>();
            for (int i = 0; i < highlightedTiles.Count; i++) {
                HexTile currTile = highlightedTiles[i];
                if (!regions.Contains(currTile.region)) {
                    regions.Add(currTile.region);
                }
            }
            return regions;
        }
    }
}
