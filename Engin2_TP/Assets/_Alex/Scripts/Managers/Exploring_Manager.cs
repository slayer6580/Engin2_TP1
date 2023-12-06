using System.Collections.Generic;
using UnityEngine;

public class Exploring_Manager : MonoBehaviour
{
    [Header("Le nombre d'exploreur au d�but de la simulation")]
    public int m_nbOfExploringWorkers;

    [SerializeField] private GameObject m_zone;

    private int m_mapDimension = 0;
    private int m_workerInExploration = 0;
    private float m_pourcentageOfTotalTimeExploration;

    private List<float> m_xPositions = new List<float>();
    private List<float> m_yPositions = new List<float>();

    [HideInInspector] public int m_zoneLenght = 0;
    [HideInInspector] public List<Vector2> m_zonesPositions = new List<Vector2>();
    [HideInInspector] public List<bool> m_zonesIsDetected = new List<bool>();
    [HideInInspector] public bool m_explorationIsDone = false;

    private const int MAX_WORKER = 40;
    //private const int RESSOURCE_TO_FOUND_TO_STOP_EXPLORING = 60;    //Bigger than max nb of worker


    public static Exploring_Manager _Instance
    {
        get;
        private set;
    }
    private void Awake()
    {
        if (_Instance == null || _Instance == this)
        {
            _Instance = this;
            return;
        }
        Destroy(this);
    }

    private void Update()
    {
        if (!m_explorationIsDone)
        {
            CheckIfExploratorsAreDoneExploring();
            StopExploringWhenEnoughRessourceFound();
		}
    }

    void Start()
    {

        GetMapDimensionAndZoneLength();
        SetZonePositionsForList();

        for (int i = 0; i < m_zoneLenght; i++)
        {
            for (int j = 0; j < m_zoneLenght; j++)
            {
                ListAllZones(i, j);
            }
        }
    }

    // Fonction pour cr�er les chunks
    private void GetMapDimensionAndZoneLength()
    {
        m_mapDimension = MapGenerator.MapDimension.Value;
        Debug.Log("Map dimension: " + m_mapDimension.ToString());

        m_zoneLenght = (m_mapDimension / 5) + 1;
        Debug.Log("Length of Zones: " + m_zoneLenght.ToString());
    }

    /// <summary> Cette fonction sert a placer les positions des zones dans une liste </summary>
    private void SetZonePositionsForList()
    {
        int mapDimension = m_mapDimension;

        // Round to 5
        while (mapDimension % 5 != 0)
        {
            mapDimension--;
        }

        float halfMapDimension = (float)mapDimension / 2;

        for (int i = 0; i < m_zoneLenght; i++)
        {
            float x = -halfMapDimension + (i * 5);
            m_xPositions.Add(x);

            float y = halfMapDimension - (i * 5);
            m_yPositions.Add(y);
        }
    }

    /// <summary> Cette fonction initialise les listes de positions et la liste de bool </summary>
    private void ListAllZones(int i, int j)
    {
        Vector2 zonePosition = new Vector2(m_xPositions[j], m_yPositions[i]);
        m_zonesPositions.Add(zonePosition);
        m_zonesIsDetected.Add(false);
    }

    /// <summary> Cette fonction sert a assigner des workers comme explorateur </summary>
    public void SetWorkerForExploring(Worker_Team worker)
    {

        // Si on pr�voit pas d'explorateur ou si l'exploration est consid�r�e finie
        if (m_workerInExploration > m_nbOfExploringWorkers || m_explorationIsDone)
        {
            worker.m_workerState = EWorkerState.collecting;
            return;
        }
     
        // Pour les extra explorateur
        if (m_workerInExploration > 3)
        {
            worker.m_extraExplorator = true;
        }

        // modulo pour s'assurer que les workers vont dans des directions s�parer �qualement
        int moduloForDirection = m_workerInExploration % 4;

        // donner des direction de d�part au worker
        switch (moduloForDirection)
        {
            case 0:
                worker.m_workerDirection = EDirection.left;
                break;
            case 1:
                worker.m_workerDirection = EDirection.right;
                break;
            case 2:
                worker.m_workerDirection = EDirection.up;
                break;
            case 3:
                worker.m_workerDirection = EDirection.down;
                break;
            default:
                break;
        }

        worker.m_workerState = EWorkerState.exploring;
        m_workerInExploration++;
    }

    /// <summary> Cette fonction sert a placer les zones d�tect�s </summary>
    public void SpawnDetectedZone(Vector2 position)
    {
        GameObject zone = Instantiate(m_zone, position, transform.rotation);
        zone.transform.SetParent(transform);
    }

    // calcule un temps d'arret de l'exploration
    private void EvaluateWhenStopExploring()
    {
        int minDuration = MapGenerator.SimulationDuration.GetMin(); // 10
        int maxDuration = MapGenerator.SimulationDuration.GetMax(); // 1000
        int mapDuration = MapGenerator.SimulationDuration.Value;

        // retourne le pourcentage sur le temps d'une partie en comparaison a son min et max
        m_pourcentageOfTotalTimeExploration = Mathf.InverseLerp(minDuration, maxDuration, mapDuration) * 100;

        float totalTime = MapGenerator.SimulationDuration.Value;
        float explorationTime = (totalTime / 100) * m_pourcentageOfTotalTimeExploration;

        // dire a mes explorateur de collect� apres un certain temps calculer plus haut
        Invoke("WorkersStopExploringAndSpawnCollectors", explorationTime);
    }

    // Arreter l'exploration et spawner les collecteur manquant
    private void WorkersStopExploringAndSpawnCollectors()
    {
        if (m_explorationIsDone)
        {
            return;
        }

        m_explorationIsDone = true;

        foreach (Worker_Team worker in TeamOrchestrator_Team._Instance.WorkersList)
        {
            if (worker.m_workerState == EWorkerState.exploring)
            {
                worker.m_workerState = EWorkerState.collecting;
            }
        }

		int knownCollectibleCount = Collecting_Manager._Instance.KnownCollectibles.Count;
        int workerCount = TeamOrchestrator_Team._Instance.WorkersList.Count;
		int numberOfCollectorToSpawn = knownCollectibleCount - workerCount;

		if (workerCount >= MAX_WORKER)
        {
            return;
        }
    }  

    // Fonction qui calcule le pourcentage de la map explorer
    public float GetPourcentageOfMapExpored()
    {
        float pourcentage = 100;
        int numberOfZoneExplored = 0;

        foreach (bool zone in m_zonesIsDetected)
        {
            if (zone == true)
            {
                numberOfZoneExplored++;
            }
        }

        pourcentage /= m_zonesIsDetected.Count;
        pourcentage *= numberOfZoneExplored;
        return pourcentage;
    }

    // Fonction qui regarde si mes explorateur on tous finis d'explorer en temps r�el
    private void CheckIfExploratorsAreDoneExploring()
    {

        foreach (Worker_Team worker in TeamOrchestrator_Team._Instance.WorkersList)
        {
            if (worker.m_workerState == EWorkerState.exploring)
            {
                return;
            }
        }

        WorkersStopExploringAndSpawnCollectors();
    }

	private void StopExploringWhenEnoughRessourceFound()
	{
        if (Collecting_Manager._Instance.KnownCollectibles.Count >= (int)(TeamOrchestrator_Team._Instance.WorkersList.Count * 1.5f))
		{
			WorkersStopExploringAndSpawnCollectors();
		}
	}
}
