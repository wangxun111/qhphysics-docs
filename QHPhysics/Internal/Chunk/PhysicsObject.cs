using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Mono.Simd;

namespace QH.Physics
{
    public class PhysicsObject
    {
        private static Int32 IIDCounter;
        public static Int32 NewIID {
            get { return Interlocked.Decrement(ref IIDCounter); }
        }
        
        public Int32 UID { get; private set; }
        public Int32 IID { get; private set; }
        
        public SimulationSystem Sim;
        private Boolean isKinematic;
        public MassObject CheckLayerMass;

        public EPhysicsObjectType Type { get; set; }
        public List<MassObject> Masses { get; private set; }
        public List<ConnectionBase> Connections { get; private set; }

        public Boolean IsKinematic {
            get { return isKinematic; }
            set {
                isKinematic = value;
                foreach (MassObject mass in Masses) {
                    mass.IsKinematic = value;
                    mass.StopMass();
                }
            }
        }

        public Boolean IsLying {
            get {
                foreach (MassObject mass in Masses) {
                    if (mass.IsLying) {
                        return true;
                    }
                }

                return false;
            }
        }

        public Boolean IsBlocked {
            get {
                foreach (MassObject mass in Masses) {
                    if (mass.IsBlocked) {
                        return true;
                    }
                }
                return false;
            }
        }
        
        public MassObject MassBlocked {
            get {
                foreach (MassObject mass in Masses) {
                    if (mass.IsBlocked) {
                        return mass;
                    }
                }
                return null;
            }
        }
        
        public MassObject[] MassesBlocked {
            get {
                return Masses.FindAll(m => m.IsBlocked).ToArray();
            }
        }
        
        public Boolean IsTrapped {
            get {
                foreach (MassObject mass in Masses) {
                    if (null != mass && mass.IsTrapped) {
                        return true;
                    }
                }
                return false;
            }
        }

        public MassObject MassTrapped {
            get {
                foreach (MassObject mass in Masses) {
                    if (null != mass && mass.IsTrapped) {
                        return mass;
                    }
                }
                return null;
            }
        }
        
        public MassObject[] MassesTrapped {
            get {
                return Masses.FindAll(m => m.IsTrapped).ToArray();
            }
        }

        public ECollisionType Collision {
            get { return Masses[0].Collision; }
            set {
                foreach (MassObject mass in Masses) {
                    mass.Collision = value;
                }
            }
        }

        public Vector3 VisualPositionOffset {
            get { return Masses[0].VisualPositionOffset; }
            set {
                foreach (MassObject mass in Masses) {
                    mass.VisualPositionOffset = value;
                }
            }
        }

        public Boolean IgnoreEnvForces {
            set {
                foreach (MassObject mass in Masses) {
                    mass.IgnoreEnvForces = value;
                }
            }
        }

        public Single CurrentVelocityLimit {
            get { return Masses[0].CurrentVelocityLimit; }
            set {
                foreach (MassObject mass in Masses) {
                    mass.CurrentVelocityLimit = value;
                }
            }
        }

        public PhysicsObject(EPhysicsObjectType type, SimulationSystem sim) {
            Type = type;
            Masses = new List<MassObject>();
            Connections = new List<ConnectionBase>();
            Sim = sim;
            UID = Sim.NewUID;
            IID = NewIID;
            Sim.Objects.Add(this);
            Sim.DictObjects[UID] = this;
            CheckLayerMass = null;
            Sim.IPhyActionsListener.CreateAPhysicsObject(this);
        }

        public PhysicsObject(SimulationSystem sim, PhysicsObject source) {
            Type = source.Type;
            Masses = new List<MassObject>();
            Connections = new List<ConnectionBase>();
            Sim = sim;
            for (Int32 i = 0; i < source.Masses.Count; i++) {
                Masses.Add(Sim.MassDict[source.Masses[i].UID]);
            }

            for (Int32 j = 0; j < source.Connections.Count; j++) {
                Connections.Add(Sim.DictConnections[source.Connections[j].UID]);
            }

            UID = source.UID;
            IID = -NewIID;
        }

        public override Int32 GetHashCode() {
            return IID;
        }

        public void StopMasses() {
            foreach (MassObject mass in Masses) {
                mass.StopMass();
            }
        }

        public virtual void KinematicTranslate(Vector4f offset) {
            foreach (MassObject mass in Masses) {
                mass.KinematicTranslate(offset);
            }
        }

        public void SetMotionDamping(Single damping) {
            foreach (MassObject mass in Masses) {
                mass.MotionDamping = damping;
            }
        }

        public virtual void DetectRefLeaks(List<MassObject> masses, List<ConnectionBase> connections, List<PhysicsObject> objects, StringBuilder msg) {
            for (Int32 i = 0; i < Masses.Count; i++) {
                if (!Sim.MassDict.ContainsKey(Masses[i].UID)) {
                    masses.Add(Masses[i]);
                    msg.AppendFormat($"{UID}.{Type} {GetType()}: Masses[{i}] {Masses[i].UID}.{Masses[i].Type} {Masses[i].GetType()} is not present in the system.\n");
                }
            }

            for (Int32 j = 0; j < Connections.Count; j++) {
                if (!Sim.DictConnections.ContainsKey(Connections[j].UID)) {
                    connections.Add(Connections[j]);
                    msg.AppendFormat($"{UID}.{Type} {GetType()}: Connections[{j}] {Connections[j].UID}.{Connections[j].GetType()} is not present in the system.\n");
                }
            }
        }

        public virtual void Sync(PhysicsObject source) {
            Masses.Clear();
            Connections.Clear();
            for (Int32 i = 0; i < source.Masses.Count; i++) {
                Masses.Add(Sim.MassDict[source.Masses[i].UID]);
            }
            for (Int32 j = 0; j < source.Connections.Count; j++) {
                Connections.Add(Sim.DictConnections[source.Connections[j].UID]);
            }
        }

        public virtual void UpdateReferences() { }

        public virtual void SyncMain(PhysicsObject source) { }

        public virtual void UpdateSim() {
            Sim.MassList.AddRange(Masses);
            Sim.ConnectionList.AddRange(Connections);
            Sim.RefreshObjectArrays();
        }

        public virtual void Remove() {
            for (Int32 i = 0; i < Masses.Count; i++) {
                Sim.RemoveMass(Masses[i]);
            }
            for (Int32 j = 0; j < Connections.Count; j++) {
                Sim.RemoveConnection(Connections[j]);
            }
            Masses.Clear();
            Connections.Clear();
            Sim.RemoveObject(this);
            Sim.RefreshObjectArrays();
        }

        public virtual void Simulate(Single dt) { }
    }
}