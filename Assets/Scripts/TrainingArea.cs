using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingArea : MonoBehaviour
{    
	//public Transform Goal;

	public Transform CubeAgent;
    public float max_coord;

    // Start is called before the first frame update
    public virtual void Start()
    {
        max_coord = 2.5F;
    }
	
	public virtual void Reset(Transform agent)
	{
		float y = CubeAgent.localPosition.y;
		CubeAgent.localPosition = new Vector3( Random.Range(-max_coord, max_coord), y, Random.Range(-max_coord, max_coord));
		
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform Goal = transform.GetChild(i);
            if (Goal.tag == "waypoint")
            {
                var renderer = Goal.GetComponent<MeshRenderer>();
                renderer.enabled = true;
                var colider = Goal.GetComponent<SphereCollider>();
                colider.enabled = true;
                y = Goal.localPosition.y;
                Goal.localPosition = new Vector3(Random.Range(-max_coord, max_coord), y, Random.Range(-max_coord, max_coord));
            }
        }
		
	}
	
	void Update()
	{
		//AreaReset();
	}
}
