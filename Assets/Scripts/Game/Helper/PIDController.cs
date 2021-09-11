using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RTS.Game.Helper
{
    class PIDController
    {
        public float pFactor, iFactor, dFactor;

        private Vector3 integral = new Vector3();
        private Vector3 lastError = new Vector3();

        public PIDController()
        {
            pFactor = 10;
            iFactor = 0.5f;
            dFactor = 0.5f;
        }

        public PIDController(float pFactor, float iFactor, float dFactor)
        {
            this.pFactor = pFactor;
            this.iFactor = iFactor;
            this.dFactor = dFactor;
        }

        public Vector3 Update(Vector3 currentError, float timeFrame)
        {
            // Compute the area under the error curve
            integral += currentError * timeFrame;
            // Compute the amount of change of the error value
            var deriv = (currentError - lastError) / timeFrame;
            lastError = currentError;
            // Compute and return the feedback based on error amount and change
            return currentError * pFactor + integral * iFactor + deriv * dFactor;
        }
    }
}
