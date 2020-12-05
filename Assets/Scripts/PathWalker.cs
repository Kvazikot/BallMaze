using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;


public struct Angle
{
    public const float FORWARD = 0;
    public const float RIGHT = 90;
    public const float BACKWARD = 180;
    public const float LEFT = 270;
}

public class PathWalker : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;
    public Component targetWaypoint = null;
    int targetWaypointIdx = 0;
    GameObject S, E;
    public float t = 0;
    List<Vector3> directions;
    RrtPlaner rrt_planer;
    public List<Component> wps;

    int seconds = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        S = GameObject.Find("S");
        E = GameObject.Find("E");
        targetWaypoint = S.transform;
        directions = new List<Vector3>();

        rrt_planer = GetComponent<RrtPlaner>();

    }

    // draw vectors a (a = closest wypoint and b = closest point on path)
    void drawStateVectors()
    {

    }

   
    void FindTargetWaypoint()
    {
        bool bReached = false;
        if (wps.Count > 2)
        {
            //for new path - find closest point on the path
            if (targetWaypoint == S.transform)
            {
                bReached = false;
                targetWaypoint = rrt_planer.FindClosestWaypoint(transform.position, S.transform, ref bReached);
            }
            bReached = false;
            Component wp = rrt_planer.FindClosestWaypoint(transform.position, targetWaypoint, ref bReached);
            Debug.DrawLine(transform.position, wp.transform.position, Color.white);
            Debug.DrawLine(transform.position, targetWaypoint.transform.position, Color.magenta);

            if (bReached == true)
            {

                //set next target waypoint
                int idx = wps.IndexOf(wp);
                if (idx > 0)
                {
                    var nextWp = wps[idx - 1];
                    targetWaypoint = nextWp.transform;
                }

            }


        }


    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        // apply input
        /*
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        Debug.Log("PlayerController: moveHorizontal " + moveHorizontal + " moveVertical" + moveVertical);
        rb.AddForce(movement * speed);
        */

        //increment time
        t += Time.deltaTime;

        FindTargetWaypoint();

        //perform Tests
        DynamicModelTest1();



    }

    // directions towards path or perpendicular to path
    void Find4Directions()
    {
        List<float> angles = new List<float> { Angle.RIGHT, Angle.LEFT, Angle.BACKWARD, Angle.FORWARD };
        Vector3 dir = new Vector3();
        foreach (float a in angles)
        {
            Vector3 p = transform.position;
            dir.x = Mathf.Sin(Mathf.Deg2Rad * a);
            dir.z = Mathf.Cos(Mathf.Deg2Rad * a);
            dir.y = 0;
            dir.Normalize();
            directions.Add(dir);
        }

    }

    bool isContractionNeeded()
    {
        const float maxRayDistance = 10f;
        const float minRayDistance = 3f;
        int idx = wps.IndexOf(targetWaypoint);
        if (idx > 0)
        {
            var nextWp = wps[idx - 1];
            Vector3 dir = nextWp.transform.position - targetWaypoint.transform.position;
            RaycastHit hitP;
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out hitP, maxRayDistance, 1 << 8))
            {
                bool flag = false;
                if (hitP.distance < minRayDistance)
                    flag = true;
                return flag;
            }
        }
        return false;
    }


    void DynamicModelTest1()
    {
        const float forceToTarget = 10.1f;
        const float forceContraction = 1.0f;
        const float forceSideways = 20.0f;
        const float dirScaler = 0.1f;

        directions.Clear();

        Find4Directions();

        // add direction to target waypoint
        Debug.Log($"targetWaypoint is {targetWaypoint.name}");
        Vector3 dirOnTarget = targetWaypoint.transform.position - transform.position;
        dirOnTarget.Normalize();
        directions.Add(dirOnTarget);

        // apply small force in target direction every frame
        rb.AddForce(dirScaler * dirOnTarget * forceToTarget);

        // apply contraction force to prevent wall bouncing when ball is close to wall
        //if (isContractionNeeded())
        //    rb.AddForce((-1.0f * dirOnTarget) * forceContraction);

        // AddForce to directions in set {Angle.RIGHT, Angle.LEFT, Angle.BACKWARD, Angle.FORWARD}
        if (seconds == Convert.ToInt32(t))
            seconds++;

        // add chaos forses for manuvers
        if (((seconds - 1) % 5) == 0)
        {
            int dir_index = Rnd.Range(0, 2);
            Vector3 dir = dirScaler * directions[dir_index];
            //Debug.Log($"t={t} dir_index={dir_index}");
            rb.AddForce(dir * forceSideways);
        }
        t += Time.deltaTime;


    }
}


