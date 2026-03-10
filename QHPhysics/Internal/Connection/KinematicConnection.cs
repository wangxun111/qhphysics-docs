using System;
using UnityEngine;

namespace QH.Physics {
    public sealed class KinematicConnection : ConnectionBase {
        public Single InertiaFactor;
        public Boolean IsPassive;

        private Vector3 nextPosition;
        private Vector3 priorPosition;
        private Quaternion nextRotation;
        private Quaternion priorRotation;
        private BezierSpline bezierSpline;
        private Vector3 newPosition;
        private Vector3 priorDeltaPeriodPoint;
        private Quaternion priorDeltaPeriodRotation;
        private Vector3 finalVelocity;
        private Vector3 startVelocity;

        public Vector3 CurrentPositionDelta { get; private set; }
        public Quaternion CurrentRotationDelta { get; private set; }
        public Vector3 AccumulatedPositionDelta { get; private set; }
        public Quaternion AccumulatedRotationDelta { get; private set; }
        public Quaternion CurrentRotation { get; private set; }

        public KinematicConnection(MassObject mass, Boolean isPassive = false) : base(mass, mass) {
            bezierSpline = new BezierSpline();
            nextPosition = mass.Position;
            priorPosition = mass.Position;
            for (Int32 i = 0; i <= bezierSpline.Order; i++) {
                bezierSpline.Points[i] = nextPosition;
            }
            InertiaFactor = 0.1f;
            CurrentRotationDelta = Quaternion.identity;
            AccumulatedRotationDelta = Quaternion.identity;
            priorRotation = mass.Rotation;
            nextRotation = mass.Rotation;
            priorDeltaPeriodPoint = mass.Position;
            priorDeltaPeriodRotation = mass.Rotation;
            IsPassive = isPassive;
        }

        public KinematicConnection(Simulation sim, KinematicConnection source) : base(sim.MassDict[source.Mass1.UID], sim.MassDict[source.Mass1.UID], source.UID) {
            bezierSpline = new BezierSpline();
            Sync(source);
            CurrentRotationDelta = Quaternion.identity;
            AccumulatedRotationDelta = Quaternion.identity;
            priorDeltaPeriodPoint = nextPosition;
            priorDeltaPeriodRotation = nextRotation;
        }

        public override void Sync(ConnectionBase sourceConnection) {
            base.Sync(sourceConnection);
            KinematicConnection kinematicSpline = sourceConnection as KinematicConnection;
            nextPosition = kinematicSpline.nextPosition;
            priorPosition = mass1.Position;
            startVelocity = kinematicSpline.startVelocity;
            finalVelocity = kinematicSpline.finalVelocity;
            for (Int32 i = 0; i <= bezierSpline.Order; i++) {
                bezierSpline.Points[i] = kinematicSpline.bezierSpline.Points[i];
            }
            InertiaFactor = kinematicSpline.InertiaFactor;
            IsPassive = kinematicSpline.IsPassive;
            priorRotation = kinematicSpline.priorRotation;
            nextRotation = kinematicSpline.nextRotation;
        }

        public override void Solve() {
            if (base.Mass1.IsKinematic) {
                Single num = mass1.Sim.IterationAmount * Simulation.TimeQuant;
                Single frameProgress = mass1.Sim.FrameIterationProgress;
                Vector3 position = base.Mass1.Position;
                Quaternion currentRotation = CurrentRotation;
                newPosition = Vector3.Lerp(priorPosition, nextPosition, frameProgress);
                CurrentRotation = Quaternion.Slerp(priorRotation, nextRotation, frameProgress);
                if (!IsPassive) {
                    base.Mass1.Position = newPosition;
                    base.Mass1.Rotation = CurrentRotation;
                    base.Mass1.Velocity = (base.Mass1.Position - position) / Simulation.TimeQuant;
                }
                CurrentPositionDelta = newPosition - position;
                CurrentRotationDelta = CurrentRotation * Quaternion.Inverse(currentRotation);
                if (base.Mass1.Sim.FrameIterationIndex % 3 == 0) {
                    AccumulatedPositionDelta = newPosition - priorDeltaPeriodPoint;
                    AccumulatedRotationDelta = CurrentRotation * Quaternion.Inverse(priorDeltaPeriodRotation);
                    priorDeltaPeriodPoint = newPosition;
                    priorDeltaPeriodRotation = CurrentRotation;
                }
            }
        }

        public void Stop() {
            priorPosition = nextPosition;
            priorRotation = nextRotation;
            startVelocity = Vector3.zero;
            finalVelocity = Vector3.zero;
            priorDeltaPeriodPoint = nextPosition;
            priorDeltaPeriodRotation = nextRotation;
            CurrentPositionDelta = Vector3.zero;
            Mass1.Sim.IPhyActionsListener?.ChangeConnectionNeedSyncMark(base.UID);
        }

        public void SetNextPositionAndRotation(Vector3 position, Quaternion rotation) {
            nextPosition = position;
            bezierSpline.SetT(1f);
            Vector3 vector = bezierSpline.Derivative();
            bezierSpline.SetT(0f);
            bezierSpline.Points[0] = mass1.Position;
            bezierSpline.Points[1] = (mass1.Position + nextPosition) * 0.5f;
            bezierSpline.Points[2] = nextPosition;
            priorPosition = mass1.Position;
            priorRotation = nextRotation;
            nextRotation = rotation;
            startVelocity = mass1.Velocity;
            finalVelocity = 2f * (nextPosition - priorPosition) / Time.deltaTime - startVelocity;
            Mass1.Sim.IPhyActionsListener?.ChangeConnectionNeedSyncMark(base.UID);
        }

        public void PostponedSolve() {
            if (IsPassive) {
                base.Mass1.Velocity = (newPosition - base.Mass1.Position) / Simulation.TimeQuant;
                base.Mass1.Position = newPosition;
                base.Mass1.Rotation = CurrentRotation;
            }
        }
    }
}