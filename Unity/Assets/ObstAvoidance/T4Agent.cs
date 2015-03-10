using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T4Agent {
	public Vector3 pos;
	public Vector3 goalPos;
	public int wp;
	public bool wpReached;
	public GameObject agent;
	public string id;
	public Vector3 velocity;
	public List<float> lastAngles;
	public float agentSize;
	public float timeStepLimit;
	public float angleEqLimit;
	
	public T4Agent(string id, Vector3 start, Vector3 goalPos) {
		this.goalPos = goalPos;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start;
		pos = start;
		wp = 0;
		wpReached = false;
		velocity = new Vector3 (0,0,0);
		agentSize = 0.5f;
		timeStepLimit =300f;
		angleEqLimit = 10f;
	}



	/*
	 * Function to check if there is a collision risk. If it is
	 * the function will return a direction vector that the agent should use
	 * in the next timestep. 
	 */
	public Vector3 collisionDetection(List<T4Agent> agents, float acceleration){
		//If we have the last angles between the agent and the other agents
		if (lastAngles != null) {
			Vector3 dirVector=new Vector3(0,0,0);
			for(int i=0;i<agents.Count;i++){
				T4Agent curAgent=agents[i];
				Vector3 thisAgentPos = this.agent.transform.position;
				Vector3 otherAgentPos = curAgent.agent.transform.position;
				//If its not the same agent
				if(!string.Equals(curAgent.id,this.id)){
					float timeSteps=this.timeStepsBetween(curAgent,acceleration);
					if(timeSteps<=timeStepLimit){
						float angle=Vector3.Angle(thisAgentPos-otherAgentPos,Vector3.right);
						float lastAngle=lastAngles[i];
						//If the difference between lastAngle and the current angle is small enough
						if(lastAngle-angle<angleEqLimit){
							Vector3 temp=thisAgentPos-otherAgentPos;
							dirVector=dirVector+new Vector3((-1f/Mathf.Pow(timeSteps,2))*temp.x,0,(-1f/Mathf.Pow(timeSteps,2))*temp.z);
						}
						lastAngles[i]=angle;
					}
				}

			}
			return dirVector;
				} 
		//If we dont have any previous angles we just calculate the current and save them
		else {
			lastAngles=new List<float>();
			for(int i=0;i<agents.Count;i++){
				T4Agent curAgent=agents[i];
				//If it's not the same agent
				if(!string.Equals(curAgent.id,this.id)){
					float angle=Vector3.Angle(this.agent.transform.position-curAgent.agent.transform.position,Vector3.right);
					lastAngles.Add(angle);
				}
				else{
					//We have to add something to the list of angles to keep the same order
					lastAngles.Add(0.0f);
				}

			}
			return new Vector3(0,0,0);
				}


	}

	/**
	 * Function to calculate the number of timesteps between the
	 * current agent and some other agent
	 */
	private float timeStepsBetween(T4Agent otherAgent, float acceleration){

		Vector3 thisAgentPos = this.agent.transform.position;
		Vector3 otherAgentPos = otherAgent.agent.transform.position;

		float distance = Vector3.Distance (thisAgentPos, otherAgentPos);
		//subtract 2*agentSize from the distance to account for the size of the agents
		distance = distance - 2 * agentSize;
		if (distance <= 0) {
			Debug.LogError("Two agents collided");
			distance=0;
				}

		//Calculate the summed speed for both agents in the direction directly towards eachother
		float speed = acceleration;
		float angle = Vector3.Angle (thisAgentPos-otherAgentPos, this.velocity);
		//Transform the angle to radians
		angle = angle * (Mathf.PI / 180.0f);
		speed = speed + this.velocity.magnitude * Mathf.Cos (angle);
		//And now for the other agents speed
		angle = Vector3.Angle (otherAgentPos - thisAgentPos, otherAgent.velocity);
		angle = angle * (Mathf.PI / 180.0f);
		speed = speed + otherAgent.velocity.magnitude * Mathf.Cos (angle);

		float timeSteps=(distance/(speed*Time.deltaTime));
		return timeSteps;
	}


}