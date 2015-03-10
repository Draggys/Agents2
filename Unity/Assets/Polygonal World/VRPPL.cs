using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Vehicle Routing Problem Polygonal Logic */
public class VRPPL {

	List<Vector3> customers;
	int size;

	public VRPPL(List<Vector3> customers, int agents) {
		this.customers = customers;
		size = agents;
	}

	public KeyValuePair<PolyAgent, Vector3> NextAgentAndCustomer(List<PolyAgent> agents) {
		if (customers.Count == 0) {
			throw new UnassignedReferenceException("customers list is empty");
		}

		Vector3 ret = Vector3.zero;
		float minCost = -1;
		int j = -1;
		PolyAgent closestAgent = null;
		foreach(PolyAgent agent in agents) {
			for (int i = 0; i < customers.Count; i++){
				float cost = Dist(customers[i], agent);
				if(minCost == -1 || cost < minCost) {
					minCost = cost;
					j = i;
					ret = customers[i];
					closestAgent = agent;
				}
			}
		}

		customers.RemoveAt(j);
		return new KeyValuePair<PolyAgent, Vector3> (closestAgent, ret);
	}

	private float Dist(Vector3 customer, PolyAgent agent) {
		float first = Vector3.Distance (customer, agent.agent.transform.position);
		float second = Vector3.Distance (customer, agent.end);
		
		if (customers.Count / size > 0.5)
			return first;
		return first + second;
	}
}
