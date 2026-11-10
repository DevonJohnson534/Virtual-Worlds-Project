using UnityEngine;

namespace VirtualWorlds.FSM
{
    public enum StalkerEvent
    {
        StalkerDetectPlayer,
        LostPlayerWhileStalking,
        ReachedPlayer,
        InAttackRange,
        LostPlayerWhileChasing,
        AttackSuccess,
        AttackRangeLeft
    }
}