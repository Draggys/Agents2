using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VGraph{

	public Dictionary<Vector3, PolyNode> graph;

	public VGraph() {
		graph = new Dictionary<Vector3, PolyNode> ();
	}

	/*
	public void PrintGraph() {
		foreach (KeyValuePair<Vector3, PolyNode> node in graph) {
			print (node.Value.pos + " has neighbours: " + node.Value.neighbours.Count);

			//foreach(Vector3 pos in node.Value.neighbours) {
			//	print (pos);
			//}
		}
	}
	*/
}
