/*
 Based on the work of Paul Bourke: http://www.paulbourke.net/miscellaneous/interpolation/
 */
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping { 
[ExecuteAlways]
public class Interpolation_Methods_Visualization : MonoBehaviour
{
    #region Variables
    public Vector3 offset = Vector3.forward * 3;
                                                                            [Header("Make up datapoints to interpolate")]
    public List<float> dataPoints = new List<float>() { 0.3f, 0.86f, 0.1f, 0f, 1f, 0.3f };
                                                                            [Header("Select the interpolation method type")]
    public InterpolationMethod interpolationMethod;
                                                                            [Header("Hermite Interpolation Variable")]
    public float bias = 0, tension = 0;
                                                                            [Header("For Custom Interpolation")]
    public AnimationCurve animationCurve;
                                                                            [Header("Change the distance between the datapoints")]
    public float distance_between_points = 0.5f;
                                                                            [Header("Change the interpolation step count")]
    public int resolution = 100;
    public enum InterpolationMethod
    {
        NearestNeighbour,
        Linear,
        Cosine,
        Cubic,
        Hermite,
        CustomHermite,
        CustomCurve
    }
#endregion
    #region InterpolationMethods
    public float NearestNeighbour_Interpolation(float A, float B, float t)
    {
        return (t > .5f) ? B : A;
    }
    public float Linear_Interpolation(float A, float B, float t)
    {
        return A * (1f - t) + B * t;
    }
    public float Cosine_Interpolation(float A, float B, float t)
    {
        t = (1f - Mathf.Cos(t * Mathf.PI)) * .5f;
        return A * (1f - t) + B * t;
    }
    public float Cubic_Interpolation(float P0, float P1, float P2, float P3, float t)
    {
        float w0, w1, w2, w3, t_sqrt;
        t_sqrt = t * t;
        w0 = P3 - P2 - P0 + P1;
        w1 = P0 - P1 - w0;
        w2 = P2 - P0;
        w3 = P1;
        return (w0 * t * t_sqrt + w1 * t_sqrt + w2 * t + w3);
    }
    public float Hermite_Interpolation(float P0, float P1, float P2, float P3, float t)
    {
        float a0, a1, t_sqrt, t_cubic;
        float w0, w1, w2, w3;
        t_sqrt = t * t;
        t_cubic = t_sqrt * t;
        a0 = (P1 - P0) * (1 + bias) * (1 - tension) / 2;
        a0 += (P2 - P1) * (1 - bias) * (1 - tension) / 2;
        a1 = (P2 - P1) * (1 + bias) * (1 - tension) / 2;
        a1 += (P3 - P2) * (1 - bias) * (1 - tension) / 2;
        w0 = 2 * t_cubic - 3 * t_sqrt + 1;
        w1 = t_cubic - 2 * t_sqrt + t;
        w2 = t_cubic - t_sqrt;
        w3 = -2 * t_cubic + 3 * t_sqrt;
        return (w0 * P1 + w1 * a0 + w2 * a1 + w3 * P2);
    }

    /// <summary>
    /// Exaggerates difference in similar values, but retains precision for large deltas.Exaggerates difference in similar values, but retains precision for large deltas.
    /// </summary>
    public float CustomDeltaTensionHermite_Interpolation(float P0, float P1, float P2, float P3, float t)
    {
        float a0, a1, t_sqrt, t_cubic;
        float w0, w1, w2, w3;
        t_sqrt = t * t;
        t_cubic = t_sqrt * t;
        var tensionA = Mathf.Abs(P1 - P0) * 2 - 1;
        var tensionB = Mathf.Abs(P2 - P1) * 2 - 1;
        var tensionC = Mathf.Abs(P3 - P2) * 2 - 1;
        tensionA += tensionB; tensionA *= .5f;
        tensionC += tensionB; tensionC *= .5f;
        var tension = tensionA * Mathf.Clamp01(0.5f - t) + tensionB * (0.5f - Mathf.Abs(t - 0.5f)) + tensionC * Mathf.Clamp01(t - 0.5f);
        a0 = (P1 - P0) * (1 - tension) / 2;
        a0 += (P2 - P1) * (1 - tension) / 2;
        a1 = (P2 - P1) * (1 - tension) / 2;
        a1 += (P3 - P2) * (1 - tension) / 2;
        w0 = 2 * t_cubic - 3 * t_sqrt + 1;
        w1 = t_cubic - 2 * t_sqrt + t;
        w2 = t_cubic - t_sqrt;
        w3 = -2 * t_cubic + 3 * t_sqrt;
        return (w0 * P1 + w1 * a0 + w2 * a1 + w3 * P2);
    }

    public float CustomCurve_Interpolation(float A, float B, float t)
    {
        t = animationCurve.Evaluate(t);
        return A * (1f - t) + B * t;
    }
    #endregion
    #region Visuals
    private void OnDrawGizmos()
    {

        //Distances in meters

        while (dataPoints.Count < 4)
        {
            Debug.Log("Must at least have four data points to support all algorithms. Adding a random one.");
            dataPoints.Add(Random.value);
        }

        for (float i = 0; i <= 1.1; i += 0.1f)
        {
            Gizmos.DrawLine(transform.position + Vector3.up * i+ offset, transform.position + Vector3.up * i + Vector3.right * (dataPoints.Count - 3) * distance_between_points + offset);
        }

        for (int i = 1; i < dataPoints.Count - 3; i++)
        {
            Gizmos.DrawWireSphere((i - 1) * Vector3.right * distance_between_points + transform.position + Vector3.up * dataPoints[i] + offset, 0.1f);

            for (float o = 1; o <= resolution; o++)
            {
                float t = o / resolution;
                float d = (o - 1) / resolution;

                switch (interpolationMethod)
                {
                    case InterpolationMethod.NearestNeighbour:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, NearestNeighbour_Interpolation(              dataPoints[i], dataPoints[i + 1], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, NearestNeighbour_Interpolation(              dataPoints[i], dataPoints[i + 1], t), 0) + offset);
                        break;
                    case InterpolationMethod.Linear:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, Linear_Interpolation(                                dataPoints[i], dataPoints[i + 1], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, Linear_Interpolation(                                dataPoints[i], dataPoints[i + 1], t), 0) + offset);
                        break;
                    case InterpolationMethod.Cosine:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, Cosine_Interpolation(                                dataPoints[i], dataPoints[i + 1], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, Cosine_Interpolation(                                dataPoints[i], dataPoints[i + 1], t), 0) + offset);
                        break;
                    case InterpolationMethod.Cubic:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, Cubic_Interpolation(                                 dataPoints[i - 1], dataPoints[i], dataPoints[i + 1], dataPoints[i + 2], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, Cubic_Interpolation(                                 dataPoints[i - 1], dataPoints[i], dataPoints[i + 1], dataPoints[i + 2], t), 0) + offset);
                        break;
                    case InterpolationMethod.Hermite:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, Hermite_Interpolation(                               dataPoints[i - 1], dataPoints[i], dataPoints[i + 1], dataPoints[i + 2], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, Hermite_Interpolation(                               dataPoints[i - 1], dataPoints[i], dataPoints[i + 1], dataPoints[i + 2], t), 0) + offset);
                        break;
                    case InterpolationMethod.CustomHermite:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, CustomDeltaTensionHermite_Interpolation( dataPoints[i - 1], dataPoints[i], dataPoints[i + 1], dataPoints[i + 2], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, CustomDeltaTensionHermite_Interpolation( dataPoints[i - 1], dataPoints[i], dataPoints[i + 1], dataPoints[i + 2], t), 0) + offset);
                        break;
                    case InterpolationMethod.CustomCurve:
                        Gizmos.DrawLine(
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * d, CustomCurve_Interpolation(                       dataPoints[i], dataPoints[i + 1], d), 0) + offset,
                     (i - 1) * Vector3.right * distance_between_points + transform.position + new Vector3(distance_between_points * t, CustomCurve_Interpolation(                       dataPoints[i], dataPoints[i + 1], t), 0) + offset);
                        break;
                }

            }
        }
    }
    #endregion
 }
}