using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelLineParent : MonoBehaviour {
    public Image bgImage;
    public Color[] _childColors;

    private HexTile _startTile;
    private HexTile _endTile;
    private int _usedColorIndex;
    private int _numOfTicks;

    private List<TravelLine> _children;

    #region getters/setters
    public HexTile startPos {
        get { return _startTile; }
    }
    public HexTile endPos {
        get { return _endTile; }
    }
    public int numOfTicks {
        get { return _numOfTicks; }
    }
    #endregion

    public void SetStartAndEndPositions(HexTile startTile, HexTile endTile, int numOfTicks) {
        _children = new List<TravelLine>();
        _startTile = startTile;
        _endTile = endTile;
        _numOfTicks = numOfTicks;

        //Creates travel line
        float angle = Mathf.Atan2(_endTile.transform.position.y - _startTile.transform.position.y, _endTile.transform.position.x - _startTile.transform.position.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, angle);
        float distance = Vector3.Distance(_startTile.transform.position, _endTile.transform.position);
        GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 0.2f);
        gameObject.transform.SetParent(_startTile.UIParent);
        transform.localPosition = Vector3.zero;

        _startTile.SetTravelLineParent(this);
    }
    public void AddChild(TravelLine line) {
        _children.Add(line);
        if (_usedColorIndex >= _childColors.Length) {
            _usedColorIndex = 0;
        }
        line.SetColor(_childColors[_usedColorIndex]);
        _usedColorIndex += 1;

        line.SetLineParent(this);
        //line.transform.SetParent(transform);
        //line.rectTransform.ForceUpdateRectTransforms();
        line.Initialize();
        //line.transform.localPosition = Vector3.zero;
        //line.rectTransform.sizeDelta = Vector3.zero;
    }
    public void RemoveChild(TravelLine line) {
        _children.Remove(line);
        line.SetLineParent(null);
        SetActiveBG(false);
        if(_children.Count <= 0) {
            GameObject.Destroy(this.gameObject);
            _startTile.SetTravelLineParent(null);
        }
    }
    public void SetActiveBG(bool state) {
        if (state) {
            bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 1);
        } else {
            for (int i = 0; i < _children.Count; i++) {
                if (_children[i].holder.activeSelf) {
                    bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 1);
                    return;
                }
            }
            bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 0);
        }
    }
}
