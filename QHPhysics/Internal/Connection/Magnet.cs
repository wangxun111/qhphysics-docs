using System;
using UnityEngine;

namespace QH.Physics {
    public sealed class Magnet : ConnectionBase {
        public Single RepulsionConstant;
        public Single RepulsionDistance;
        public Single AttractionConstant;
        public Single AttractionDistance;

        private Boolean isAttracting;

        public Boolean IsAttracting {
            get {
                return isAttracting;
            }
            set {
                isAttracting = value;
                ConnectionNeedSyncMark();
            }
        }

        public Magnet(MassObject mass1, MassObject mass2, Single repulsionConstant, Single repulsionDistance, Single attractionConstant, Single attractionDistance) : base(mass1, mass2) {
            RepulsionConstant = repulsionConstant;
            RepulsionDistance = repulsionDistance;
            AttractionConstant = attractionConstant;
            AttractionDistance = attractionDistance;
        }

        public Magnet(Simulation sim, Magnet source) : base(sim.MassDict[source.Mass1.UID], sim.MassDict[source.Mass2.UID], source.UID) {
            RepulsionConstant = source.RepulsionConstant;
            RepulsionDistance = source.RepulsionDistance;
            AttractionConstant = source.AttractionConstant;
            AttractionDistance = source.AttractionDistance;
            Sync(source);
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            Magnet magnet = source as Magnet;
            isAttracting = magnet.IsAttracting;
        }

        public override void Solve() {
            if (isAttracting) {
                Vector3 vector = base.Mass1.Position - base.Mass2.Position;
                Single magnitude = vector.magnitude;
                if (!(magnitude > AttractionDistance)) {
                    Single num = (AttractionDistance - magnitude) / AttractionDistance;
                    Vector3 vector2 = ((!(num < 0.9f)) ? Vector3.zero : (vector.normalized * AttractionConstant * num));
                    base.Mass2.ApplyForce(vector2);
                    base.Mass1.ApplyForce(-vector2);
                }

                return;
            }

            Vector3 vector3 = base.Mass2.Position - base.Mass1.Position;
            Single magnitude2 = vector3.magnitude;
            if (!(magnitude2 > RepulsionDistance)) {
                Vector3 vector2 = vector3.normalized * RepulsionConstant * (RepulsionDistance - magnitude2);
                base.Mass2.ApplyForce(vector2);
                base.Mass1.ApplyForce(-vector2);
            }
        }
    }
}