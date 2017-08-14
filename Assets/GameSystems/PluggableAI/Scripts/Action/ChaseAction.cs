﻿using UnityEngine;

namespace GameSystem.AI
{
    /// <summary>
    /// 追踪目标
    /// </summary>
    [CreateAssetMenu(menuName = "PluggableAI/Actions/Chase")]
    public class ChaseAction : Action
    {
        public override void Act(StateController controller)
        {
            if (!controller.instancePrefs.Contains("ChaseTarget"))
                return;

            controller.navMeshAgent.destination = ((Transform)controller.instancePrefs["ChaseTarget"]).position;
            controller.navMeshAgent.isStopped = false;
        }
    }
}