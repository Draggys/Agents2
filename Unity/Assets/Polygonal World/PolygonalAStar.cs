using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PolygonalAStar{

	public List<PolyNode> AStarSearch(Vector3 start, Vector3 end, VGraph graph) {
		if (start == end)
			return new List<PolyNode>();

		PolyNode targetNode = graph.graph [end];
		PolyNode startNode = graph.graph [start];
		
		PriorityQueue<PolyNode, float> frontier = new PriorityQueue<PolyNode, float> ();
		Dictionary<PolyNode, PolyNode> cameFrom = new Dictionary<PolyNode, PolyNode> ();
		Dictionary<PolyNode, float> costSoFar = new Dictionary<PolyNode, float> ();
	
		frontier.Enqueue(startNode, 0f);
		cameFrom [startNode] = null;
		costSoFar [startNode] = 0;
		
		PolyNode currentNode;
		while (frontier.Count() != 0) {
			currentNode = frontier.Dequeue ();
			
			if(currentNode == targetNode) {
				return ConstructPath (startNode, targetNode, cameFrom);
			}
			
			foreach(Vector3 nodePos in currentNode.neighbours){
				float newCost = costSoFar[currentNode] + GetCost (currentNode, graph.graph[nodePos]);
				if (!costSoFar.ContainsKey (graph.graph[nodePos]) || newCost < costSoFar[graph.graph[nodePos]]) {
					costSoFar[graph.graph[nodePos]] = newCost;
					float priority = newCost + Heuristic (targetNode.pos, nodePos);
					frontier.Enqueue (graph.graph[nodePos], priority);
					cameFrom[graph.graph[nodePos]] = currentNode;
				}
			}
		}
		
		return new List<PolyNode> ();
	}
	
	float Heuristic(Vector3 A, Vector3 B) {
		//return Mathf.Abs (A.x - B.x) + Mathf.Abs (A.y - B.y);
		return Vector3.Distance (A, B);
	}

	public float GetCost(PolyNode from, PolyNode to) {
		//return Mathf.Abs (to.pos.x - from.pos.x) + Mathf.Abs (to.pos.y - from.pos.y);
		return Vector3.Distance (from.pos, to.pos);
	}

	List<PolyNode> ConstructPath(PolyNode start, PolyNode target, Dictionary<PolyNode, PolyNode> cameFrom) {
		PolyNode currentNode = target;
		List<PolyNode> path = new List<PolyNode>();
		while (currentNode != start) {
			path.Add (currentNode);
			currentNode = cameFrom[currentNode];
		}
		path.Reverse ();
		
		return path;
	}
	

}
