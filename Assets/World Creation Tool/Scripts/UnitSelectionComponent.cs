using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace worldcreator {
    public class UnitSelectionComponent : MonoBehaviour {
        bool isSelecting = false;
        Vector3 originDragPosition;
        private List<HexTile> highlightedTiles = new List<HexTile>();

        #region getters/setters
        public List<HexTile> selection {
            get { return highlightedTiles; }
        }
        #endregion

        void Update() {
            // If we press the left mouse button, save mouse location and begin selection
            if (Input.GetMouseButtonDown(0) && !WorldCreatorUI.Instance.IsMouseOnUI()) {
                isSelecting = true;
                originDragPosition = Input.mousePosition;
            }
            // If we let go of the left mouse button, end selection
            if (Input.GetMouseButtonUp(0))
                isSelecting = false;
        }

        void OnGUI() {
            if (isSelecting) {
                // Create a rect from both mouse positions
                var rect = Utilities.GetScreenRect(originDragPosition, Input.mousePosition);
                Utilities.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utilities.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
                

                if (highlightedTiles != null) {
                    for (int i = 0; i < highlightedTiles.Count; i++) {
                        HexTile currTile = highlightedTiles[i];
                        currTile.UnHighlightTile();
                    }
                    highlightedTiles.Clear();
                }
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
                        highlightedTiles.Add(currTile);
                        currTile.HighlightTile(Color.gray);
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
    }
}
