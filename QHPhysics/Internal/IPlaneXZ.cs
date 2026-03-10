using System;

namespace QH.Physics
{
    public interface IPlaneXZ
    {
        Int32 ID { get; }
        Boolean TestAABB(Single minX, Single maxX, Single minZ, Single maxZ);
        Boolean ProcreateFactor(Single minX, Single maxX, Single minZ, Single maxZ);
        Single DistanceToPoint(Single px, Single pz);
    }
}