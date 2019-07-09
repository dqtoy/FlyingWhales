using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace worldcreator {
    public class UnitSelectionComponent : MonoBehaviour {
        bool isSelecting = false;
        Vector2 originDragMousePosition;
        Vector2 originDragWorldPosition;
        [SerializeField] private List<HexTile> highlightedTiles = new List<HexTile>();

        private HexTile dragStartTile;

        #region getters/setters
        public List<HexTile> selection {
            get { return highlightedTiles; }
        }
        public List<HexTile> nonOuterSelection {
            get { return highlightedTiles.Where(x => !WorldCreatorManager.Instance.outerGridList.Contains(x)).ToList(); }
        }
        public List<BaseLandmark> selectedLandmarks {
            get { return GetSelectedLandmarks(); }
        }
        public List<Area> selectedAreas {
            get { return GetSelectedAreas(); }
        }
        #endregion

        private void Awake() {
            Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnTileLeftClicked);
            Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, OnTileRightClicked);
            Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
            Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);
        }


        private void Update() {
            if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.RECTANGLE && !WorldCreatorUI.Instance.IsMouseOnUI()) {
                // If we press the left mouse button, save mouse location and begin selection
                if (Input.GetMouseButtonDown(0)) {
                    isSelecting = true;
                    originDragMousePosition = Input.mousePosition;
                    originDragWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                // If we let go of the left mouse button, end selection
                if (Input.GetMouseButtonUp(0)) {
                    isSelecting = false;
                }
            }

            if (isSelecting) {
                ClearSelectedTiles();
                Collider2D[] colliders = Physics2D.OverlapAreaAll(originDragWorldPosition, Camera.main.ScreenToWorldPoint(Input.mousePosition), LayerMask.GetMask("Hextiles"));
                for (int i = 0; i < colliders.Length; i++) {
                    Collider2D currCollider = colliders[i];
                    HexTile tile = currCollider.gameObject.GetComponent<HexTile>();
                    if (tile != null) {
                        AddToHighlightedTiles(tile);
                        //tile.HighlightTile(Color.gray, 128f/255f);
                    }
                }
            }

            //if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A)) {
            //    //select all
            //    Debug.Log("Select All");
            //    ClearSelectedTiles();
            //    AddToHighlightedTiles(WorldCreatorManager.Instance.allTiles);
            //}

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
                //Draw the selection rectangle
                var rect = Utilities.GetScreenRect(originDragMousePosition, Input.mousePosition);
                Utilities.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utilities.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

                
                //Dragging();
                //List<HexTile> selectedTiles = new List<HexTile>(highlightedTiles);
                //ClearSelectedTiles();
                //for (int i = 0; i < selectedTiles.Count; i++) {
                //    HexTile currTile = selectedTiles[i];
                //    if (IsWithinSelectionBounds(currTile.gameObject)) {
                //        AddToHighlightedTiles(currTile);
                //        currTile.HighlightTile(Color.gray, 128f/255f);
                //    }
                //}
            }
        }

        private List<HexTile> GetSelection(HexTile originTile, HexTile otherTile) {
            List<HexTile> selected = new List<HexTile>();
            int minX = Mathf.Min(originTile.xCoordinate, otherTile.xCoordinate);
            int maxX = Mathf.Max(originTile.xCoordinate, otherTile.xCoordinate);
            int minY = Mathf.Min(originTile.yCoordinate, otherTile.yCoordinate);
            int maxY = Mathf.Max(originTile.yCoordinate, otherTile.yCoordinate);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    HexTile selectedTile = WorldCreatorManager.Instance.map[x, y];
                    selected.Add(selectedTile);
                }
            }
            return selected;
        }

        public bool IsWithinSelectionBounds(GameObject gameObject) {
            if (!isSelecting)
                return false;

            var camera = Camera.main;
            var viewportBounds =
                Utilities.GetViewportBounds(camera, originDragMousePosition, Input.mousePosition);

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
            }
        }
        private void OnTileRightClicked(HexTile clickedTile) {
            if (WorldCreatorManager.Instance.selectionMode == SELECTION_MODE.TILE) {
                highlightedTiles.Remove(clickedTile);
                clickedTile.UnHighlightTile();
            }
        }
        private void OnHoverOverTile(HexTile tile) {
            switch (WorldCreatorManager.Instance.selectionMode) {
                case SELECTION_MODE.RECTANGLE:
                case SELECTION_MODE.TILE:
                    tile.HighlightTile(Color.grey, 128f/255f);
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
        private List<BaseLandmark> GetSelectedLandmarks() {
            List<BaseLandmark> landamrks = new List<BaseLandmark>();
            for (int i = 0; i < highlightedTiles.Count; i++) {
                HexTile currTile = highlightedTiles[i];
                if (currTile.landmarkOnTile != null && !landamrks.Contains(currTile.landmarkOnTile)) {
                    landamrks.Add(currTile.landmarkOnTile);
                }
            }
            return landamrks;
        }

        private List<Area> GetSelectedAreas() {
            List<Area> areas = new List<Area>();
            for (int i = 0; i < highlightedTiles.Count; i++) {
                HexTile currTile = highlightedTiles[i];
                if (currTile.areaOfTile != null && !areas.Contains(currTile.areaOfTile)) {
                    areas.Add(currTile.areaOfTile);
                }
            }
            return areas;
        }
    }
}
