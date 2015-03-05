using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRPD {
    
    public List<Node> customers;
	int bound;

    public VRPD(List<Node> customers, int agents) {
        this.customers = customers;
		this.bound = agents * 2;
    }

	public Node NextCustomer(Agent agent, Node endPos) {
        if (customers.Count == 0)
            return null;

        Node ret = null;
        float minCost = -1;
        int j = -1;
        for (int i = 0; i < customers.Count; i++){
            float cost = Dist(customers[i], agent, endPos);
            if(minCost == -1 || cost < minCost) {
                minCost = cost;
                j = i;
                ret = customers[i];
            }
        }

        customers.RemoveAt(j);
        return ret;
    }

	public KeyValuePair<Agent, Node> NextAgentAndCustomer(List<Agent> agents, Dictionary<Agent, Node> endPos) {
		if (customers.Count == 0) {
			throw new UnassignedReferenceException("customers list is empty");
		}

		Node ret = null;
		float minCost = -1;
		int j = -1;
		Agent closestAgent = null;
		foreach(Agent agent in agents) {
			for (int i = 0; i < customers.Count; i++){
				float cost = Dist(customers[i], agent, endPos[agent]);
				if(minCost == -1 || cost < minCost) {
					minCost = cost;
					j = i;
					ret = customers[i];
					closestAgent = agent;
				}
			}
		}
		
		customers.RemoveAt(j);
		return new KeyValuePair<Agent, Node> (closestAgent, ret);
	}

    private float Dist(Node customer, Agent agent, Node endPos) {
		float first = Mathf.Abs (customer.gridPosX - agent.pos.gridPosX) + Mathf.Abs (customer.gridPosY - agent.pos.gridPosY);
		float second = Mathf.Abs (endPos.gridPosX - customer.gridPosX) + Mathf.Abs (endPos.gridPosY - customer.gridPosY);

		if(customers.Count < bound)
			return first + second;
		return first;
	}
}
