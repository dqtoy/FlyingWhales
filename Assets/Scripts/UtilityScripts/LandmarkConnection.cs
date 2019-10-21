using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkConnection : MonoBehaviour {
    public GameObject childConnectorPrefab;
    public SpriteRenderer lineParentSpriteRenderer;
    public Sprite[] lineVariations;

	public void DrawLandmarkConnection(Region region1, Region region2, float brokenLineGapInterval) {
        float angle = Mathf.Atan2(region2.coreTile.transform.position.y - region1.coreTile.transform.position.y, region2.coreTile.transform.position.x - region1.coreTile.transform.position.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, angle);
        float distance = Vector3.Distance(region1.coreTile.transform.position, region2.coreTile.transform.position);
        lineParentSpriteRenderer.size = new Vector2(distance, 0.01f);
        //lineGO.transform.localScale = new Vector2(distance / 7.04f, 0.02f);
        //gameObject.transform.SetParent(this.startTile.UIParent);
        transform.localPosition = Vector3.zero;

        int numOfLineVariations = Mathf.CeilToInt(distance / brokenLineGapInterval);
        numOfLineVariations -= 2; //trim 2 line child on end of line
        //brokenLineGapInterval *= 2; //start the line on third position
        for (int i = 2; i < numOfLineVariations; i++) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(childConnectorPrefab.name, Vector3.zero, Quaternion.identity, this.transform);
            SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = lineVariations[UnityEngine.Random.Range(0, lineVariations.Length)];
            spriteRenderer.sortingOrder = lineParentSpriteRenderer.sortingOrder;
            go.transform.localPosition = new Vector3(brokenLineGapInterval * i, go.transform.localPosition.y, go.transform.localPosition.z);
        }
    }
}
