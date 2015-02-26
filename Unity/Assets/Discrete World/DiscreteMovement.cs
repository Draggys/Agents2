using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent {
	public Node start;
	//public Node end;
	public List<Node> waypoints;
	public GameObject agent;
	public string id;
	
	public Agent(string id, Node start, List<Node> waypoints) {
		this.start = start;
		this.waypoints = waypoints;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start.worldPosition;
	}
}

public class DiscreteMovement : MonoBehaviour {
	
	List<Agent> agents = new List<Agent> ();
	AStar astar;
	Grid grid;

	List<Node> debug = null;
	
	void Start () {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();
		
		astar = new AStar (grid.rTable);
		Node startNode = grid.grid [Convert.ToInt32(grid.mapData.start.x), Convert.ToInt32 (grid.mapData.start.y)];
		Node endNode = grid.grid [Convert.ToInt32 (grid.mapData.end.x), Convert.ToInt32 (grid.mapData.end.y)];

		List<Node> w1 = new List<Node> ();
		w1.Add (endNode);
		w1.Add (startNode);
		w1.Add (grid.grid [19, 1]);
		agents.Add (new Agent("Blue", startNode, w1));
		agents [0].agent.renderer.material.color = Color.blue;
		agents.Add (new Agent ("Green", endNode, w1));
		agents [1].agent.renderer.material.color = Color.green;
		agents.Add (new Agent ("Black", grid.grid[0, 10], w1));
		agents [2].agent.renderer.material.color = Color.black;


	//	Node node = grid.grid [19, 0];
	//	debug = node.neighbours;

		StartAllAgents ();
	}
	
	public void StartAllAgents() {
		bool moving = true;
		foreach (Agent agent in agents) {
			StartCoroutine (Move (agent));
		}
	}
	
	public List<Node> RequestPath(Node startNode, Node endNode) {
		List<Node> path = astar.STAStar (startNode, endNode);
		
		/*
		int i = 1;
		foreach (Node node in path) {
			State state = new State(node.gridPosX, node.gridPosY, i++);
			grid.rTable.Add (state, 1);
		}
		*/
		
		int i = 1;
		for(int j = 0; j < path.Count; j++) {
			if(j != path.Count - 1) {
				State cState = new State(path[j].gridPosX, path[j].gridPosY, i);
				State nState = new State(path[j+1].gridPosX, path[j+1].gridPosY, i); 
				grid.rTable.Add (cState, 1);
				grid.rTable.Add (nState, 2);
			}
			i++;
		}
		
		return path;
	}
	
	IEnumerator Move(Agent agent) {
		Node start = agent.start;
		int r = 0;
		Node end = agent.waypoints[r];
		List<Node> path;
		while (true) {
		StartOver:
			Node pastNode = null;
			print ("----------");
			path = RequestPath (start, end);
			/*
			print (agent.id + ": Requesting: " + start.worldPosition + " -> " + end.worldPosition + "\n"
			       + "With result: " + path.Count
			       + " My position: " + agent.agent.transform.position);
			       */
			print (agent.id + " Requesting: " + "[" + start.gridPosX + ", " + start.gridPosY + "] -> [" +
			       end.gridPosX + "," + end.gridPosY + "]\n" + "With result: " + path.Count);
            
            if(path.Count == 0) {
				if(start == end) {
					r++;
					if(r < agent.waypoints.Count) {
						end = agent.waypoints[r];
					}
					else {
						print (agent.id + " yield break");
						yield break;
					}
				}

                print (agent.id + ": pause");
				yield return new WaitForSeconds(0.5f);
				goto StartOver;
			}
			
			int i = 1;	
			foreach (Node node in path) {
				agent.agent.transform.position = node.worldPosition;
				
				if (pastNode != null) {
					State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
					grid.rTable.Free (state);
					state.t = state.t - 1;
					grid.rTable.Free (state);

					//print (agent.id + " Freeing: " + state.x + ", " + state.y + ", " + state.t);
                }
                pastNode = node;
                /*
                print(agent.id + ": [" + node.gridPosX + ", " + node.gridPosY + ", " + i + "] "
                      + grid.rTable.Occupied (new State(node.gridPosX, node.gridPosY, i)));
                */
                //yield return null;
                yield return new WaitForSeconds (0.2f);
            }
            
            start = path[path.Count - 1];
            
            if(pastNode != null) {
                State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
				grid.rTable.Free (state);
				state.t = state.t - 1;
				grid.rTable.Free (state);
            }

        }
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		foreach (KeyValuePair<State, int> p in grid.rTable.rTable) {
			if(p.Value != 0)
				Gizmos.DrawCube (grid.grid [p.Key.x, p.Key.y].worldPosition - Vector3.up, Vector3.one);
        }
		Gizmos.color = Color.yellow;
		if (debug != null) {
			foreach (Node node in debug) {
				Gizmos.DrawCube (node.worldPosition, Vector3.one);
				//print ("drawing: " + node.gridPosX + ", " + node.gridPosY);
			}
		}
    }
}