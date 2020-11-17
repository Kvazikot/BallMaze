using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingArea : MonoBehaviour
{    
	public Transform Goal;
	public Transform CubeAgent;
	float  max_coord;
	
	// Start is called before the first frame update
    void Start()
    {
        max_coord = 2.5F;
    }
	
	public void Reset()
	{
		float y = Goal.localPosition.y;
		Goal.localPosition = new Vector3( Random.Range(-max_coord, max_coord), y, Random.Range(-max_coord, max_coord));
		y = CubeAgent.localPosition.y;
		CubeAgent.localPosition = new Vector3( Random.Range(-max_coord, max_coord), y, Random.Range(-max_coord, max_coord));
		var renderer = Goal.GetComponent<MeshRenderer>();
		renderer.enabled = true;
		var colider = Goal.GetComponent<SphereCollider>();
		colider.enabled = true;
	}
	
	void Update()
	{
		//AreaReset();
	}
}
