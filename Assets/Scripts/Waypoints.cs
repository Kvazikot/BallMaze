using UnityEngine;

public class Waypoints : MonoBehaviour
{
    Transform highlighted_wp = null;
    float t = 0;
    Color highlightedTargetColor = Color.magenta;
    Color targetColor = Color.green;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void HighlightOn(Transform wp)
    {
        highlighted_wp = wp;
        var renderer = highlighted_wp.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_Color", highlightedTargetColor);
    }

    public void HighlightOff()
    {
        return;
        if (highlighted_wp == null) return;
        var renderer = highlighted_wp.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_Color", targetColor);
        highlighted_wp = null;
    }

    public void UnhideAllWaypoints()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform wp = transform.GetChild(i);
            var collider = wp.GetComponent<SphereCollider>();
            var renderer = collider.GetComponent<MeshRenderer>();
            collider.enabled = true;
            renderer.enabled = true;
            renderer.material.SetColor("_Color", Color.red);
        }
        
    }

    public void HideAllWaypoints()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform wp = transform.GetChild(i);
            var collider = wp.GetComponent<SphereCollider>();
            var renderer = collider.GetComponent<MeshRenderer>();
            collider.enabled = false;
            renderer.enabled = false;
        }
    }

    public void IncreaseBlueValueToHiddenWaypoints()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform wp = transform.GetChild(i);
            var collider = wp.GetComponent<SphereCollider>();
            var renderer = collider.GetComponent<MeshRenderer>();
            if (renderer.enabled == false)
            {
                collider.enabled = true;
                renderer.enabled = true;
            }

            //renderer.material.color.b = renderer.material.color.b + 0.001f;
            //renderer.material.SetColor("_Color", new Color();


        }
    }

    // Update is called once per frame
    void Update()
    {
        return;
        if (highlighted_wp != null)
        {
            if (Mathf.Floor(t) % 2 == 0)
            {
                var renderer = highlighted_wp.GetComponent<MeshRenderer>();
                renderer.material.SetColor("_Color", Color.red);
            }
            else
            {
                var renderer = highlighted_wp.GetComponent<MeshRenderer>();
                renderer.material.SetColor("_Color", highlightedTargetColor);
            }
        }


        t += Time.deltaTime;
    }
}
