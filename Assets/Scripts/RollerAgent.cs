//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;

public class RollerAgent : Agent
{
    Rigidbody rb;
    PlayerController pc;
    //PathWalker path_walker;
    Waypoints waypoints;
    public Transform Target;
    public float forceMultiplier = 15;
    public float Sum = 10;
    System.DateTime ts = System.DateTime.UtcNow;
    const float MaxMass = 3f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<PlayerController>();
      //  path_walker = GetComponent<PathWalker>();
        GameObject wps = GameObject.Find("Waypoints");
        waypoints = wps.GetComponent<Waypoints>();
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
        actionsOut[2] = 0;
        Debug.Log("Heuristic function call");
    }

    public override void OnEpisodeBegin()
    {
        ts = System.DateTime.UtcNow;
        // If the Agent fell, zero its momentum
        this.rb.angularVelocity = Vector3.zero;
        this.rb.velocity = Vector3.zero;
        // Move the target to a new spot

        //waypoints.HideAllWaypoints();
        //this.transform.position =  new Vector3(-5.4f,
        //                                0.0f,
        //                               4.98f);

        //waypoints.UnhideAllWaypoints();
		Debug.Log("Episode begin");

    }
   
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        if (pc.targetWaypoint != null)
        {
            Target = pc.targetWaypoint.transform;
            sensor.AddObservation(pc.targetWaypoint.transform.position);
        }
        //sensor.AddObservation(pc.targetWaypoint2);
        //sensor.AddObservation(this.transform.position);
        //sensor.AddObservation(pc.redShpereDetected);

        //sensor.AddObservation(Target.position - this.transform.position);

        //Hit points on maze walls
       //for(int i=0; i < pc.hitPoints.Count; i++)
//sensor.AddObservation(pc.hitPoints[i]);
//
       // for (int i = 0; i < PlayerController.n_scans; i++)
       //    sensor.AddObservation(pc.hit_bits[i]);

        //float maxDistance = float.MinValue;
        foreach (float d in pc.distancesToWallsVector)
            if(d!=-1)
               sensor.AddObservation(d);
        //    maxDistance = Mathf.Max(d, maxDistance);
        //sensor.AddObservation(maxDistance);

        // Agent velocity
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

        sensor.AddObservation(pc.targetDetected);
        //sensor.AddObservation(!pc.targetLost);

        sensor.AddObservation(pc.distance);
    }

    bool stickToWalls()
    {
        const float minSumThreshold = 400f;
        float min_dist = float.MaxValue;
        for (int i = 0; i < pc.distancesToWallsVector.Length; i++)
        {
            if (pc.distancesToWallsVector[i] != -1)
                min_dist = Mathf.Min(min_dist, pc.distancesToWallsVector[i]);
        }
        Sum += min_dist;
        var t = System.DateTime.UtcNow - ts;
        if (t.Milliseconds >= 500)
        {
            Debug.Log($"stickToWalls  Sum={Sum}");
            ts = System.DateTime.UtcNow;
            Sum = 0;
        }

        if (Sum < minSumThreshold)
            return true;
        return false;
    }

    bool wallIsNear()
    {
        const float wallThreshold = 2;
        float min_dist = float.MaxValue;
        for (int i = 0; i < pc.distancesToWallsVector.Length; i++)
        {
            if (pc.distancesToWallsVector[i] != -1)
                min_dist = Mathf.Min(min_dist, pc.distancesToWallsVector[i]);
        }
        return min_dist < wallThreshold;
    }

    void FixedUpdate()
    {
        
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        
        //------------------- Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        forceMultiplier = 10 * Mathf.Abs(vectorAction[3]);
        rb.AddForce(controlSignal * forceMultiplier);

        rb.mass = 1 + MaxMass * Mathf.Abs( vectorAction[2] );

        //--------- inference mode only
        //if (pc.hadGreenSphereColisions | pc.hadRedSphereColisions |  distanceToTarget < 2f)
        //    pc.targetDetected = false;
		//return;
     
        //-------------------- Rewards


        // Reward for scanning red sphere
       // SetReward(pc.scan_reward);


        if (pc.targetLost)
        {
            SetReward(-0.001f);
            
        }
       // if (t.Seconds > 40)
       //     EndEpisode();
       //Debug.Log("t.TotalSeconds = " + t.TotalSeconds);
       // if (t.TotalSeconds > 2)

        //Debug.Log($"t={Mathf.Round(pc.t)}");
        //if min distance to walls < 2 stick

        if (wallIsNear())
        {
            SetReward(-0.001f);
        }
            
        if ( stickToWalls() )
        {
            SetReward(-0.005f);
            //Debug.Log("Stick to walls!");
            //EndEpisode();
        }
                  
        //fall out
        if (transform.position.y < 0)
        {
            SetReward(-1f);
            EndEpisode();
        }

        //dont spend time
        SetReward(-0.0001f);
        //SetReward(0.01f);

    }
}
