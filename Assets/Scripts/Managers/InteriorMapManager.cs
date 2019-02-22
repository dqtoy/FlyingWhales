using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteriorMapManager : MonoBehaviour {

    public static InteriorMapManager Instance = null;
    public AreaInnerTileMap currentlyShowingMap { get; private set; }
    public Area currentlyShowingArea { get; private set; }

    public bool isAnAreaMapShowing {
        get {
            return currentlyShowingMap != null;
        }
    }

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        AreaMapCameraMove.Instance.Initialize();
    }

    public void ShowAreaMap(Area area) {
        area.areaMap.Open();
        currentlyShowingMap = area.areaMap;
        currentlyShowingArea = area;
        Messenger.Broadcast(Signals.AREA_MAP_OPENED, area);
    }

    public void HideAreaMap() {
        if (currentlyShowingMap == null) {
            return;
        }
        currentlyShowingMap.Close();
        Messenger.Broadcast(Signals.AREA_MAP_CLOSED, currentlyShowingArea);
        currentlyShowingMap = null;
        currentlyShowingArea = null;
    }

    
}

