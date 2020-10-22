using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RrtPlaner : MonoBehaviour
{
    public Vector3 StartP;
    public Vector3 EndP;
    public List<Component> wps;

    // Start is called before the first frame update
    void Start()
    {
        Component[] obs_list;

        GameObject o = GameObject.Find("Obstacles");
        obs_list = o.GetComponentsInChildren(typeof(Transform));
        //foreach(GameObject p in wps.
        //GameObject[] points = wps.GetComponents<GameObject>();
        //Debug.Log("number of obstacles is " + obs_list.Length);
        foreach (Transform p in obs_list)
        {
       
         
           // Debug.Log("obs " + p.name);
        }
       
     
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (wps.Count == 2)
        { 
            StartP = wps[0].transform.position;
            EndP = wps[1].transform.position;
            Debug.DrawLine(StartP, EndP);
        }
    }
}
