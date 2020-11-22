// Kvazikot 
// based on mlagents samples code

//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System.Linq;

public class RollerAgent : Agent
{
    Rigidbody rb;
    PlayerController pc;
    //PathWalker path_walker;
	public TrainingArea area;
    Waypoints waypoints;
    public Transform Target;
    public float Sum = 10;
    System.DateTime ts = System.DateTime.UtcNow;
	float m_LateralSpeed;
    float m_ForwardSpeed;
	float agentRunSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<PlayerController>();
      //  path_walker = GetComponent<PathWalker>();
        GameObject wps = GameObject.Find("Waypoints");
        waypoints = wps.GetComponent<Waypoints>();
		
		// two new variables for discreet space motion
		m_LateralSpeed = 0.3f;
        m_ForwardSpeed = 1.3f;
		agentRunSpeed = 1f;
    }
	
	public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        int forwardAxis = (int)act[0];
        int rightAxis = (int)act[1];
        int rotateAxis = (int)act[2];
	
		//Debug.Log($"forwardAxis={forwardAxis} rightAxis={rightAxis} rotateAxis={rotateAxis}");
        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        rb.AddForce(dirToGo * agentRunSpeed,
            ForceMode.VelocityChange);
    }


 public override void Heuristic(float[] actionsOut)
    {
		actionsOut[0] = 0;
		actionsOut[1] = 0;
		actionsOut[2] = 0;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            actionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            actionsOut[1] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        ts = System.DateTime.UtcNow;
        // If the Agent fell, zero its momentum
        this.rb.angularVelocity = Vector3.zero;
        this.rb.velocity = Vector3.zero;
		area.Reset(transform);
		pc.targetWaypoint = null;
        // Move the target to a new spot

        //waypoints.HideAllWaypoints();
        //this.transform.position =  new Vector3(-5.4f,
        //                                0.0f,
        //                               4.98f);

        //waypoints.UnhideAllWaypoints();
		//Debug.Log("Episode begin");

    }
   
    public override void CollectObservations(VectorSensor sensor)
    {
	
        // Target and Agent positions
		  return;
        if (pc.targetWaypoint != null)
        {
            Target = pc.targetWaypoint.transform;
            sensor.AddObservation(Target.position);
			Vector3 vect = new Vector3();
			vect = Target.position - this.transform.position;
			//sensor.AddObservation(vect.magnitude);
        }
        //return;
		//sensor.AddObservation(pc.targetWaypoint2);
        //sensor.AddObservation(this.transform.position);
        //sensor.AddObservation(pc.redShpereDetected);

      
        

       //Hit points on maze walls
       for(int i=0; i < pc.hitPoints.Count; i++)
	     sensor.AddObservation(pc.hitPoints[i].point);

       //distances
        //for (int i = 0; i < pc.hitPoints.Count; i++)
        //    sensor.AddObservation(pc.hitPoints[i].distance);
        
        //tag labels
        for (int i = 0; i < pc.tagLabels.Count; i++)
         sensor.AddObservation(pc.tagLabels[i]);

        // for (int i = 0; i < PlayerController.n_scans; i++)
        //    sensor.AddObservation(pc.hit_bits[i]);

        //float maxDistance = float.MinValue;
        foreach (float d in pc.distancesToWallsVector)
               sensor.AddObservation(d);
        //    maxDistance = Mathf.Max(d, maxDistance);
        //sensor.AddObservation(maxDistance);

        // Agent velocity
        //sensor.AddObservation(rb.velocity.x);
        //sensor.AddObservation(rb.velocity.z);

        //sensor.AddObservation(pc.targetDetected);
        //sensor.AddObservation(!pc.targetLost);

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
            //Debug.Log($"stickToWalls  Sum={Sum}");
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
		MoveAgent(vectorAction);

        //--------- inference mode only
        //if (pc.hadGreenSphereColisions | pc.hadRedSphereColisions |  distanceToTarget < 2f)
        //    pc.targetDetected = false;
		//return;
     
        //-------------------- Rewards


        // Reward for scanning red sphere
       // SetReward(pc.scan_reward);


        //if (pc.targetLost)
        //    SetReward(-0.001f);            
        
       // if (t.Seconds > 40)
       //     EndEpisode();
       //Debug.Log("t.TotalSeconds = " + t.TotalSeconds);
       // if (t.TotalSeconds > 2)

        //Debug.Log($"t={Mathf.Round(pc.t)}");
        //if min distance to walls < 2 stick

        //if (wallIsNear())
        //{
         //   SetReward(-0.001f);
       // }
            
        if ( stickToWalls() )
        {
            //SetReward(-0.005f);
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
