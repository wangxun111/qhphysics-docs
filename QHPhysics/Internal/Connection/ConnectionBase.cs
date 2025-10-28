using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace QH.Physics {
    public abstract class ConnectionBase {
        private static Int32 IIDCounter;
        public static Int32 NewIID {
            get {
                return Interlocked.Decrement(ref IIDCounter);
            }
        }

        protected MassObject mass1;
        protected MassObject mass2;

        public Single CurrentForce;
        public Single DeltaLength;

        public Int32 UID {
            get; protected set;
        }
        public Int32 IID {
            get; private set;
        }

        public MassObject Mass1 {
            get {
                return mass1;
            }
            set {
                mass1 = value;
                ConnectionNeedSyncMark();
            }
        }

        public MassObject Mass2 {
            get {
                return mass2;
            }
            set {
                mass2 = value;
                ConnectionNeedSyncMark();
            }
        }

        public Single MaxForce {
            get; set;
        }

        protected ConnectionBase(MassObject mass1, MassObject mass2, Int32 uid = -1) {
            UID = ((uid >= 0) ? uid : mass1.Sim.NewUID);
            if (uid < 0) {
                IID = NewIID;
                mass1.Sim.IPhyActionsListener.CreateAConnection(this);
            }
            else {
                IID = -NewIID;
            }
            SetMasses(mass1, mass2);
#if ENABLE_EASTMALLI_DEBUG
            if (!mass1.Sim.IsMain) {
                QHFramework.MessengerManager.Instance.Broadcast<QH.Physics.ConnectionBase>("CreatePhyConnection", this);
            }
#endif
        }

        public abstract void Solve();
        public virtual void UpdateReferences() {}
        public virtual void SyncSimUpdate() {}

        public virtual void DetectRefLeaks(List<MassObject> massList, List<ConnectionBase> connectionList, List<PhysicsObject> phyObjList, StringBuilder sbMsg) {
            if (Mass1 != null && !Mass1.Sim.MassDict.ContainsKey(Mass1.UID)) {
                sbMsg.AppendFormat($"{UID}.{GetType()}: [{Mass1.UID}.{Mass1.Type} {Mass1.GetType()} --- {Mass2.UID}.{Mass2.Type} {Mass2.GetType()}] Mass1 is not present in the system.\n");
                massList.Add(Mass1);
            }
            if (Mass2 != null && !Mass2.Sim.MassDict.ContainsKey(Mass2.UID)) {
                sbMsg.AppendFormat($"{UID}.{GetType()}: [{Mass1.UID}.{Mass1.Type} {Mass1.GetType()} --- {Mass2.UID}.{Mass2.Type} {Mass2.GetType()}] Mass2 is not present in the system.\n");
                massList.Add(Mass2);
            }
            if (Mass1 != null && Mass2 != null && Mass1.Sim != Mass2.Sim) {
                sbMsg.AppendFormat($"{UID}.{GetType()}: [{Mass1.UID}.{Mass1.Type} {Mass1.GetType()} --- {Mass2.UID}.{Mass2.Type} {Mass2.GetType()}] Mass1 and Mass2 do not belong to a single system.\n");
                massList.Add(Mass1);
                massList.Add(Mass2);
            }
        }

        public virtual void Sync(ConnectionBase source) {
            if (Mass1.UID != source.Mass1.UID) {
                mass1 = mass1.Sim.MassDict[source.Mass1.UID];
            }
            if (Mass2.UID != source.Mass2.UID) {
                mass2 = mass1.Sim.MassDict[source.Mass2.UID];
            }
        }

        public void ConnectionNeedSyncMark() {
            Mass1.Sim.IPhyActionsListener?.ChangeConnectionNeedSyncMark(UID);
        }

        public virtual void SetMasses(MassObject mass1, MassObject mass2 = null) {
            if (null != mass1) {
                this.mass1 = mass1;
            }
            if (null != mass2) {
                this.mass2 = mass2;
            }
            ConnectionNeedSyncMark();
        }

        public virtual Single CalculatePotentialEnergy() {
            return 0f;
        }
    }
}