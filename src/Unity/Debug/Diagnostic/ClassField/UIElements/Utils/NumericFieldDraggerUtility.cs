using System;
using UnityEngine;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    internal class NumericFieldDraggerUtility
    {
        private static bool s_UseYSign;

        internal static float Acceleration(bool shiftPressed, bool altPressed)
            => ((!shiftPressed) ? 1 : 4) * (altPressed ? 0.25f : 1f);

        internal static float NiceDelta(Vector2 deviceDelta, float acceleration)
        {
            deviceDelta.y = 0f - deviceDelta.y;
            if (Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > 0.1f)
            {
                if (Mathf.Abs(deviceDelta.x) > Mathf.Abs(deviceDelta.y))
                {
                    s_UseYSign = false;
                }
                else
                {
                    s_UseYSign = true;
                }
            }

            if (s_UseYSign)
            {
                return Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration;
            }

            return Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
        }

        internal static double CalculateFloatDragSensitivity(double value)
            => (double.IsInfinity(value) || double.IsNaN(value)) 
                ? 0.0
                : Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5)) * 0.029999999329447746;
        
        internal static double CalculateFloatDragSensitivity(double value, double minValue, double maxValue)
            => (double.IsInfinity(value) || double.IsNaN(value))
                ? 0.0
                : Math.Abs(maxValue - minValue) / 100.0 * 0.029999999329447746;
            
        internal static ulong CalculateIntDragSensitivity(ulong value)
            => (ulong)Math.Max(1.0, Math.Pow(Math.Abs((double)value), 0.5) * 0.029999999329447746);
    }

}