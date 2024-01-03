using System.Collections.Generic;
using System.Linq;
using Stride.Core;
using Stride.Core.Annotations;

namespace TurtleGames.VoxelEngine;

[DataContract("Spline")]
public class NoiseSpline
{
    [DataMember]
    [DataMemberRange(-1f, 1f, 0.05f, 0.1f, 2)]
    public float MinValue { get; set; } = -1f;

    [DataMember]
    [DataMemberRange(-1f, 1f, 0.05f, 0.1f, 2)]
    public float MaxValue { get; set; } = -1f;

    [DataMember] public List<SplinePoint> SplinePoints = new();

    public ushort GetValue(float splinePoint)
    {
        SplinePoint fromPoint = null;
        SplinePoint toPoint = null;
        var orderPoints = SplinePoints.OrderBy(b => b.Point).ToArray();
        for (int i = 0; i < orderPoints.Length; i++)
        {
            var point = orderPoints[i];
            if (point.Point > splinePoint)
            {
                if (i != 0)
                {
                    fromPoint = orderPoints[i - 1];
                }

                toPoint = orderPoints[i];

                break;
            }
        }

        if (fromPoint == null)
        {
            fromPoint = toPoint;
        }

        var ifZeroThanThisWasNextPoint = (toPoint.Point - fromPoint.Point);
        var ifZeroThanThisIsPoint = (splinePoint - fromPoint.Point);
        var positionBetweenPoints = ifZeroThanThisIsPoint / ifZeroThanThisWasNextPoint;

        var ifZeroThanThisWasNextValue = (toPoint.Value - fromPoint.Value);
        return (ushort)(fromPoint.Value + (ifZeroThanThisWasNextValue * positionBetweenPoints));
    }
}