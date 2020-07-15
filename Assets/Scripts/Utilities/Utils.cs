using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    /// <summary>
    /// Clamps an angle 
    /// </summary>
    /// <param name="angle">The angle</param>
    /// <param name="min">Maximum angle</param>
    /// <param name="max">Minimum angle</param>
    /// <returns>An angle between -360 and 360 degrees, clamped</returns>
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
