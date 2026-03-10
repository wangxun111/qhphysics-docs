using System;
using UnityEngine;

namespace QH.Physics {
    public class PlaneSudokuCollider : ICollider {
        private Single[,] data;
        private Vector3 scale;
        private Vector3 dimensions;

        private Boolean isDirty;

        public Int32 SizeX {
            get {
                return data.GetLength(0);
            }
        }
        public Int32 SizeZ {
            get {
                return data.GetLength(1);
            }
        }

        public Single[,] Data {
            get {
                return data;
            }
        }

        public Vector3 Scale {
            get => scale;
        }

        public Int32 InstanceID {
            get; private set;
        }
        public EDensityType DensityType {
            get => EDensityType.None;
        }
        public Single FrictionFactor {
            get => 0;
        }
        public Vector3 Position {
            get; private set;
        }
        public Int32 IndexX {
            get; private set;
        }
        public Int32 IndexZ {
            get; private set;
        }

        public PlaneSudokuCollider(Int32 size, Vector3 position, Single horizontalScale, Single verticalScale) {
            data = new Single[size, size];
            this.Position = position;
            scale = new Vector3(horizontalScale, verticalScale, horizontalScale);
            Single min = data[0, 0];
            Single max = data[0, 0];
            for (Int32 i = 0; i < data.GetLength(0); i++) {
                for (Int32 j = 0; j < data.GetLength(1); j++) {
                    if (data[i, j] > max) {
                        max = data[i, j];
                    }
                    if (data[i, j] < min) {
                        min = data[i, j];
                    }
                }
            }

            dimensions = new Vector3(horizontalScale * (data.GetLength(0) - 1), verticalScale * max, horizontalScale * (data.GetLength(1) - 1));
            IndexX = -1;
            IndexZ = -1;
        }

        public PlaneSudokuCollider(MeshCollider sourceCollider, Single resolution) {
            Position = sourceCollider.bounds.center - sourceCollider.bounds.extents - Vector3.up * 10;
            InstanceID = sourceCollider.gameObject.GetInstanceID();
            dimensions = sourceCollider.bounds.size + Vector3.up * 10;
            Int32 num = Mathf.Max(2, Mathf.CeilToInt(dimensions.x * resolution + 1f));
            Int32 num2 = Mathf.Max(2, Mathf.CeilToInt(dimensions.z * resolution + 1f));
            data = new Single[num, num2];
            scale = new Vector3(dimensions.x / (num - 1), 1f, dimensions.z / (num2 - 1));
            Ray ray = default;
            RaycastHit hitInfo = default;
            Vector3 vector = new Vector3(Position.x, sourceCollider.bounds.center.y + sourceCollider.bounds.extents.y, Position.z);
            for (Int32 i = 0; i < num; i++) {
                for (Int32 j = 0; j < num2; j++) {
                    ray.origin = vector + new Vector3(i * scale.x, 1f, j * scale.z);
                    ray.direction = Vector3.down;
                    if (sourceCollider.Raycast(ray, out hitInfo, sourceCollider.bounds.size.y + 1f)) {
                        data[i, j] = hitInfo.point.y - Position.y + 0.02f;
                    }
                    else {
                        data[i, j] = Position.y;
                    }
                }
            }
        }

        public PlaneSudokuCollider(PlaneSudokuCollider source) {
            data = new Single[source.SizeX, source.SizeZ];
            Position = source.Position;
            scale = source.scale;
            dimensions = source.dimensions;
            IndexX = source.IndexX;
            IndexZ = source.IndexZ;
            Sync(source);
        }

        public void MarkDirty() {
            isDirty = true;
        }

        public void Move(Vector3 newPosition, Single scale, Int32 hmIndexX, Int32 hmIndexZ) {
            Position = newPosition;
            this.scale.x = scale;
            this.scale.z = scale;
            this.IndexX = hmIndexX;
            this.IndexZ = hmIndexZ;
            MarkDirty();
        }

