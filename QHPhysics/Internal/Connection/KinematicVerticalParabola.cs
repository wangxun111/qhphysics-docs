using System;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public sealed class KinematicVerticalParabola : ConnectionBase {
        private static readonly Single CompensationFactor = 0.8f;
        private static readonly Single CompensationMax = 0.5f;

        public VerticalParabola verticalParabola;
        public Vector3 EndPosition;

        private Vector3 lastPosition;
        private Single duration;
        private Single startTime;
        private Single timePower;

        public Single TimeElapsed {
            get {
                return base.Mass1.Sim.InternalTime - startTime;
            }
        }

        public Single Progress {
            get {
                return Mathf.Pow((base.Mass1.Sim.InternalTime - startTime) / duration, timePower);
            }
        }

        public Single Duration {
            get {
                return duration;
            }
        }

        public KinematicVerticalParabola(MassObject mass, MassObject compensatorMass, Single duration, Single timePower, VerticalParabola parabola) : base(mass, compensatorMass) {
            this.verticalParabola = parabola;
            startTime = mass.Sim.InternalTime;
            this.duration = duration;
            this.timePower = timePower;
        }

        public KinematicVerticalParabola(Simulation sim, KinematicVerticalParabola source) : base(sim.MassDict[source.Mass1.UID], sim.MassDict[source.Mass2.UID], source.UID) {
            verticalParabola = new VerticalParabola(source.Mass1.Position, source.verticalParabola.PointEnd, source.verticalParabola.PointMiddleY);
            startTime = 0f;
            Sync(source);
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            KinematicVerticalParabola kinematicVerticalParabola = source as KinematicVerticalParabola;
            EndPosition = kinematicVerticalParabola.EndPosition;
            lastPosition = EndPosition;
            duration = kinematicVerticalParabola.duration;
            timePower = kinematicVerticalParabola.timePower;
        }

        public override void Solve() {
            if (!base.Mass1.IsKinematic) {
                return;
            }
            if (startTime == 0f) {
                startTime = base.Mass1.Sim.InternalTime;
            }
            if (base.Mass1.Sim.InternalTime > startTime + duration) {
                return;
            }
            Single progress = Progress;
            Vector3 position = base.Mass1.Position;
            Vector3 vector = base.Mass2.Position - position;
            vector.y = 0f;
            vector *= CompensationFactor * Mathf.Pow(progress, 0.2f);
            Single magnitude = vector.magnitude;
            if (magnitude > CompensationMax) {
                vector = vector.normalized * CompensationMax;
            }
            Vector3 point = verticalParabola.GetPoint(progress);
            Vector3 b = Vector3.Lerp(lastPosition, EndPosition, base.Mass1.Sim.FrameIterationProgress);
            Vector3 vector2 = position;
            Vector3 vector3 = Vector3.Lerp(point + vector, b, progress * progress * progress * progress);
            if (progress < 0.5f || position.y >= base.Mass1.GroundHeight) {
                base.Mass1.Position = vector3;
                base.Mass1.Velocity4f = (vector3 - vector2).AsPhyVector() / Simulation.TimeQuant4f;
                Single num = base.Mass1.Velocity4f.Magnitude();
                if (num > base.Mass1.CurrentVelocityLimit && base.Mass1.CurrentVelocityLimit > 0f) {
                    base.Mass1.Velocity4f *= new Vector4f(base.Mass1.CurrentVelocityLimit / num);
                }
            }
        }
    }
}