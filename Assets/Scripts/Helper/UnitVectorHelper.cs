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

        // For a given vector, find a vector which is orthogonal to it.
        public static Vector3 RandomTangent(Vector3 vector)
        {
            return (Quaternion.FromToRotation(Vector3.forward, vector) * 
                (Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Vector3.right)).normalized;
        }

        // For a given vector, find a vector which is orthogonal to it.
        // Specifically, it must be on the surface with normal = (0, 1, 0) aka up
        public static Vector3 NormalSurfaceTangent(Vector3 vector)
        {
            if (vector.normalized == Vector3.up)
            {
                return RandomTangent(vector);
            }
            else
            {
                return Vector3.Cross(vector, Vector3.up).normalized;
            }
        }

        public static List<Vector3> GetFourSurfaceTagent(Vector3 vector)
        {
            Vector3 vector1 = NormalSurfaceTangent(vector);
            Vector3 vector2 = Vector3.Cross(vector, vector1).normalized;
            return new List<Vector3>()
            {
                vector1,
                -vector1,
                vector2,
                -vector2
            };
        }

        public static List<Vector3> GetEightSurfaceTagent(Vector3 vector)
        {
            Vector3 vector1 = NormalSurfaceTangent(vector);
            Vector3 vector2 = Vector3.Cross(vector, vector1).normalized;
            Vector3 vector3 = (vector1 + vector2).normalized;
            Vector3 vector4 = (vector1 - vector2).normalized;
            return new List<Vector3>()
            {
                vector1,
                -vector1,
                vector2,
                -vector2,
                vector3,
                -vector3,
                vector4,
                -vector4
            };
        }
    }
}
