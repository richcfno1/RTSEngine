using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Helper
{
    public class UnitVectorHelper
    {
        public static float DetermineUnitDistance(GameObject from, GameObject to, bool ignoreFromSize = true, bool ignoreToSize = true)
        {
            if (from.GetComponent<Collider>() == null || to.GetComponent<Collider>() == null)
            {
                return (to.transform.position - from.transform.position).magnitude;
            }
            Vector3 fromPoint = ignoreFromSize ? from.transform.position : from.GetComponent<Collider>().ClosestPoint(to.transform.position);
            Vector3 toPoint = ignoreToSize ? to.transform.position : to.GetComponent<Collider>().ClosestPoint(from.transform.position);
            return (toPoint - fromPoint).magnitude;
        }
        public static Vector3 RandomTangent(Vector3 vector)
        {
            return (Quaternion.FromToRotation(Vector3.forward, vector) * 
                (Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Vector3.right)).normalized;
        }
    }
}
