using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent {
	public Node start;
	public Node end;
	public GameObject agent;
	public List<Node> path;
	public string id;

	public Agent(string id, Node start, Node end) {
		this.start = start;
		this.end = end;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = new Vector3 (agent.transform.position.x, 1, agent.transform.position.z);
	}
}

public class DiscreteMovement : MonoBehaviour {

	List<Agent> agents = new List<Agent> ();
	AStar astar;
	Grid grid;

	void Start () {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();

		astar = new AStar (grid.rTable);
		Node startNode = grid.grid [Convert.ToInt32(grid.mapData.start.x), Convert.ToInt32 (grid.mapData.start.y)];
		Node endNode = grid.grid [Convert.ToInt32 (grid.mapData.end.x), Convert.ToInt32 (grid.mapData.end.y)];

		agents.Add (new Agent ("agent1", endNode, startNode));
		agents [0].agent.renderer.material.color = Color.blue;
		agents.Add (new Agent ("agent2", startNode, endNode));
		agents [1].agent.renderer.material.color = Color.green;

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
	
		int i = 1;
		foreach (Node node in path) {
			State state = new State(node.gridPosX, node.gridPosY, i++);
			grid.rTable.Add (state, 1);
		}

		return path;
	}

	IEnumerator Move(Agent agent) {
		Node start = agent.start;
		while (true) {
		StartOver:
			Node pastNode = null;
			agent.path = RequestPath (start, agent.end);

			if(agent.path.Count == 0) {
				print (agent.id + ": pause");
				yield return new WaitForSeconds(0.5f);
				goto StartOver;
			}

			int i = 1;
			foreach (Node node in agent.path) {
				agent.agent.transform.position = node.worldPosition;
				if (pastNode != null) {
					State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
					grid.rTable.Add (state, 0);
				}
				pastNode = node;

				//yield return null;
				yield return new WaitForSeconds (0.5f);
			}
			
			start = agent.path[agent.path.Count - 1];

			if(pastNode != null && start != agent.end) {
				State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
				grid.rTable.Add (state, 0);
            }

		}
	}

	void OnDrawGizmos() {
	}
}
