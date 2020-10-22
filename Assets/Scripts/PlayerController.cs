using System;
using System.Collections;
using System.Collections.Generic;
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
    public List<Vector3> hitPoints;
    public float FOV = 15;
    public const int n_scans = 10;
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
        //prepare rotation matrices
        Vector3 rotationVector = new Vector3(0, -(FOV / (n_scans)), 0);
        Vector3 rotationVector2 = new Vector3(0, (FOV / (n_scans)), 0);
        Quaternion q = Quaternion.Euler(rotationVector);
        Quaternion q2 = Quaternion.Euler(rotationVector2);
        rotateM = Matrix4x4.Rotate(q);
        rotateM2 = Matrix4x4.Rotate(q2);
		targetWaypoint = GameObject.Find("S").transform;
		dieingWaypoint = GameObject.Find("S").transform;
    }

    // directions towards path or perpendicular to path
    void Find4Directions()
    {
        const float maxRayDistance = 10f;
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
            if (hitP.collider.GetType() == typeof(SphereCollider))
            {
                scan_info.type = ObjectType.WAYPOINT;
                scan_info.color = color;
                hitPoints.Add(hitP.point);
            }
            else
            {
                scan_info.type = ObjectType.WALL;
                
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

        Debug.DrawRay(transform.position, rb.velocity, Color.blue);

        // scan left
        Vector3 dir = rb.velocity;
     
        for (int i = 0; i < n_scans / 2; i++)
        {
            RaycastHit hitP;
            distancesVector[i] = 0;
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out hitP, maxRayDistance, (1 << 8)))
            {
                Debug.DrawLine(transform.position, hitP.point, Color.green);
                set_scan_data(hitP, i, ref dict);
     
            }
            dir = rotateM.MultiplyVector(dir);
        }

        // scan right
        dir = rb.velocity;
        for (int i = 0; i < n_scans / 2; i++)
        {
            RaycastHit hitP;
            distancesVector[i + n_scans / 2] = 0;
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out hitP, maxRayDistance, (1 << 8)))
            {
                Debug.DrawLine(transform.position, hitP.point, Color.magenta);
                set_scan_data(hitP, i + n_scans / 2, ref dict);
            }
            dir = rotateM2.MultiplyVector(dir);
        }

        // get target detection
        float minValue = int.MaxValue;
        Transform Target = targetWaypoint;
        int num_target_scans=0;
        int count = 0;
        foreach (var entry in dict)
        {
            if (entry.Value.type == ObjectType.WAYPOINT)
            {
                float distance = Vector3.Distance(entry.Key.position, this.transform.position);
                if (minValue > Mathf.Min(minValue, distance))
                {
                    minValue = Mathf.Min(minValue, distance);
                    Target = entry.Key;
                }
                //Debug.Log( entry.Key + " type=" + entry.Value.type+"n_scans="+ entry.Value.n_scans);
                if (entry.Key == targetWaypoint)
                    num_target_scans++;

           
            }
          
            count++;
        }

        //scan_reward = scan_reward * (num_target_scans);
        //Debug.Log("scan_reward " + scan_reward);

        if(Target!=null && Target!=dieingWaypoint)
        if (targetDetected == false && Target!= targetWaypoint)
        {

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
    

   

    void OnCollisionEnter(Collision collision)
    {
		
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            Collider collider = contact.otherCollider;
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
                    agent.SetReward(1f);
					Debug.Log("reward from red sphere");
				}
                // collide with green spheres i.e. targets
                else if (color == Color.green)
                    agent.SetReward(0.5f);
                agent.EndEpisode();
                if (targetWaypoint != null)
                {
                    renderer.material.SetColor("_Color", Color.magenta);
                    Destroy(targetWaypoint.gameObject, 1);
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

    }

    
    // Update is called once per frame
    void FixedUpdate()
    {
       
        ScanInVelocityDirection(FOV, n_scans);
        Find4Directions();

    }

    
  
}

