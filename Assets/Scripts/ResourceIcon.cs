using UnityEngine;
using System.Collections;
using System.Linq;

public class ResourceIcon : MonoBehaviour {

    private RESOURCE _resource;
    private bool isHovering = false;

	public Sprite[] icons;

    #region getters/setters
    public RESOURCE resource {
        get { return _resource; }
    }
    #endregion

    public void SetResource(RESOURCE resource) {
        _resource = resource;
		for (int i = 0; i < this.icons.Length; i++) {
			if(this.icons[i].name == resource.ToString()){
				if(gameObject.GetComponent<SpriteRenderer>() != null) {
					gameObject.GetComponent<SpriteRenderer> ().sprite = this.icons [i];
				} else if(gameObject.GetComponent<UI2DSprite>() != null) {
					gameObject.GetComponent<UI2DSprite>().sprite2D = this.icons [i];
				}
			}
		}
//        if(gameObject.GetComponent<SpriteRenderer>() != null) {
//            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("Resources Icons")
//                    .Where(x => x.name == resource.ToString()).ToList()[0];
//        } else if(gameObject.GetComponent<UI2DSprite>() != null) {
//            gameObject.GetComponent<UI2DSprite>().sprite2D = Resources.LoadAll<Sprite>("Resources Icons")
//                    .Where(x => x.name == resource.ToString()).ToList()[0];
//        }
        gameObject.SetActive(true);
    }

    public void OnMouseOver() {
        isHovering = true;
        UIManager.Instance.ShowSmallInfo("[b]" + resource.ToString() + "[/b]");
    }

    public void OnMouseExit() {
        isHovering = false;
        UIManager.Instance.HideSmallInfo();
    }

    private void Update() {
        if (this.isHovering) {
            if (resource != RESOURCE.NONE) {
                UIManager.Instance.ShowSmallInfo("[b]" + resource.ToString() + "[/b]");
            }
        }
    }

}
