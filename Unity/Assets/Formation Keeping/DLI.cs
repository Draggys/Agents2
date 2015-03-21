using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DLI : MonoBehaviour {
	List<PolyAgent> agents = null;
	int sepDist = 30; // Separation distance

	// Use this for initialization
	void Start () {
		agents = new List<PolyAgent> ();
		int numAgents = 10;
		int R = 5;
		Vector3 pos = new Vector3(1, 1, 1);
		for(int i = 0; i < numAgents; i++) {
			pos = new Vector3(Random.Range(-300.0F, 300.0F), 1, Random.Range(-300.0F, 300.0F));
			//pos = new Vector3(Random.Range(-50.0F, 50.0F), 1, Random.Range(-50.0F, 50.0F));
			pos.y = 1;
			agents.Add (new PolyAgent(i + "", pos, pos, R, "car"));
			agents [i].agent.renderer.material.color = Color.yellow;
			agents[i].model = gameObject.AddComponent<DynamicCarModel> ();
			//agents[i].model = gameObject.AddComponent<DynamicPointModel> ();
		}
	}

	// Update is called once per frame
	void Update () {

		if (agents != null) {
			foreach (PolyAgent agent in agents) {
				if(agent.running)
					agent.model.StopCoroutineMove();
				if (!agent.running) {
					List<Vector3> path = new List<Vector3> ();
					Vector3 pos = GetNewPos (agent);
					pos.y = 1;
					path.Add (pos);
					agent.model.SetPath (path, agent, new List<Line> ());
					agent.model.StartCoroutineMove();
				}
			}
		}

	}

	Vector3 Cohesion(PolyAgent me) {
		Vector3 avg = me.agent.transform.position;
		foreach(PolyAgent agent in agents) {
			if(agent != me) {
				avg = avg + agent.agent.transform.position;
			}
		}
		avg = avg / (agents.Count - 2);
		return (avg - me.agent.transform.position);
	}

	Vector3 Separation(PolyAgent me) {
		Vector3 c = Vector3.zero;
		foreach (PolyAgent agent in agents) {
			if(agent != me) {
				if(Vector3.Distance (agent.agent.transform.position, 
				                     me.agent.transform.position) < sepDist) {
					c = c - (agent.agent.transform.position - me.agent.transform.position);
				}
			}
		}
		return c*5;
	}

	public Vector3 GetNewPos(PolyAgent me) {
		Vector3 v1 = Cohesion (me);
		Vector3 v2 = Separation (me);
		return v1 + v2;
	}
}
