using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T5Agent
{
		public Vector3 pos;
		public Vector3 goalPos;
		public int wp;
		public bool wpReached;
		public GameObject agent;
		public string id;
		public Vector3 velocity;
		public float agentSize;
		public float timeStepLimit;
		public float neighborTimeLimit;
		private int priority;
		private bool isCar;
		private float weightPenalty;
		public float velSize;
	
		public T5Agent (string id, Vector3 start, Vector3 goalPos, int priority, bool isCar)
		{
				this.goalPos = goalPos;
				this.id = id;
				this.isCar = isCar;
				if (isCar) {
						agentSize = 5f;
						agent = GameObject.CreatePrimitive (PrimitiveType.Cube);
						agent.transform.position = start;
						agent.transform.localScale = new Vector3 ( agentSize, 1, 2*agentSize);
						velSize = 0;
						//In the case of a car the velocity will be the rotation while velSize is the velocity
						velocity = agent.transform.rotation.eulerAngles;
				} 
		//If it's not a car it's a point (sphere)
		else {
						agentSize = 5f;
						agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
						agent.transform.position = start;
						agent.transform.localScale = new Vector3 (2 * agentSize, 1, 2 * agentSize);
						velocity = new Vector3 (0, 0, 0);
				}
				pos = start;
				wp = 0;
				wpReached = false;

				this.priority = priority;

				timeStepLimit = 100f;
		
				weightPenalty = 10f;

				neighborTimeLimit = 5f;

		}

		public bool isAtGoal (float goalInterval)
		{
				float distToGoal = Vector3.Distance (this.agent.transform.position, this.goalPos);
				if (distToGoal < goalInterval)
						return true;
				return false;
		}

	
	
		/*
	 * Function to find a new velocity with minimum penalty. This is done by calculating the preferred
	 * velocity and then sample N velocities among the velocities possible to reach within one timestep
	 * 
	 */
		public Vector3 findMinimumPenaltyVel (List<T5Agent> agents, float acceleration, Vector3 waypoint, 
	                                     float goalInterval, List<Obstacle> obstacles)
		{
		
				//Test to check collisions
				for (int i=0; i<agents.Count; i++) {
						if (!string.Equals (this.id, agents [i].id)) {
								if (Vector3.Distance (this.agent.transform.position, agents [i].agent.transform.position) * 0.95 < 2 * agentSize) {
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
				if (Vector3.Distance (curPos, waypoint) > goalInterval) {
						//Then calculate the preferred velocity
						float distanceToTarget = Vector3.Distance (waypoint, curPos);
						float prefSpeed = Mathf.Sqrt (distanceToTarget * 2 * acceleration);
						//Debug.Log("Pref speed:"+prefSpeed);
						Vector3 prefDir = Vector3.Normalize (waypoint - curPos);
						prefVel = prefDir * prefSpeed;
				}
		
				//Debug.Log ("PrefVel:" + prefVel);

				//Now we need to sample a bunch of possible velocities and check their penalty
				Vector3 minPenVel = Vector3.zero;
				float minPen = float.PositiveInfinity;

				//First check the penalty of staying at the same velocity (no acceleration)
				float timeToCollision = this.calculateTimeToCollision (this.velocity, agents, goalInterval, waypoint);
				float colWithObstTime = this.calculateTimeToColWithObstacle (obstacles, this.velocity, waypoint);
				if (colWithObstTime < timeToCollision) {
						//Debug.Log (this.id + " ColWithWall before");
						timeToCollision = colWithObstTime;
				}
				if (!float.IsPositiveInfinity (timeToCollision)) {
						//Debug.Log ("TimeToCol:" + timeToCollision);
				}
		
				float stayPen = this.calculatePenalty (this.velocity, timeToCollision, prefVel);
				if (stayPen < minPen) {
						//Debug.Log("StayPen:"+stayPen);
						minPen = stayPen;
						minPenVel = this.velocity;
				}
		
				//Sample N possible velocities
				int N = 300;
				float changeAng = 0;
		
				bool foundOneWithoutCol = false;
				for (int i=0; i<N; i++) {
						Vector3 accelerationDir = new Vector3 (0, 0, 0);
						accelerationDir.x = Mathf.Cos (changeAng);
						accelerationDir.z = Mathf.Sin (changeAng);
						Vector3 newVel = this.calculateNewVelocity (acceleration, accelerationDir);
						timeToCollision = this.calculateTimeToCollision (newVel, agents, goalInterval, waypoint);
						colWithObstTime = this.calculateTimeToColWithObstacle (obstacles, newVel, waypoint);
						if (colWithObstTime < timeToCollision) {
								timeToCollision = colWithObstTime;
						}
						if (float.IsPositiveInfinity (timeToCollision)) {
								foundOneWithoutCol = true;
						}
						float penalty = this.calculatePenalty (newVel, timeToCollision, prefVel);
						if (penalty < minPen) {
								minPen = penalty;
								minPenVel = newVel;
						}
			
						changeAng = changeAng + (2 * Mathf.PI / N);
				}
		
				if (!foundOneWithoutCol) {
						//Debug.Log("did not find Vel without col");
				}
	
		
				//Debug.Log ("MinPen:" + minPen);
				//Debug.Log ("DotBetween prefVel and chosen vel" + Vector3.Dot (Vector3.Normalize(minPenVel), Vector3.Normalize(prefVel)));
				return minPenVel;
		
		}


		/**
	 * This function will calculate the minimum penalty velocity and save this velocity
	 * to the variables in the object. These variables will then be used in a calling function
	 * to update the gameObject.
	 */
		public void findMinimumPenaltyVelCar (List<T5Agent> agents, float acceleration, Vector3 waypoint, 
	                                      float goalInterval, List<Obstacle> obstacles, float maxTurning,
	                                      float carLength)
		{
		
				//Test to check collisions
				for (int i=0; i<agents.Count; i++) {
						if (!string.Equals (this.id, agents [i].id)) {
								if (Vector3.Distance (this.agent.transform.position, agents [i].agent.transform.position) * 0.95 < 2 * agentSize) {
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
				if (Vector3.Distance (curPos, waypoint) > goalInterval) {
						//Then calculate the preferred velocity
						float distanceToTarget = Vector3.Distance (waypoint, curPos);
						float prefSpeed = Mathf.Sqrt (distanceToTarget * 2 * acceleration);
						//Debug.Log("Pref speed:"+prefSpeed);
						Vector3 prefDir = Vector3.Normalize (waypoint - curPos);
						prefVel = prefDir * prefSpeed;
				}
		
				//Debug.Log ("PrefVel:" + prefVel);
		
				//Now we need to sample a bunch of possible velocities and check their penalty
				Vector3 minPenVel = Vector3.zero;
				float minPen = float.PositiveInfinity;
				float minPenAcc = 0;
				float minPenRot = 0;

				//First check the penalty of staying at the same velocity (no acceleration)
				Vector3 curVel = this.getCarVelocity ();


				float timeToCollision = this.calculateTimeToCollision (curVel, agents, goalInterval, waypoint);
				float colWithObstTime = this.calculateTimeToColWithObstacle (obstacles, curVel, waypoint);
				if (colWithObstTime < timeToCollision) {
						//Debug.Log (this.id + " ColWithWall before");
						timeToCollision = colWithObstTime;
				}
				if (!float.IsPositiveInfinity (timeToCollision)) {
						//Debug.Log ("TimeToCol:" + timeToCollision);
				}
		
				float stayPen = this.calculatePenalty (curVel, timeToCollision, prefVel);
				if (stayPen < minPen) {
						//Debug.Log("StayPen:"+stayPen);
						minPen = stayPen;
						minPenAcc = 0;
						minPenRot = 0;
						minPenVel = this.velocity;
				}
		
				//Sample N possible velocities
				int N = 300;
				float changeAng = -maxTurning;
				float intervalSize = 2 * Mathf.Abs (maxTurning);


				//Testing with positive acceleration
				for (int i=0; i<N; i++) {

						Vector3 newVel = this.calculateNewVelocityCar (acceleration, changeAng, carLength);
						timeToCollision = this.calculateTimeToCollision (newVel, agents, goalInterval, waypoint);
						colWithObstTime = this.calculateTimeToColWithObstacle (obstacles, newVel, waypoint);
						if (colWithObstTime < timeToCollision) {
								timeToCollision = colWithObstTime;
						}
						float penalty = this.calculatePenalty (newVel, timeToCollision, prefVel);
						if (penalty < minPen) {
								minPen = penalty;
								minPenVel = newVel;
								minPenRot = changeAng;
								minPenAcc = acceleration;
						}
			
						changeAng = changeAng + (intervalSize / N);
				}

				//Testing with negative acceleration
				changeAng = -maxTurning;
				float testAcceleration = -acceleration;

				//Testing with positive acceleration
				for (int i=0; i<N; i++) {

						Vector3 newVel = this.calculateNewVelocityCar (testAcceleration, changeAng, carLength);
						timeToCollision = this.calculateTimeToCollision (newVel, agents, goalInterval, waypoint);
						colWithObstTime = this.calculateTimeToColWithObstacle (obstacles, newVel, waypoint);
						if (colWithObstTime < timeToCollision) {
								timeToCollision = colWithObstTime;
						}
						float penalty = this.calculatePenalty (newVel, timeToCollision, prefVel);
						if (penalty < minPen) {
								minPen = penalty;
								minPenVel = newVel;
								minPenRot = changeAng;
								minPenAcc = testAcceleration;
						}
			
						changeAng = changeAng + (intervalSize / N);
				}


				//Taking the minumum penalty velocity and setting the variables
				Vector3 newRotation = this.velocity;
				float newVelSize = this.velSize + minPenAcc;
				float wheelAngleRad = minPenRot * (Mathf.PI / 180);
				float dTheta=(newVelSize/carLength)*Mathf.Tan(wheelAngleRad);
				newRotation.y = newRotation.y + dTheta;
				
				this.velocity = newRotation;
				this.velSize = newVelSize;

		
		}

		public Vector3 getCarVelocity ()
		{
				
				if (this.isCar) {
						Vector3 curRot = this.velocity;
						float angleRad = curRot.y * (Mathf.PI / 180);
						Vector3 curVel = new Vector3 (0, 0, 0);
						curVel.x = this.velSize * Mathf.Sin (angleRad);
						curVel.z = this.velSize * Mathf.Cos (angleRad);
						return curVel;
				} else {
						return this.velocity;
				}
		}

		//Checking if there is no straight line to the next waypoint
		public bool checkStuck (List<Obstacle> obstacles, Vector3 nextWaypoint)
		{

				Line travelLine = new Line (this.agent.transform.position, nextWaypoint);

				foreach (Obstacle obs in obstacles) {
						foreach (Line line in obs.edges) {
								if (line.intersect (travelLine)) {
										return true;
								}
						}
				}
				return false;
		}

		public bool checkStraightWayToGoal (List<Obstacle> obstacles)
		{

				Line travelLine = new Line (this.agent.transform.position, this.goalPos);

				foreach (Obstacle obs in obstacles) {
						foreach (Line line in obs.edges) {
								if (line.intersect (travelLine)) {
										return false;
								}
						}
				}
				return true;

		}

		public Vector3 calculateNewVelocityCar (float acceleration, float turnDegrees, float carLength)
		{
		
				float newVelSize = this.velSize + acceleration;
		float wheelAngleRad = turnDegrees * (Mathf.PI / 180);
		float dTheta=(newVelSize/carLength)*Mathf.Tan(wheelAngleRad);
		
		Vector3 newRotation = this.velocity;
		newRotation.y = newRotation.y + dTheta;

				float angleRad = newRotation.y * (Mathf.PI / 180);
				Vector3 newVel = new Vector3 (0, 0, 0);
				newVel.x = newVelSize * Mathf.Sin (angleRad);
				newVel.z = newVelSize * Mathf.Cos (angleRad);

		
				return newVel;
		
		}
	
		public Vector3 calculateNewVelocity (float acceleration, Vector3 accelerateTowards)
		{
		
				Vector3 dir;
		
				dir = accelerateTowards;
		
				Vector3 dynPVel = this.velocity;
		
				Vector3 change = Vector3.Normalize (dir);
		
				float accInX = change.x / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
				float accInZ = change.z / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
		
		
				if (float.IsNaN (accInX)) {
						accInX = 0;
				}
				if (float.IsNaN (accInZ)) {
						accInZ = 0;
				}
		
				dynPVel.x = dynPVel.x + acceleration * accInX;
				dynPVel.z = dynPVel.z + acceleration * accInZ;
		
		
				return dynPVel;
		
		}
	
		private float findIntersectionPoint (Vector3 newVelocity, T5Agent otherAgent)
		{
		
				Vector3 velToCheck3 = new Vector3 (0, 0, 0);
				if (this.isCar) {
						velToCheck3 = 2 * newVelocity - this.getCarVelocity ();
				} else {
						velToCheck3 = 2 * newVelocity - this.velocity;
				}
				Vector2 velToCheck = new Vector2 ();
				velToCheck.x = velToCheck3.x;
				velToCheck.y = velToCheck3.z;

		
				Vector2 curPos = new Vector2 ();
				curPos.x = this.agent.transform.position.x;
				curPos.y = this.agent.transform.position.z;
				Vector2 otherPos = new Vector2 ();
				otherPos.x = otherAgent.agent.transform.position.x;
				otherPos.y = otherAgent.agent.transform.position.z;
				Vector2 toCircleCenter = otherPos - curPos;
				Vector2 otherAgentVel = new Vector2 ();
				if (otherAgent.isCar) {
						Vector3 otherAgentVel3 = otherAgent.getCarVelocity ();
						otherAgentVel.x = otherAgentVel3.x;
						otherAgentVel.y = otherAgentVel3.z;
				} else {
						otherAgentVel.x = otherAgent.velocity.x;
						otherAgentVel.y = otherAgent.velocity.z;
				}
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
		
				float distance = Mathf.Abs (Mathf.Sin (angle) * toCircleCenter.magnitude);
				//Debug.Log ("Distance:" + distance);
		
		
				//If the distance is less than the radius the velocity is not ok
				if (distance <= r) {
			
						float distAlongRay = Mathf.Abs (Mathf.Cos (angle) * toCircleCenter.magnitude);
						float distInside = Mathf.Pow (r, 2) - Mathf.Pow (distance, 2);
						float distToIntersect = distAlongRay - distInside;
			
						float timeToIntersect = distToIntersect / relativeVel.magnitude;
						//Debug.Log("Relative Vel magnitude:"+relativeVel.magnitude);

						//Debug.Log ("Line cut circle");
						return timeToIntersect;
				} else {
						return float.PositiveInfinity;
				}

		}

		/**
	 * Check if the agent will get to the goal before it collides 
	 */
		private bool goalBeforeCollision (T5Agent otherAgent, Vector3 waypoint)
		{

				float distToGoal = Vector3.Distance (this.agent.transform.position, waypoint);
				float distToOther = Vector3.Distance (this.agent.transform.position, otherAgent.agent.transform.position);

		Vector3 otherAgentVel = new Vector3 (0, 0, 0);
		if (otherAgent.isCar && otherAgent.velSize != 0f) {
						otherAgentVel = otherAgent.getCarVelocity ();
				} else {
			otherAgentVel=otherAgent.velocity;
				}

				if (Vector3.Equals (otherAgentVel, Vector3.zero) && distToGoal < distToOther) {
						//Debug.Log("GoalBeforeCol true");
						return true;
				}
				return false;
		}
	
		public float calculateTimeToCollision (Vector3 newVelocity, List<T5Agent> agents, float goalInterval, Vector3 waypoint)
		{
		
				float minTimeToCollision = float.PositiveInfinity;
		
				for (int i=0; i<agents.Count; i++) {
						T5Agent curAgent = agents [i];
						if (!string.Equals (this.id, curAgent.id) && !this.goalBeforeCollision (curAgent, waypoint)) {
								if (!curAgent.isAtGoal (goalInterval)) {
										if (this.priority <= curAgent.priority) {
												float timeToCollision = this.findIntersectionPoint (newVelocity, curAgent);
												if (timeToCollision < minTimeToCollision && timeToCollision < neighborTimeLimit) {
														minTimeToCollision = timeToCollision;
												}
										}
								} else {
										float timeToCollision = this.findIntersectionPoint (newVelocity, curAgent);
										if (timeToCollision < minTimeToCollision && timeToCollision < neighborTimeLimit) {
												minTimeToCollision = timeToCollision;
										}
								}
						}
				}
		
				return minTimeToCollision;
		}
	
		private float calculatePenalty (Vector3 newVelocity, float timeToCollision, Vector3 prefVel)
		{
		
				float insideRVOpenalty = 0;
		
				if (!float.IsPositiveInfinity (timeToCollision)) {
						insideRVOpenalty = weightPenalty * (1.0f / timeToCollision);
						//Debug.Log("CollisionRisk. Adding to penalty:"+insideRVOpenalty);
				}

				float velDist = Vector3.Distance (newVelocity, prefVel);
		
				float totPenalty = insideRVOpenalty + velDist;
		
				return totPenalty;
		
		}

		private float calculateTimeToColWithObstacle (List<Obstacle> obstacles, Vector3 newVelocity, Vector3 waypoint)
		{

				Vector3 lineStart = this.agent.transform.position;
				Vector3 lineEnd = lineStart + timeStepLimit * newVelocity;
				Line velLine = new Line (lineStart, lineEnd);

				float timeToCol = float.PositiveInfinity;

				foreach (Obstacle obs in obstacles) {
						foreach (Line line in obs.edges) {
								float tempTime = line.intersectTime (velLine);
								Vector3 intersecPoint = lineStart + tempTime * newVelocity;
								float distToWaypoint = Vector3.Distance (lineStart, waypoint);
								float distToIntersect = Vector3.Distance (lineStart, intersecPoint);
								//Because the line is extended by timeStepLimit
								tempTime = tempTime * timeStepLimit;
								if (distToIntersect < distToWaypoint && tempTime < timeToCol) {
										timeToCol = tempTime;
								}
						}
				}
				return timeToCol;
		}
	
	
}