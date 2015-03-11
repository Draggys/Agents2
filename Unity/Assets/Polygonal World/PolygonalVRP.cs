using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolygonalVRP : MonoBehaviour {

	Model model;
	PolyMapLoaderT3 map;
	PolyData mapData;
	List<PolyAgent> agents = null;
	VRPPL vrp;

	float R;

	// Use this for initialization
	void Start () {
		map = new PolyMapLoaderT3 ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button" , "polygMap1/customerPos");
		mapData = map.polyData;
		R = 5;

		agents = new List<PolyAgent> ();
		for(int i = 0; i < mapData.start.Count; i++) {
			Vector3 start = mapData.start[i];
			Vector3 end = mapData.end[i];
			agents.Add (new PolyAgent("Agent " + i, start, end, R, "point"));
			agents [i].agent.renderer.material.color = Color.blue;
			agents[i].model = gameObject.AddComponent<DynamicPointModel> ();
		}

		vrp = new VRPPL (mapData.customers, agents.Count);

		foreach (PolyAgent agent in agents) {
			List<Vector3> path = new List<Vector3> ();
			path.Add (agent.end);
			agent.model.SetPath (path, agent, mapData.lines);
			agent.model.StartCoroutineMove();
		}

    }


	void OnDrawGizmos() {
		map = new PolyMapLoaderT3 ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button" , "polygMap1/customerPos");

		if (map.polyData.nodes != null) {
			foreach(Vector3 node in map.polyData.nodes) {
				Gizmos.color = Color.blue;
				Gizmos.DrawCube (node, Vector3.one);
			}
			foreach(Line line in map.polyData.lines){
				Gizmos.color=Color.black;
                Gizmos.DrawLine(line.point1,line.point2);
			}
		}

		Gizmos.color = Color.green;
		foreach (Vector3 v in map.polyData.start) {
			Gizmos.DrawCube (v, new Vector3(3,1,3));
        }
		Gizmos.color = Color.red;
		foreach (Vector3 v in map.polyData.end) {
			Gizmos.DrawCube (v, new Vector3 (3, 1, 3));
		}
		Gizmos.color = Color.blue;
		foreach (Vector3 v in map.polyData.customers) {
			Gizmos.DrawCube (v, new Vector3 (3, 1, 3));
		}
		if (agents != null) {
			Gizmos.color = Color.magenta;
			foreach (PolyAgent pagent in agents) {
				Gizmos.DrawSphere (pagent.agent.transform.position, R);
			}
		}
    }
}
