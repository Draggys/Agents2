using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VirtualStructure : MonoBehaviour {

	Vector3[] pos;
	List<PolyAgent> agents;
    List<Vector3> structureWP;
	float vel;
	Transform transform;

	int numAgents;
	int R = 3;
	int updateSpeed;

	void Start () {
		vel = 10;
		updateSpeed = 1;
		List<PolyAgent> agents = new List<PolyAgent> ();
		numAgents = 4;
		for(int i = 0; i < numAgents; i++) {
			agents.Add (new PolyAgent(i + "", Vector3.zero, Vector3.zero, R, "car"));
			agents [i].agent.renderer.material.color = Color.yellow;
			agents[i].model = gameObject.AddComponent<DynamicCarModel> ();
			//agents[i].model = gameObject.AddComponent<DynamicPointModel> ();
        }
        
		List<Vector3> start = new List<Vector3> ();
		start.Add (new Vector3 (100, 1, 100));
		start.Add (new Vector3 (200, 1, 100));
		start.Add (Vector3.zero);
		start.Add (new Vector3 (0, 1, -100));

        VirtualStructure vs = new VirtualStructure (start, agents);
		StartCoroutine (MoveStructure(vs));
		StartCoroutine (UpdateStructurePos (vs));
	}

	public VirtualStructure(List<Vector3> wp, List<PolyAgent> agents) {
		pos = new Vector3[agents.Count];
		structureWP = wp;
		vel = 50;
		transform = GameObject.CreatePrimitive (PrimitiveType.Capsule).transform;

		this.agents = agents;
		// TODO automate this for any formation size
        int i = 0;
		foreach (PolyAgent agent in agents) {
			Vector3 dir = transform.forward;
			Vector3 left = Vector3.Cross (dir, Vector3.up);
            
            if (i == 0)
                pos[i] = transform.position + left * 10;
            if (i == 1)
                pos[i] = transform.position + dir * 10;
			if (i == 2)
				pos[i] = transform.position - left * 10;
			if(i == 3)
				pos[i] = transform.position - dir * 10;

			agent.agent.transform.position = pos[i];
			i++;
		}
    }
	

	void UpdatePos() {
		for(int i = 0; i < agents.Count; i++) {
			Vector3 dir = transform.forward;
			Vector3 left = Vector3.Cross (dir, Vector3.up);
			
			if (i == 0)
				pos[i] = transform.position + left * 10;
			if (i == 1)
				pos[i] = transform.position + dir * 10;
			if (i == 2)
				pos[i] = transform.position - left * 10;
			if(i == 3)
				pos[i] = transform.position - dir * 10;

        }
    }
    
	IEnumerator UpdateStructurePos(VirtualStructure vs) {
		while (true) {
			vs.UpdatePos ();

			int j = 0;
			foreach(PolyAgent agent in vs.agents) {
				if(Vector3.Distance (agent.agent.transform.position, vs.pos[j]) > 5) {
					if(agent.running)
						agent.model.StopCoroutineMove();
					List<Vector3> path = new List<Vector3> ();
					path.Add (vs.pos[j]);
					agent.model.SetPath (path, agent, new List<Line> ());
                    agent.model.StartCoroutineMove();
                }
                j++;
            }
            yield return new WaitForSeconds (updateSpeed);
		}
	}
    
    IEnumerator MoveStructure(VirtualStructure vs) {
		int i = 0;
		while (i < vs.structureWP.Count) {
			vs.transform.position = Vector3.MoveTowards (vs.transform.position,
			                                             vs.structureWP[i], 
			                                             vel * Time.deltaTime);
			if(Vector3.Distance (vs.transform.position, vs.structureWP[i]) < 5){
				i = (i + 1) % vs.structureWP.Count;	
			}

			//vs.UpdatePos ();
			yield return null;
		}
	}
	
	void OnDrawGizmos () {
		if (pos != null) {
			Gizmos.color = Color.red;
			foreach(Vector3 p in pos) {
				Gizmos.DrawSphere (p, 20);
			}
		}
	}


}
