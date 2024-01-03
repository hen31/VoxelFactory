using Stride.Core;
using Stride.Core.Annotations;

namespace TurtleGames.VoxelEngine;

[DataContract("Spline point")]
public class SplinePoint
{
    [DataMember]
    [DataMemberRange(-1f, 1f, 0.05f, 0.1f, 2)]
    public float Point { get; set; }

    [DataMember] public ushort Value { get; set; }
}