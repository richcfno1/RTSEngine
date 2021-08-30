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

        public static Vector3 VectorClamp(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z));
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

        public static List<Vector3> GetFourSurfaceTagent(Vector3 vector, float distance)
        {
            Vector3 vector1 = NormalSurfaceTangent(vector);
            Vector3 vector2 = Vector3.Cross(vector, vector1).normalized;
            return new List<Vector3>()
            {
                vector1 * distance,
                -vector1 * distance,
                vector2 * distance,
                -vector2 * distance
            };
        }

        public static List<Vector3> GetEightSurfaceTagent(Vector3 vector, float distance)
        {
            Vector3 vector1 = NormalSurfaceTangent(vector);
            Vector3 vector2 = Vector3.Cross(vector, vector1).normalized;
            Vector3 vector3 = (vector1 + vector2).normalized;
            Vector3 vector4 = (vector1 - vector2).normalized;
            return new List<Vector3>()
            {
                vector1 * distance,
                -vector1 * distance,
                vector2 * distance,
                -vector2 * distance,
                vector3 * distance,
                -vector3 * distance,
                vector4 * distance,
                -vector4 * distance
            };
        }

        public static List<Vector3> GetSixAroundPoint(Vector3 center, float distance)
        {
            return new List<Vector3>()
            {
                center + Vector3.up * distance,
                center - Vector3.up * distance,
                center + Vector3.right * distance,
                center - Vector3.right * distance,
                center + Vector3.forward * distance,
                center - Vector3.forward * distance
            };
        }

        // http://paulbourke.net/geometry/circlesphere/
        public static bool CollisionBetweenLineAndSphere(Vector3 from, Vector3 to, Vector3 center, float radius)
        {
            float a = Mathf.Pow(to.x - from.x, 2) + Mathf.Pow(to.y - from.y, 2) + Mathf.Pow(to.z - from.z, 2);
            float b = 2 * ((to.x - from.x) * (from.x - center.x) + (to.y - from.y) * (from.y - center.y) + (to.z - from.z) * (from.z - center.z));
            float c = Mathf.Pow(center.x, 2) + Mathf.Pow(center.y, 2) + Mathf.Pow(center.z, 2) +
                Mathf.Pow(from.x, 2) + Mathf.Pow(from.y, 2) + Mathf.Pow(from.z, 2) -
                2 * (center.x * from.x + center.y * from.y + center.z * from.z) - radius * radius;
            return b * b - 4 * a * c >= 0;
        }

        public static bool CollisionBetwenTwoUnitPath(Vector3 unit1Position, float unit1Velocity, float unit1Radius, Vector3 unit1Destination,
            Vector3 unit2Position, Vector3 unit2Velocity, float unit2Radius)
        {
            Vector3 relativeVelocity = (unit1Destination - unit1Position).normalized * unit1Velocity - unit2Velocity;
            float time = (unit1Destination - unit1Position).magnitude / unit1Velocity;
            return CollisionBetweenLineAndSphere(unit1Position, unit1Position + relativeVelocity * time, unit2Position, unit1Radius + unit2Radius);
        }
    }
}
