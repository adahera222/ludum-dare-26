using UnityEngine;
using System.Collections;

public class AttackSensor : SensorSingle<UnitBaseEntity> {
    public int hostileFlags; //based on flock id

    public float minRange = 0.0f;
    public float maxRange = 0.0f; //range to attack
    public bool angleCheck = false;
    public float angle = 0.0f;

    private float mCosTheta;

    public bool CheckRange(Vector2 curDir, Transform target) {
        Vector2 pos = transform.position;
        Vector2 tpos = target.position;
        Vector2 dir = tpos - pos;
        float dist = dir.magnitude; dir /= dist;

        return dist >= minRange
            && dist <= maxRange
            && (!angleCheck || Vector2.Dot(dir, curDir) > mCosTheta);
    }

    protected override bool UnitVerify(UnitBaseEntity unit) {
        return (hostileFlags & (1 << unit.flockId)) != 0;
    }

    void Awake() {
        mCosTheta = Mathf.Cos(angle * Mathf.Deg2Rad);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;

        if(minRange > 0.0f) {
            Gizmos.DrawWireSphere(transform.position, minRange);
        }

        if(maxRange > 0.0f) {
            Gizmos.color *= 0.75f;
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }
}
