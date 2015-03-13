using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T4Agent2 {
	public Vector3 pos;
	public Vector3 goalPos;
	public int wp;
	public bool wpReached;
	public GameObject agent;
	public string id;
	public Vector3 velocity;
	//public List<float> lastAngles;
	public float agentSize;
	public float timeStepLimit;
	//public float angleEqLimit;

	private float weightPenalty;
	
	public T4Agent2(string id, Vector3 start, Vector3 goalPos) {
		this.goalPos = goalPos;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start;
		pos = start;
		wp = 0;
		wpReached = false;
		velocity = new Vector3 (0,0,0);
		agentSize = 0.5f;
		timeStepLimit =1000f;
		//angleEqLimit = 2f;

		weightPenalty = 10f;
	}




	/*
	 * Function to find a new velocity with minimum penalty. This is done by calculating the preferred
	 * velocity and then sample N velocities among the velocities possible to reach within one timestep
	 * 
	 */
	public Vector3 findMinimumPenaltyVel(List<T4Agent2> agents, float acceleration, Vector3 goalPos, float goalInterval){

		//Test to check collisions
		for (int i=0; i<agents.Count; i++) {
			if(!string.Equals(this.id,agents[i].id)){
				if(Vector3.Distance(this.agent.transform.position,agents[i].agent.transform.position)<2*agentSize){
					Debug.Log ("Two agents collided");
				}
			}
				}


		Vector3 curPos = this.agent.transform.position;

		//Calculating the preferred velocity. This will be the velocity that points straight towards
		//the goal with a magnitude that is as big as possible but not to big so that the point will
		//overshoot the target.
		//If the agent is of distance goalInterval from the goal the preferred velocity is the null vector
		Vector3 prefVel = new Vector3 (0, 0, 0);
		if (Vector3.Distance (curPos, goalPos) > goalInterval) {
			//Then calculate the preferred velocity
			float distanceToTarget=Vector3.Distance (goalPos, curPos);
			float prefSpeed=Mathf.Sqrt(distanceToTarget*2*acceleration*Time.deltaTime);
			//Debug.Log("Pref speed:"+prefSpeed);
			Vector3 prefDir=Vector3.Normalize(goalPos-curPos);
			prefVel=prefDir*prefSpeed;
			}

		//Now we need to sample a bunch of possible velocities and check their penalty
		Vector3 minPenVel = Vector3.zero;
		float minPen = float.PositiveInfinity;

		//First check the penalty of staying at the same velocity (no acceleration)
		float timeToCollision = this.calculateTimeToCollision (this.velocity, agents);
		if (!float.IsPositiveInfinity (timeToCollision)) {
			//Debug.Log ("TimeToCol:" + timeToCollision);
				}

		float stayPen = this.calculatePenalty (this.velocity, timeToCollision, prefVel);
		if (stayPen < minPen) {
			//Debug.Log("StayPen:"+stayPen);
			minPen=stayPen;
			minPenVel=this.velocity;
				}

		//Sample N possible velocities
		int N = 300;
		float changeAng = 0;

		bool foundOneWithoutCol = false;
		for (int i=0; i<N; i++) {
			Vector3 accelerationDir=new Vector3(0,0,0);
			accelerationDir.x=Mathf.Cos(changeAng);
			accelerationDir.z=Mathf.Sin(changeAng);
			Vector3 newVel=this.calculateNewVelocity(acceleration,accelerationDir);
			timeToCollision=this.calculateTimeToCollision(newVel,agents);
			if(float.IsPositiveInfinity(timeToCollision)){
				foundOneWithoutCol=true;
			}
			float penalty=this.calculatePenalty(newVel,timeToCollision,prefVel);
			if(penalty<minPen){
				minPen=penalty;
				minPenVel=newVel;
			}

			changeAng=changeAng+(2*Mathf.PI/N);
			}

		if (!foundOneWithoutCol) {
			//Debug.Log("did not find Vel without col");
			}

		//Debug.Log ("MinPen:" + minPen);
		//Debug.Log ("DotBetween prefVel and chosen vel" + Vector3.Dot (Vector3.Normalize(minPenVel), Vector3.Normalize(prefVel)));
		return minPenVel;

		}

	public Vector3 calculateNewVelocity(float acceleration, Vector3 accelerateTowards){

		Vector3 dir;
		
		dir = accelerateTowards;

		Vector3 dynPVel = this.velocity;

		//Debug.Log("dir="+dir);
		//Debug.Log("DynPVel="+dynPVel);
		
		//Vector3 normVel = Vector3.Normalize (dynPVel);

		Vector3 change = dir;
		
		float accInX = change.x / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
		float accInZ = change.z / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
		
		if (float.IsNaN (accInX)) {
			accInX = 0;
		}
		if (float.IsNaN (accInZ)) {
			accInZ = 0;
		}

			dynPVel.x = dynPVel.x + acceleration * accInX * Time.deltaTime;
			dynPVel.z = dynPVel.z + acceleration * accInZ * Time.deltaTime;
		

		return dynPVel;

		}

	/**
	 * Function to check if the proposed velocity is OK, i.e. outside the vector obstacle
	 * given the other agent.
	 * 
	 */
	/*private bool checkOkVelocity(Vector3 newVel, T4Agent2 otherAgent){

		Vector3 curPos = this.agent.transform.position;
		Vector3 otherPos = otherAgent.agent.transform.position;
		float radius = 2 * agentSize;
		Vector3 toCircleCenter = otherPos - curPos;
		Vector3 relativeVel = Vector3.Normalize(newVel - otherAgent.velocity);
		//Debug.Log ("Relative Vel:" + relativeVel);
		Vector3 velocityRay = curPos + timeStepLimit * relativeVel;

		//Debug.Log ("VelocityRay:" + velocityRay);

		Vector2 toCircle = new Vector2 (toCircleCenter.x, toCircleCenter.z);
		Vector2 velRay = new Vector2 (velocityRay.x, velocityRay.z);

		float angle = Vector2.Angle (velRay, toCircle);
		angle = angle * (Mathf.PI / 180.0f);
		float distance = Mathf.Abs(Mathf.Cos (angle) * toCircle.magnitude);

		//Vector3 projection = Vector3.Project (toCircleCenter, Vector3.Normalize(velocityRay));
		//Debug.Log ("Projection:" + projection);

		//float distance = Vector3.Distance (projection, otherPos);
		//Debug.Log ("Distance:" + distance);
		//If the distance is less than the radius the velocity is not ok
		if (distance < radius) {
			return false;
				}

		return true;
		}*/

	private float findIntersectionPoint(Vector3 newVelocity, T4Agent2 otherAgent){


		Vector3 velToCheck3 = 2 * newVelocity - this.velocity;
		Vector2 velToCheck = new Vector2 ();
		velToCheck.x = velToCheck3.x;
		velToCheck.y = velToCheck3.z;

		//Debug.Log ("VelTOCheck3:" + velToCheck3);
		//Debug.Log("VelToCheck:"+velToCheck);

		//Vector3 velToCheck = newVelocity ;
		/*
		Vector3 curPos = this.agent.transform.position;
		Vector3 otherPos = otherAgent.agent.transform.position;
		float r = 2 * agentSize;
		Vector3 toCircleCenter = otherPos-curPos;
		Vector3 relativeVel = Vector3.Normalize(velToCheck - otherAgent.velocity);
		//Debug.Log ("Relative Vel:" + relativeVel);
		Vector3 velocityRay =  (timeStepLimit * relativeVel) - curPos;*/

		Vector2 curPos = new Vector2 ();
		curPos.x = this.agent.transform.position.x;
		curPos.y = this.agent.transform.position.z;
		Vector2 otherPos = new Vector2 ();
		otherPos.x = otherAgent.agent.transform.position.x;
		otherPos.y = otherAgent.agent.transform.position.z;
		Vector2 toCircleCenter = otherPos - curPos;
		Vector2 otherAgentVel = new Vector2 ();
		otherAgentVel.x = otherAgent.velocity.x;
		otherAgentVel.y = otherAgent.velocity.z;
		Vector2 relativeVel = velToCheck - otherAgentVel;
		Vector2 velocityRay = (timeStepLimit * relativeVel) - curPos;
		float r = 2 * agentSize;


		//Debug.Log ("To circle center:" + toCircleCenter);
		//Debug.Log ("relativeVel:" + relativeVel);
		//Debug.Log ("velocityRay:" + velocityRay);

		
		float angle = Vector2.Angle (velocityRay, toCircleCenter);
		if (angle < 0.1f)
						angle = 0;

		angle = angle * (Mathf.PI / 180.0f);
		//Debug.Log ("Angle:" + angle);

		float distance = Mathf.Abs(Mathf.Sin (angle) * toCircleCenter.magnitude);
		//Debug.Log ("Distance:" + distance);


		//If the distance is less than the radius the velocity is not ok
		if (distance <= r) {

			float distAlongRay=Mathf.Abs(Mathf.Cos(angle)*toCircleCenter.magnitude);
			float distInside=Mathf.Pow(r,2)-Mathf.Pow(distance,2);
			float distToIntersect=distAlongRay-distInside;

			float timeToIntersect=distToIntersect/relativeVel.magnitude;

			//Debug.Log ("Line cut circle");
			return timeToIntersect;
			} 
		else {
			return float.PositiveInfinity;
			}


		/*
		float a = Vector2.Dot(velocityRay, velocityRay ) ;
		float b = 2*Vector2.Dot(toCircleCenter, velocityRay ) ;
		float c = Vector2.Dot(toCircleCenter,toCircleCenter ) - r*r ;

		Debug.Log ("a:" + a);
		Debug.Log ("b:" + b);
		Debug.Log ("c:" + c);

		float discriminant = b*b-4*a*c;
		Debug.Log ("Discriminant:" + discriminant);
		if( discriminant < 0 )
		{
			//Debug.Log("RayTotalyMissed");
			return float.PositiveInfinity;
		}
		else
		{
			// ray didn't totally miss sphere,
			// so there is a solution to
			// the equation.
			
			discriminant = Mathf.Sqrt( discriminant );
			
			// either solution may be on or off the ray so need to test both
			// t1 is always the smaller value, because BOTH discriminant and
			// a are nonnegative.
			float t1 = (-b - discriminant)/(2*a);
			float t2 = (-b + discriminant)/(2*a);

			Debug.Log("t1:"+t1);
			Debug.Log ("t2:"+t2);


			if( t1 >= 0 && t1 <= 1 )
			{
				// t1 is the intersection, and it's closer than t2
				// (since t1 uses -b - discriminant)
				// Impale, Poke

				//Since the ray is relativeVel*timeStepLimit long
				float time=t1*timeStepLimit;

				//Debug.Log("Time to collision found:"+t1);

				return time ;
			}

			if( t2 >= 0 && t2 <= 1 )
			{
				// t1 is the intersection, and it's closer than t2
				// (since t1 uses -b - discriminant)
				// Impale, Poke
				
				//Since the ray is relativeVel*timeStepLimit long
				float time=t2*timeStepLimit;
				
				//Debug.Log("Time to collision found:"+t1);
				
				return time ;
			}

			// no intn: FallShort, Past, CompletelyInside
			return float.PositiveInfinity ;
		}*/

		}
	/*
	private bool isInReciprocalVO(Vector3 newVelocity, List<T4Agent2> agents){

		Vector3 velToCheck = 2 * newVelocity - this.velocity;

		for (int i=0; i<agents.Count; i++) {
			T4Agent2 curAgent=agents[i];
			if(!string.Equals(this.id,curAgent.id)){
				if(!this.checkOkVelocity(velToCheck,curAgent)){
					//if the velocity is not OK
					return true;
				}
			}

				}

		return false;

		}*/
	

	public float calculateTimeToCollision(Vector3 newVelocity, List<T4Agent2> agents){

		float minTimeToCollision = float.PositiveInfinity;

		for (int i=0; i<agents.Count; i++) {
			T4Agent2 curAgent=agents[i];
			if(!string.Equals(this.id,curAgent.id)){
				float timeToCollision = this.findIntersectionPoint (newVelocity, curAgent);
				if(timeToCollision<minTimeToCollision){
					minTimeToCollision=timeToCollision;
				}
			}
		}

		return minTimeToCollision;
		}


	private float calculatePenalty(Vector3 newVelocity, float timeToCollision,Vector3 prefVel){

		float insideRVOpenalty = 0;

		if (!float.IsPositiveInfinity (timeToCollision)) {
			insideRVOpenalty=weightPenalty*(1.0f/timeToCollision);
			//Debug.Log("CollisionRisk. Adding to penalty:"+insideRVOpenalty);
				}

		float velDist=Vector3.Distance (newVelocity, prefVel);

		//Debug.Log ("Vel dist penalty:" + velDist);

		float totPenalty = insideRVOpenalty + velDist;

		return totPenalty;

	}



	
}