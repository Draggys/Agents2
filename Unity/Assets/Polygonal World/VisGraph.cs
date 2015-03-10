using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class VisGraph : MonoBehaviour {

	List<Obstacle> obstacles = new List<Obstacle> ();
	PolyData polyData = null;

	List<Line> walkableLines;
	VGraph graph;
	PolygonalAStar pathFinder = new PolygonalAStar();

	public float kinematic_vel;
	PolyMapLoaderT3 map;

	List<PolyAgent> agents = null;
	VRPPL vrp = null;
	float R;
	List<PolyAgent> ready = null;
	Stopwatch stopwatch = new Stopwatch();

	void Start() {
		map = new PolyMapLoaderT3 ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button" , "polygMap1/customerPos");
		polyData = map.polyData;

		graph = new VGraph();
		walkableLines = new List<Line> ();

		// START VRP
		R = 3;
		agents = new List<PolyAgent> ();
		for(int i = 0; i < polyData.start.Count; i++) {
			Vector3 start = polyData.start[i];
			Vector3 end = polyData.end[i];
			agents.Add (new PolyAgent("Agent " + i, start, end, R));
			agents [i].agent.renderer.material.color = Color.blue;
			agents[i].model = gameObject.AddComponent<DynamicPointModel> ();
		}

		List<Vector3> customers = new List<Vector3> (); // for plotting
		foreach(Vector3 v in polyData.customers)
			customers.Add (v);
		vrp = new VRPPL (customers, agents.Count);


		// END VRP

		CreateObstacles ();
		ConstructWalkableLines ();
		CreateInterObstacleWalk ();
	//	print ("Walkable lines: " + walkableLines.Count);
//		graph.PrintGraph ();

		stopwatch.Start();
		ready = new List<PolyAgent> ();
	}

	bool time = true;
	void Update() {
		
		foreach (PolyAgent agent in agents) {
			if (!agent.running && !ready.Contains (agent)) {
				ready.Add (agent);
			}
		}
		
		bool emptyReady = false;
		while (ready.Count != 0) {
			KeyValuePair<PolyAgent, Vector3> agentWP;
			try {
				agentWP = vrp.NextAgentAndCustomer (ready);
			} catch {
				emptyReady = true;
				break;
			}
			
			Vector3 start;
			if (agentWP.Key.visited.Count == 0)
				start = agentWP.Key.agent.transform.position;
			else
				start = agentWP.Key.visited [agentWP.Key.visited.Count - 1];
			Vector3 end = agentWP.Value;
			List<PolyNode> ppath = pathFinder.AStarSearch (start, end, graph);
			List<Vector3> path = new List<Vector3> ();
			foreach (PolyNode p in ppath) 
				path.Add (p.pos);
			path.Add (end);
			agentWP.Key.visited.Add (end);
			agentWP.Key.model.SetPath (path, agentWP.Key, polyData.lines);
			agentWP.Key.model.StartCoroutineMove ();
			ready.Remove (agentWP.Key);
		}
		
		if (emptyReady) {
			foreach (PolyAgent agent in ready) {
				Vector3 s = agent.visited [agent.visited.Count - 1];
				Vector3 e = agent.end;
				List<PolyNode> eppath = pathFinder.AStarSearch (s, e, graph);
				List<Vector3> epath = new List<Vector3> ();
				foreach (PolyNode p in eppath) 
					epath.Add (p.pos);
				epath.Add (e);
				agent.visited.Add (e);
				agent.model.SetPath (epath, agent, polyData.lines);
				agent.model.StartCoroutineMove ();
			}
			ready.Clear ();
			emptyReady = false;
			
		}
		
		bool done = true;
		foreach (PolyAgent agent in agents) {
			if(Vector3.Distance (agent.agent.transform.position, agent.end) > 1) {
				done = false;
				break;
			}
		}
		if (done) {
			if(time) {
				stopwatch.Stop ();
				print ("Time elapsed: " + stopwatch.Elapsed);
				time = false;
			}
		}
	}

	public void CreateInterObstacleWalk() {
		foreach (Obstacle obs in obstacles) {
			foreach(Line line in obs.edges) {
				walkableLines.Add (new Line(line.point1, line.point2)); // debug
				walkableLines.Add (new Line(line.point2, line.point1)); // debug

				graph.graph[line.point1].neighbours.Add (line.point2);
				graph.graph[line.point2].neighbours.Add (line.point1);
			}
		}
	}

	public void CreateObstacles() {
		int j = 0;
		Obstacle obs = new Obstacle ();
		foreach (int i in polyData.buttons) {
			obs.vertices.Add (polyData.nodes[j]);
			obs.id = j;
			if(i != 3) {
				//print (j);
				obs.edges.Add (polyData.lines[j++]);
			}
			else {
				obs.edges.Add (polyData.lines[j++]);
				obstacles.Add(new Obstacle(obs));
				obs.edges.Clear ();
				obs.vertices.Clear ();
			}
		}

		// Start
		foreach (Vector3 start in polyData.start) {
			Obstacle o = new Obstacle();
			o.id = j++;
			o.vertices.Add (start);
			obstacles.Add (o);
		}

		// End
		foreach (Vector3 end in polyData.end) {
			Obstacle o = new Obstacle();
			o.id = j++;
			o.vertices.Add (end);
			obstacles.Add (o);
		}

		// Customers
		foreach (Vector3 customer in polyData.customers) {
			Obstacle o = new Obstacle();
			o.id = j++;
			o.vertices.Add (customer);
			obstacles.Add (o);
		}
	}

	void ConstructWalkableLines() {

		// For every obstacle and it's neighbours
		// See if a line can be drawn from each vertex from the current obstacle to 
		// it's neighbours' vertices.
		// If the line does not intersect with any other line, add it to walkableLines
		int index = 0;
		foreach (Obstacle obs in obstacles) {
			foreach (Obstacle neigh in obstacles) {
				if(obs != neigh) {
					foreach(Vector3 vertex in obs.vertices) {
						PolyNode currentNode = new PolyNode();
						currentNode.pos = vertex;
						foreach(Vector3 neighVertex in neigh.vertices) {
							Line potentialLine = new Line(vertex, neighVertex);
							if(!IntersectsWithAnyLine(potentialLine)){
								walkableLines.Add (potentialLine); //debugging

								currentNode.neighbours.Add (neighVertex);
							}
						}
						if(graph.graph.ContainsKey (vertex)) 
							graph.graph[vertex].neighbours.AddRange (currentNode.neighbours);
						else
							graph.graph[vertex] = currentNode;
					}
				}
			}
			index++;
		}
	}

	public bool IntersectsWithAnyLine(Line myLine) {
		foreach (Obstacle obs in obstacles) {
			foreach (Line line in obs.edges) {
				if (myLine.point1 == line.point1 || myLine.point1 == line.point2)
					continue;
				if (myLine.point2 == line.point1 || myLine.point2 == line.point2)
					continue;

				if (myLine.intersect (line)) {
					//print (myLine.point1 + ", " + myLine.point2 + " vs " + line.point1 + ", " + line.point2);
					return true;
				}
			}
		}
		return false;
    }
    
    void OnDrawGizmos() {

		if (walkableLines != null) {
			foreach(Line line in walkableLines) {
				Gizmos.color = Color.grey;
		//		Gizmos.DrawLine(line.point1, line.point2);
			}
		}


		if (polyData != null) {
			foreach(Obstacle ob in obstacles) {
				foreach(Line line in ob.edges) {
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine (line.point1, line.point2);
				}
				foreach(Vector3 v in ob.vertices) {
					Gizmos.color = Color.white;
		//			Gizmos.DrawSphere (v, 5);
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
}
