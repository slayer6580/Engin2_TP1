using MBT;
using UnityEngine;

[MBTNode("Fran�ois State Enter/Enter Endphase State")]
[AddComponentMenu("")]

public class EnterEndphaseState : Leaf
{
    public GameObjectReference m_workerGO = new GameObjectReference();

    public override NodeResult Execute()
    {
        m_workerGO.Value.GetComponent<Worker_Team>().m_workerState = EWorkerState.endPhase;
        return NodeResult.success;

    }
}
