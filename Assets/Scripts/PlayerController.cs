using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Rnd = UnityEngine.Random;

enum ObjectType { WAYPOINT, WALL };

struct ScanInfo
{
    public ObjectType type;
    public int n_scans;
    public Color color;
}

public class PlayerController : MonoBehaviour
{
    RollerAgent agent;
    public Rigidbody rb;
    public float speed;
    Waypoints waypoints;
    public float distance;
    public Transform targetWaypoint = null;
    public Transform dieingWaypoint = null;
    public bool targetDetected = false;
    public bool targetLost = false;
    Color targetColor = Color.green;
    public List<RaycastHit> hitPoints;
    public List<int> tagLabels;
    public const int n_scans = 6;
    public float FOV = 10;
    int targets_collected = 0;
    public int TOTAL_TARGETS = 4;
    public float[] distancesToWallsVector;
    public float[] distancesVector;//{ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
                                   //                                 ----------- forward scans(n_scans)  +   forward, backward, left, right


    Matrix4x4 rotateM, rotateM2;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<RollerAgent>(); 
        waypoints = GetComponent<Waypoints>(); 
        distancesVector = new float[n_scans+4];
        distancesToWallsVector = new float[5];
        for (int i = 0; i < distancesVector.Length; i++)
            distancesVector[0] = float.MaxValue;
        tagLabels = new List<int>();
        hitPoints = new List<RaycastHit>();
        //prepare rotation matrices
        Quaternion q = Quaternion.AngleAxis(-FOV , Vector3.up); 
        Quaternion q2 = Quaternion.AngleAxis(FOV , Vector3.up); 		
        rotateM = Matrix4x4.Rotate(q);
        rotateM2 = Matrix4x4.Rotate(q2);
		targetWaypoint = GameObject.Find("S").transform;
		dieingWaypoint = GameObject.Find("S").transform;
        

    }

    // directions towards path or perpendicular to path
    void Find4Directions()
    {
        const float maxRayDistance = 50f;
        int n = 0;
        List<float> angles = new List<float> { Angle.RIGHT, Angle.LEFT, Angle.BACKWARD, Angle.FORWARD };
        Vector3 dir = new Vector3();
        foreach (float a in angles)
        {
            Vector3 p = transform.position;
            dir.x = Mathf.Sin(Mathf.Deg2Rad * a);
            dir.z = Mathf.Cos(Mathf.Deg2Rad * a);
            dir.y = 0;
            dir.Normalize();
            RaycastHit hitP;
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out hitP, maxRayDistance, 1 << 8))
            {
                // if its a wall
                if (hitP.collider.GetType() != typeof(SphereCollider))
                    distancesToWallsVector[n] = hitP.distance;
                else
                    distancesToWallsVector[n] = -1;
                Debug.DrawLine(transform.position, hitP.point, Color.green);
                distancesVector[n_scans + n] = hitP.distance;
                //Debug.Log($"n={n}");
                n++;
            }

                
        }

    }


    bool set_scan_data(RaycastHit hitP, int i, ref Dictionary<Transform, ScanInfo> dict)
    {


        ScanInfo scan_info = new ScanInfo();
        var renderer = hitP.collider.GetComponent<MeshRenderer>();
        Color color = renderer.material.GetColor("_Color");

        if (!dict.ContainsKey(hitP.collider.transform))
        {
            scan_info.n_scans = 1;
			//Debug.Log("tag = " + hitP.collider.tag);
            if (hitP.collider.tag == "waypoint")
            {
                scan_info.type = ObjectType.WAYPOINT;
                scan_info.color = color;
				hitPoints.Add(hitP);
                tagLabels.Add(1);
            }
            else
            {
                scan_info.type = ObjectType.WALL;
                hitPoints.Add(hitP);
                tagLabels.Add(0);
            }
            dict[hitP.collider.transform] = scan_info;
        }
        else
        {
            scan_info = dict[hitP.collider.transform];
            scan_info.n_scans++;
            dict[hitP.collider.transform] = scan_info;
            
        }
        distancesVector[i] = hitP.distance;
        return true;
    }

    // get scans in velocity direction for RollerAgent
    // deltaAngle in degrees
    void ScanInVelocityDirection(float deltaAngle, int n_scans)
    {
        const float maxRayDistance = 30f;
        Dictionary<Transform, ScanInfo> dict = new Dictionary<Transform, ScanInfo>();

        hitPoints.Clear();
        tagLabels.Clear();

        Vector3 axis = Vector3.back;
        Matrix4x4 M1 = Matrix4x4.Rotate(Quaternion.AngleAxis(-60 + Rnd.Range(-5,5), Vector3.up));
        Matrix4x4 M2 = Matrix4x4.Rotate(rb.rotation);
        Matrix4x4 M3 = M1 ;
        axis = M3.MultiplyVector(axis);
        Debug.DrawRay(transform.position, axis, Color.blue);
      

        // scan left
        Vector3 dir = axis;
     
        for (int i = 0; i < n_scans; i++)
        {
            RaycastHit hitP;
            distancesVector[i] = 0;
            Ray ray = new Ray(transform.position, dir);
          
            RaycastHit hit;

            float distanceToObstacle = 0;
            // Cast a sphere wrapping character controller 10 meters forward
            // to see if it is about to hit anything.
            if (Physics.SphereCast(ray,2,out hit, 100,(1 << 8)))
            {
                distanceToObstacle = hit.distance;
                set_scan_data(hit, i, ref dict);
                Debug.DrawLine(transform.position, hit.point, Color.green);
            }
           

            dir = rotateM.MultiplyVector(dir);
        }
     
        // get target detection
        float minValue = int.MaxValue;
        Transform Target = targetWaypoint;
        int num_target_scans=0;
        int count = 0;
        foreach (var entry in dict)
        {
            
            if(count == n_scans/2)
			//Debug.Log( entry.Key + " type=" + entry.Value.type+"n_scans="+ entry.Value.n_scans);
            if (entry.Value.type == ObjectType.WAYPOINT)
            {
				
                float distance = Vector3.Distance(entry.Key.position, this.transform.position);
                if (minValue > Mathf.Min(minValue, distance))
                {
                    minValue = Mathf.Min(minValue, distance);
                    Target = entry.Key;
                }

                if (entry.Key == targetWaypoint)
                    num_target_scans++;

           
            }
          
            count++;
        }
       
        //scan_reward = scan_reward * (num_target_scans);
        //Debug.Log("dict.size" + dict.Count);

        //if(Target!=null && Target!=dieingWaypoint)
        if (Target!= targetWaypoint)
        {
            dieingWaypoint = targetWaypoint;
            targetWaypoint = Target;
            targetDetected = true;
            targetLost = false;
            //var renderer = targetWaypoint.GetComponent<MeshRenderer>();
            //renderer.material.SetColor("_Color", targetColor);
        }

        if (num_target_scans == 0)
        {
            targetLost = true;
            //targetDetected = false;
        }
        else
            targetLost = false;
        // if (scan_reward > 1)
        //     scan_reward = 1f;

    }




    //void OnCollisionEnter(Collision collision)
    void OnTriggerEnter(Collider collider)
    {	       
            var renderer = collider.GetComponent<MeshRenderer>();
            Color color = renderer.material.GetColor("_Color");
            //Debug.Log("color " + color);
            if (collider.GetType() == typeof(SphereCollider))
            {
                renderer.enabled = false;
                collider.enabled = false;
                // collide with red spheres
                if (color == Color.red)
				{
                    agent.SetReward( 1f );
                    targets_collected++;
                    //Debug.Log("reward from red sphere");
                }
                // collide with green spheres i.e. targets
                else if (color == Color.green)
                    agent.SetReward(0.5f);

                if (targets_collected == TOTAL_TARGETS)
                {
                    agent.EndEpisode();
                    targets_collected = 0;
                }
				
                if (targetWaypoint != null)
                {
                    //renderer.material.SetColor("_Color", Color.magenta);
                    //Destroy(targetWaypoint.gameObject, 1);
                    dieingWaypoint = targetWaypoint;
                    targetWaypoint = null;                   
                    targetDetected = false;
                    targetLost = false;
                }
                

            }
            else
                // collide with walls
                agent.SetReward(-0.001f);

       

    }


    // Update is called once per frame
    int n_fr=0;
    void FixedUpdate()
    {
        //if(n_fr > 10 && (n_fr % 10)==0)
        //  ScanInVelocityDirection(FOV, n_scans);
        //Find4Directions();
       
        n_fr++;
    }

    
  
}

