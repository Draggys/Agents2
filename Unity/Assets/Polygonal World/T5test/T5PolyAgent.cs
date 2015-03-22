using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T5PolyAgent{
	public Vector3 waypoint;
	public Vector3 end;
	public GameObject agent;
	public string id;
	public float R;
	public T5DynCar model;
	public bool running;
	public List<Vector3> visited;
	public bool dontMove = false;
	
	public T5PolyAgent(string id, Vector3 start, Vector3 end, float r, string type) {
		this.id = id;
		if (type == "car") {
			agent = GameObject.CreatePrimitive (PrimitiveType.Cube);
			// Car size
			agent.transform.localScale = new Vector3(5, 1, 10);
		} else {
			agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			// Point size
			agent.transform.localScale = new Vector3(5, 5, 5);
		}
		agent.transform.position = start;
		R = r;
		this.end = end;
		running = false;
		visited = new List<Vector3> ();
	}
}