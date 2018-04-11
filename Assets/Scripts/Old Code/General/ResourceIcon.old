using UnityEngine;
using System.Collections;
using System.Linq;

public class ResourceIcon : MonoBehaviour {

	public HexTile hexTile;
    private RESOURCE _resource;
    private bool isHovering = false;

	public Sprite[] icons;

    #region getters/setters
    public RESOURCE resource {
        get { return _resource; }
    }
    #endregion

    public void SetResource(RESOURCE resource) {
        gameObject.SetActive(true);
        _resource = resource;
		for (int i = 0; i < this.icons.Length; i++) {
			if(this.icons[i].name == resource.ToString()){
                SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
                UI2DSprite ui2ds = gameObject.GetComponent<UI2DSprite>();
                if (sr != null) {
                    sr.sprite = this.icons [i];
				} else if(ui2ds != null) {
                    //ui2ds.MarkAsChanged();
                    ui2ds.sprite2D = this.icons [i];
				}
			}
		}
    }

    public void OnMouseOver() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        isHovering = true;
		UIManager.Instance.ShowSmallInfo("[b]" + resource.ToString() + ": " + hexTile.resourceCount.ToString() + "[/b]");
    }

    public void OnMouseExit() {
        isHovering = false;
        UIManager.Instance.HideSmallInfo();
    }

    private void Update() {
        if (this.isHovering) {
            if (resource != RESOURCE.NONE) {
				UIManager.Instance.ShowSmallInfo("[b]" + resource.ToString() + ": " + hexTile.resourceCount.ToString() + "[/b]");
            }
        }
    }

}