        public void Sync(ICollider source) {
            PlaneSudokuCollider heightFieldChunk = source as PlaneSudokuCollider;
            if (heightFieldChunk == null || !heightFieldChunk.isDirty) {
                return;
            }

            Position = heightFieldChunk.Position;
            scale = heightFieldChunk.scale;
            IndexX = heightFieldChunk.IndexX;
            IndexZ = heightFieldChunk.IndexZ;
            Int32 dataSizeX = SizeX;
            Int32 dataSizeZ = SizeZ;
            Single min = heightFieldChunk.data[0, 0];
            Single max = heightFieldChunk.data[0, 0];
            for (Int32 i = 0; i < dataSizeX; i++) {
                for (Int32 j = 0; j < dataSizeZ; j++) {
                    data[i, j] = heightFieldChunk.data[i, j];
                    if (data[i, j] > max) {
                        max = data[i, j];
                    }
                    if (data[i, j] < min) {
                        min = data[i, j];
                    }
                }
            }
            dimensions = new Vector3(scale.x * (dataSizeX - 1), scale.y * max, scale.x * (dataSizeZ - 1));
            heightFieldChunk.isDirty = false;
        }

        public Vector3 GetHeightAtPoint(Vector3 point, out Vector3 normal) {
            Vector3 vector = point - Position;
            Single num = vector.x / scale.x;
            Single num2 = vector.z / scale.z;
            Int32 num3 = (Int32)num;
            Int32 num4 = (Int32)num2;
            Single num5 = num - num3;
            Single num6 = num2 - num4;
            Single x = num3 * scale.x;
            Single z = num4 * scale.z;
            Single x2 = (num3 + 1) * scale.x;
            Single z2 = (num4 + 1) * scale.z;
            if (num3 + 1 >= SizeX || num4 + 1 >= SizeZ || num3 < 0 || num4 < 0) {
                num3 = Mathf.Clamp(num3, 0, SizeX - 2);
                num4 = Mathf.Clamp(num4, 0, SizeZ - 2);
            }

            Vector3 vector2 = new Vector3(x, data[num3, num4] * scale.y, z);
            Vector3 vector3 = new Vector3(x2, data[num3 + 1, num4 + 1] * scale.y, z2);
            normal = Vector3.zero;
            if (num5 < num6) {
                Vector3 vector4 = new Vector3(x, data[num3, num4 + 1] * scale.y, z2);
                normal = Vector3.up;
                return new Vector3(point.x, Position.y + vector2.y + (vector3.y - vector4.y) * num5 + (vector4.y - vector2.y) * num6, point.z);
            }

            Vector3 vector5 = new Vector3(x2, data[num3 + 1, num4] * scale.y, z);
            normal = Vector3.up;
            return new Vector3(point.x, Position.y + vector2.y + (vector5.y - vector2.y) * num5 + (vector3.y - vector5.y) * num6, point.z);
        }

