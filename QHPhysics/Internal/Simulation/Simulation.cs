using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Simd;
using SObject = System.Object;

namespace QH.Physics {
    public abstract class Simulation : ISimulation {
        public const Int32 MassesMaxAmount = 1024;
        public const Single TimeQuant = 0.0004f;

        public static readonly Vector4f TimeQuant4f = new Vector4f(TimeQuant);
        public static readonly Vector4f InvTimeQuant4f = new Vector4f(1 / TimeQuant);

        private Int32 uidCount;

        private MassObject[] paddingMasses;
        private Vector3 visualPositionOffset;
        private Dictionary<Int32, SObject> massToRemoveDict = new Dictionary<Int32, SObject>(MassesMaxAmount);

        protected MassObject[] masses;
        protected Int32 massAmount;

        public IPhysicsEngineListener IPhyActionsListener { get; set; }
        public UInt32 IterationsCount { get; set; }

        public Boolean IsMain { get; set; }
        public Dictionary<Int32, MassObject> MassDict { get; set; }

        public Int32 NewUID {
            get { return uidCount++; }
        }

        public List<MassObject> MassList { get; private set; }
        public List<ConnectionBase> ConnectionList { get; private set; }
        public Single FrameIterationProgress { get; private set; }
        public Single FrameDeltaTime { get; set; }
        public Int32 FrameIterationIndex { get; private set; }
        public Int32 IterationAmount { get; private set; }

        public Single InternalTime {
            get { return IterationsCount * TimeQuant; }
        }

        public Vector3 VisualPositionOffset {
            get { return visualPositionOffset; }
            set {
                visualPositionOffset = value;
                foreach (MassObject mass in MassList) {
                    mass.VisualPositionOffset = value;
                }
            }
        }

        protected Simulation() {
            MassList = new List<MassObject>();
            ConnectionList = new List<ConnectionBase>();
            IterationsCount = 0u;
            paddingMasses = new MassObject[3] { new MassObject(this, 0f, Vector3.zero, EMassObjectType.Padding), new MassObject(this, 0f, Vector3.zero, EMassObjectType.Padding), new MassObject(this, 0f, Vector3.zero, EMassObjectType.Padding) };
        }

        protected Simulation(IEnumerable<MassObject> masses) {
            MassList = new List<MassObject>(masses);
            paddingMasses = new MassObject[3] { new MassObject(this, 0f, Vector3.zero, EMassObjectType.Padding), new MassObject(this, 0f, Vector3.zero, EMassObjectType.Padding), new MassObject(this, 0f, Vector3.zero, EMassObjectType.Padding) };
        }

        public virtual void Clear() {
            IPhyActionsListener?.Clear();
            massAmount = 0;
            if (MassDict != null) {
                MassDict.Clear();
            }

            MassList.Clear();
        }

        protected virtual void Reset() {
            for (Int32 i = 0; i < massAmount; i++) {
                masses[i].Reset();
            }
        }

        protected abstract void Solve();

        protected virtual void Simulate(Single deltaTime) {
            for (Int32 i = 0; i < massAmount; i++) {
                masses[i].Simulate();
            }
        }

        protected virtual void Operate(Single deltaTime) {
            Reset();
            Solve();
            Simulate(deltaTime);
            IterationsCount++;
        }

        public virtual void Update(Single frameTime) {
            IterationAmount = (Int32)(frameTime / TimeQuant) + 1;
            FrameDeltaTime = frameTime;

            Single deltaTime = frameTime;
            if (IterationAmount != 0) {
                deltaTime = frameTime / IterationAmount;
            }

            for (Int32 i = 0; i < IterationAmount; i++) {
                FrameIterationIndex = i;
                FrameIterationProgress = (Single)(i + 1) / IterationAmount;
                Operate(deltaTime);
            }
        }

        private void RefreshMasses() {
            if (masses == null) {
                masses = new MassObject[MassesMaxAmount];
            }

            if (massAmount > MassList.Count) {
                for (Int32 i = MassList.Count; i < massAmount; i++) {
                    masses[i] = null;
                }
            }

            massAmount = MassList.Count;
            Int32 num = 0;
            for (Int32 j = 0; j < MassList.Count; j++) {
                if (!MassList[j].DisableSimulation && (MassList[j].SourceMass == null || !MassList[j].SourceMass.DisableSimulation)) {
                    masses[num] = MassList[j];
                    num++;
                }
            }

            massAmount = num;

            for (Int32 k = 0; k < (4 - massAmount % 4) % 4; k++) {
                masses[massAmount + k] = paddingMasses[k];
            }
        }

        public void ModifyObjectArrays(IList<MassObject> massListToCreate, IList<MassObject> massListToDestroy) {
            RefreshMasses();
            ModifyMassesDict(massListToCreate, massListToDestroy);
        }

        public void ModifyMassesDict(IList<MassObject> massListToCreate, IList<MassObject> massListToDestroy) {
            if (MassDict == null) {
                MassDict = new Dictionary<Int32, MassObject>();
            }

            for (Int32 i = 0; i < massListToDestroy.Count; i++) {
                MassDict.Remove(massListToDestroy[i].UID);
            }

            for (Int32 j = 0; j < massListToCreate.Count; j++) {
                MassObject mass = massListToCreate[j];
                MassDict[mass.UID] = mass;
            }
        }

        public virtual void RefreshObjectArrays(Boolean refreshDict = true) {
            RefreshMasses();
            if (MassDict == null) {
                MassDict = new Dictionary<Int32, MassObject>();
            }

            if (!refreshDict) {
                return;
            }

            massToRemoveDict.Clear();
            foreach (Int32 key in MassDict.Keys) {
                massToRemoveDict.Add(key, null);
            }

            for (Int32 i = 0; i < massAmount; i++) {
                if (!MassDict.ContainsKey(masses[i].UID)) {
                    MassDict[masses[i].UID] = masses[i];
                }
                else {
                    massToRemoveDict.Remove(masses[i].UID);
                }
            }

            foreach (Int32 key2 in massToRemoveDict.Keys) {
                MassDict.Remove(key2);
            }
        }

        public void RemoveMass(MassObject mass) {
            if (null == mass) {
                return;
            }
            MassList.Remove(mass);
            IPhyActionsListener.DestroyAMass(mass);
        }

        public virtual Single CalculateSystemPotentialEnergy() {
            Single num = 0f;
            for (Int32 i = 0; i < ConnectionList.Count; i++) {
                num += ConnectionList[i].CalculatePotentialEnergy();
            }

            return num;
        }
    }
}