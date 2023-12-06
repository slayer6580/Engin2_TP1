using MBT;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode(name = "Alex Leaf/Set ressource from Pack to closest worker")]

public class Tommy_RessourceAvailable : Leaf
{
	public GameObjectReference m_workerGo = new GameObjectReference();
	private Worker_Team m_worker;
	public override NodeResult Execute()
	{
		m_worker = m_workerGo.Value.GetComponent<Worker_Team>();
		Collectible_Team closestCollectible = null;

		float minDistance = 10000; // test


		// si le worker n'a pas de ressource
		if (m_worker.m_reservedCollectible == null)
		{
			// regarder dans la liste des ressource � utiliser
			foreach (Collectible_Team collectible in Collecting_Manager._Instance.m_ressourceToUse)
			{
				float distanceBetweenRessourceAndWorker = Vector2.Distance(m_worker.transform.position, collectible.transform.position);

				// prendre la ressource la plus proche du worker
				if (collectible.m_designedWorker == null && distanceBetweenRessourceAndWorker < minDistance)
				{
					minDistance = distanceBetweenRessourceAndWorker;
					closestCollectible = collectible;

				}

			}

			// si une ressource disponible la plus proche es trouv�, donner cette ressource au worker et r�serv� des deux bords
			if (closestCollectible != null)
			{
				closestCollectible.m_designedWorker = m_worker;
				m_worker.m_reservedCollectible = closestCollectible;
				m_worker.m_campPosition = closestCollectible.m_associatedCamp;

				Collecting_Manager._Instance.m_ressourceToUse.Remove(closestCollectible);
				return NodeResult.success;
			}

		}
		return NodeResult.failure;
	}
}