        public Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal) {
            Vector3 vector = point - Position;
            if (vector.x >= 0f && vector.x < dimensions.x && vector.z >= 0f && vector.z < dimensions.z && vector.y <= dimensions.y) {
                Single num = vector.x / scale.x;
                Single num2 = vector.z / scale.z;
                Int32 num3 = (Int32)num;
                Int32 num4 = (Int32)num2;
                Single num5 = num - num3;
                Single num6 = num2 - num4;
                Single x = num3 * scale.x;
                Single z = num4 * scale.z;
                Single x2 = (num3 + 1) * scale.x;
                Single z2 = (num4 + 1) * scale.z;
                if (num3 + 1 >= SizeX || num4 + 1 >= SizeZ || num3 < 0 || num4 < 0) {
                    num3 = Mathf.Clamp(num3, 0, SizeX - 2);
                    num4 = Mathf.Clamp(num4, 0, SizeZ - 2);
                }

                Vector3 vector2 = new Vector3(x, data[num3, num4] * scale.y, z);
                Vector3 vector3 = new Vector3(x2, data[num3 + 1, num4 + 1] * scale.y, z2);
                normal = Vector3.zero;
                if (num5 < num6) {
                    Vector3 vector4 = new Vector3(x, data[num3, num4 + 1] * scale.y, z2);
                    normal = Vector3.up;
                    Single num7 = vector2.y + (vector3.y - vector4.y) * num5 + (vector4.y - vector2.y) * num6;
                    Single num8 = num7 - vector.y;
                    if (num8 > 0f) {
                        return num8 * normal;
                    }

                    return Vector3.zero;
                }

                Vector3 vector5 = new Vector3(x2, data[num3 + 1, num4] * scale.y, z);
                normal = Vector3.up;
                Single num9 = vector2.y + (vector5.y - vector2.y) * num5 + (vector3.y - vector5.y) * num6;
                Single num10 = num9 - vector.y;
                if (num10 > 0f) {
                    return num10 * normal;
                }

                return Vector3.zero;
            }

            normal = Vector3.zero;
            return Vector3.zero;
        }

        public Vector3 TestPoint(Vector3 point, out Vector3 normal) {
            Vector3 vector = point - Position;
            if (vector.x >= 0f && vector.x < dimensions.x && vector.z >= 0f && vector.z < dimensions.z && vector.y <= dimensions.y) {
                Single num = vector.x / scale.x;
                Single num2 = vector.z / scale.z;
                Int32 num3 = (Int32)num;
                Int32 num4 = (Int32)num2;
                Single num5 = num - num3;
                Single num6 = num2 - num4;
                Single x = num3 * scale.x;
                Single z = num4 * scale.z;
                Single x2 = (num3 + 1) * scale.x;
                Single z2 = (num4 + 1) * scale.z;
                Vector3 vector2 = new Vector3(x, data[num3, num4] * scale.y, z);
                Vector3 vector3 = new Vector3(x2, data[num3 + 1, num4 + 1] * scale.y, z2);
                normal = Vector3.up;
                Vector3 v4 = new Vector3(x, data[num3, num4 + 1] * scale.y, z2);
                Vector3 v5 = new Vector3(x2, data[num3 + 1, num4] * scale.y, z);

                Single y = 0f;
                if (num3 + num4 < SizeX - 1) {
                    Vector3 mv0 = v4 - vector2;
                    Vector3 mv1 = v5 - vector2;
                    normal = Vector3.Cross(mv1, mv0).normalized;
                    if (0 != normal.y) {
                        y = (normal.x * (vector2.x - vector.x) + normal.z * (vector2.z - vector.z)) / normal.y + vector2.y;
                    }
                }
                else if (num3 + num4 == SizeX - 1) {
                    y = (v4 + (v5 - v4) * (num5 / scale.x)).y;
                }
                else {
                    Vector3 mv0 = v4 - vector3;
                    Vector3 mv1 = v5 - vector3;
                    normal = Vector3.Cross(mv0, mv1).normalized;
                    if (0 != normal.y) {
                        y = (normal.x * (vector3.x - vector.x) + normal.z * (vector3.z - vector.z)) / normal.y + vector3.y;
                    }
                }

                if (0 < y - vector.y) {
                    return Vector3.up * (y - vector.y);
                }

                return Vector3.zero;
            }

            normal = Vector3.zero;
            return Vector3.zero;
        }

        public Boolean IsNormal() {
            Single p0 = data[0, 0];
            Single p1 = data[0, SizeZ - 1];
            Single p2 = data[SizeX - 1, 0];
            Single p3 = data[SizeX - 1, SizeZ - 1];
            return (Math.Sign(p0) == Math.Sign(p1) && Math.Sign(p0) == Math.Sign(p2) && Math.Sign(p0) == Math.Sign(p3));
        }
    }
}