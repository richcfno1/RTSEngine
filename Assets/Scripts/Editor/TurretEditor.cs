using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RTS.Game.RTSGameObject.Subsystem;

namespace RTS.RTSEditor
{
    // Rotation code is written by another author: https://github.com/brihernandez/GunTurrets
    [CustomEditor(typeof(TurretBaseScript))]
    [CanEditMultipleObjects]
    public class TurretEditor : Editor
    {
        private const float ArcSize = 10.0f;

        public override void OnInspectorGUI()
        {
            TurretBaseScript turret = (TurretBaseScript)target;

            DrawDefaultInspector();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            TurretBaseScript turret = (TurretBaseScript)target;
            Transform transform = turret.transform;

            // Don't show turret arcs when playing, because they won't be correct.
            if (turret.showArcs && !Application.isPlaying)
            {
                if (turret.turretBarrels != null)
                {
                    // Traverse
                    Handles.color = new Color(1.0f, 0.5f, 0.5f, 0.1f);
                    if (turret.limitTraverse)
                    {
                        Handles.DrawSolidArc(turret.turretBarrels.position, turret.turretBarrels.up, turret.turretBarrels.forward, turret.rightTraverse, ArcSize);
                        Handles.DrawSolidArc(turret.turretBarrels.position, turret.turretBarrels.up, turret.turretBarrels.forward, -turret.leftTraverse, ArcSize);
                    }
                    else
                    {
                        Handles.DrawSolidArc(turret.turretBarrels.position, turret.turretBarrels.up, turret.turretBarrels.forward, 360.0f, ArcSize);
                    }

                    // Elevation
                    Handles.color = new Color(0.5f, 1.0f, 0.5f, 0.1f);
                    Handles.DrawSolidArc(turret.turretBarrels.position, turret.turretBarrels.right, turret.turretBarrels.forward, -turret.elevation, ArcSize);

                    // Depression
                    Handles.color = new Color(0.5f, 0.5f, 1.0f, 0.1f);
                    Handles.DrawSolidArc(turret.turretBarrels.position, turret.turretBarrels.right, turret.turretBarrels.forward, turret.depression, ArcSize);
                }
                else
                {
                    Handles.color = new Color(1.0f, 0.5f, 0.5f, 0.1f);
                    Handles.DrawSolidArc(transform.position, transform.up, transform.forward, turret.leftTraverse, ArcSize);
                    Handles.DrawSolidArc(transform.position, transform.up, transform.forward, -turret.leftTraverse, ArcSize);

                    Handles.color = new Color(0.5f, 1.0f, 0.5f, 0.1f);
                    Handles.DrawSolidArc(transform.position, transform.right, transform.forward, -turret.elevation, ArcSize);

                    Handles.color = new Color(0.5f, 0.5f, 1.0f, 0.1f);
                    Handles.DrawSolidArc(transform.position, transform.right, transform.forward, turret.depression, ArcSize);
                }
            }
        }
    }
}