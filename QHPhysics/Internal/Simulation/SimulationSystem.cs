using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;
using SObject = System.Object;

namespace QH.Physics {
    public abstract class SimulationSystem : Simulation {
        public const Int32 ConnectionsMaxCount = 1024;
        public const Int32 VerletConstraintsMaxCount = 1024;
        public const Int32 ObjectsMaxCount = 128;
        public const Int32 MaxVerletConstraintIterations = 1;
        public const Single GravityAcceleration = 9.81f;
        public const Single WaterDragConstant = 10f;
        public const Single WaterLevelEpsillon = 0.0125f;
        public const Single BuoyancySpeedMultiplierFactor = 1.3f;
        public const Single WaterSpeedLaminar = 1f;
        public const Single MaxDynamicBuoyancy = 1000f;
        public const Single WaterFlowVelocityFactor = 0.8f;
        public const Single AirDragConstant = 0.001f;
        public const Single GroundRepulsionConstant = 2f;
        public const Single GroundDragConstant = 0.2f;
        public const Single GroundAbsorptionConstant = 0.995f;
        public const Single GroundFrictionDelta = 0.035f;

        public static readonly Vector4f GravityAcceleration4f = new Vector4f(GravityAcceleration);
        public static readonly Vector4f WaterDragConstant4f = new Vector4f(WaterDragConstant);
        public static readonly Vector4f WaterLevelEpsillon4f = new Vector4f(WaterLevelEpsillon);
        public static readonly Vector4f WaterSpeedLaminar4f = new Vector4f(WaterSpeedLaminar);
        public static readonly Vector4f MaxDynamicBuoyancy4f = new Vector4f(MaxDynamicBuoyancy);
        public static readonly Vector4f AirDragConstant4f = new Vector4f(AirDragConstant);
        public static readonly Vector4f GroundDragConstant4f = new Vector4f(GroundDragConstant);
        public static readonly Vector4f GroundAbsorptionConstant4f = new Vector4f(GroundAbsorptionConstant);

        public static Int32 replayCounter = 0;

        protected ConnectionBase[] ArrayConnections;
        protected Int32 ArrayConnectionsLength;

        protected VerletConstraint[] ArrayVerletConstraints;
        protected Int32 ArrayVerletConstraintsLength;

        protected IImpulseConstraint[] ArrayImpulseBasedSprings;
        protected Int32 ArrayImpulseBasedSpringsLength;

        public Dictionary<Int32, ConnectionBase> DictConnections;

        protected String name;
        private Int32 verletCycle;
        private MassObject[] tmpMasses = new MassObject[4];

        private Int32[] tracedMasses;
        public Vector3[][] Traces;
        private Int32 traceScanPeriod;
        private Int32 skipCounter;

        private Vector4f[] tmpVelocities = new Vector4f[4];
        private Dictionary<Int32, SObject> removeKeyDict = new Dictionary<Int32, SObject>(1024);
        public List<PhysicsObject> Objects { get; private set; }
        public Dictionary<Int32, PhysicsObject> DictObjects { get; private set; }

        protected SimulationSystem(String name) {
            this.name = name;
            Objects = new List<PhysicsObject>();
            DictObjects = new Dictionary<Int32, PhysicsObject>();
        }

        protected SimulationSystem(String name, IEnumerable<MassObject> masses, IEnumerable<ConnectionBase> springs) : base(masses) {
            this.name = name;
            ConnectionList.AddRange(springs);
        }

        public override void Clear() {
            base.Clear();
            ConnectionList.Clear();
            if (DictConnections != null) {
                DictConnections.Clear();
            }

            ArrayConnectionsLength = 0;
            ArrayImpulseBasedSpringsLength = 0;
            ArrayVerletConstraintsLength = 0;
            Objects.Clear();
            if (DictObjects != null) {
                DictObjects.Clear();
            }
        }

        protected override void Solve() {
            ApplyForcesToMasses();
            for (Int32 i = 0; i < ArrayConnectionsLength; i++) {
                ArrayConnections[i].Solve();
            }
        }

        protected override void Operate(Single dt) {
            base.Operate(dt);
            for (Int32 i = 0; i < Objects.Count; i++) {
                Objects[i].Simulate(dt);
            }

            SatisfyVerletConstraints();
            verletCycle = (verletCycle + 1) % 3;
        }

        protected void SatisfyVerletConstraints() {
            for (Int32 j = 0; j < ArrayVerletConstraintsLength; j++) {
                if (j % 3 == verletCycle) {
                    ArrayVerletConstraints[j].Satisfy();
                }
            }
        }

        public virtual void ApplyForcesToMasses() {
            for (Int32 i = 0; i < massAmount; i += 4) {
                tmpMasses[0] = masses[i];
                tmpMasses[1] = masses[i + 1];
                tmpMasses[2] = masses[i + 2];
                tmpMasses[3] = masses[i + 3];
                Vector4f weightTmpMasses = new Vector4f(tmpMasses[0].Weight, tmpMasses[1].Weight, tmpMasses[2].Weight, tmpMasses[3].Weight);
                if ((IterationsCount + tmpMasses[0].UID) % 3 == 0) {
                    Vector4f posY = new Vector4f(tmpMasses[0].Position4f.Y, tmpMasses[1].Position4f.Y, tmpMasses[2].Position4f.Y, tmpMasses[3].Position4f.Y);
                    Vector4f posYVisualOffset = new Vector4f(tmpMasses[0].VisualPositionOffset.y, tmpMasses[1].VisualPositionOffset.y, tmpMasses[2].VisualPositionOffset.y, tmpMasses[3].VisualPositionOffset.y);
                    Vector4f compoundWaterResistance = new Vector4f(tmpMasses[0].CompoundWaterResistance, tmpMasses[1].CompoundWaterResistance, tmpMasses[2].CompoundWaterResistance, tmpMasses[3].CompoundWaterResistance);
                    Vector4f buoyancy = new Vector4f(tmpMasses[0].Buoyancy, tmpMasses[1].Buoyancy, tmpMasses[2].Buoyancy, tmpMasses[3].Buoyancy);
                    Vector4f scaledBuoyancySpeedMultiplier = new Vector4f(tmpMasses[0].ScaledBuoyancySpeedMultiplier, tmpMasses[1].ScaledBuoyancySpeedMultiplier, tmpMasses[2].ScaledBuoyancySpeedMultiplier, tmpMasses[3].ScaledBuoyancySpeedMultiplier);
                    Vector4f airDragConstant = new Vector4f(tmpMasses[0].AirDragConstant, tmpMasses[1].AirDragConstant, tmpMasses[2].AirDragConstant, tmpMasses[3].AirDragConstant);
                    Vector4f isLying = new Vector4f((tmpMasses[0].IsLying ? 1 : 0), (tmpMasses[1].IsLying ? 1 : 0), (tmpMasses[2].IsLying ? 1 : 0), (tmpMasses[3].IsLying ? 1 : 0));

                    Vector4f posYReal = posY + posYVisualOffset;
                    posYReal = posYReal.Min(WaterLevelEpsillon4f);
                    posYReal = posYReal.Max(WaterLevelEpsillon4f.Negative());
                    posYReal = (Vector4fExtensions.one - posYReal / WaterLevelEpsillon4f) * Vector4fExtensions.half;

                    Vector4f isPosYLessEqualWaterLevelEpsillon = posY.CompareLessEqual(WaterLevelEpsillon4f);
                    isPosYLessEqualWaterLevelEpsillon &= Vector4fExtensions.one;

                    buoyancy += Vector4fExtensions.one;

                    Vector4f fWeight = -1 * weightTmpMasses;
                    Vector4f speed0 = Vector4f.Zero;
                    Vector4f speed1 = Vector4f.Zero;
                    for (Int32 j = 0; j < 4; j++) {
                        if (!tmpMasses[j].IgnoreEnvForces) {
                            tmpVelocities[j] = tmpMasses[j].Velocity4f - tmpMasses[j].FlowVelocity;
                            tmpVelocities[j].W = 0f;
                            speed0[j] = Math.Max((Single)Math.Sqrt(tmpVelocities[j].SqrMagnitude()), WaterSpeedLaminar4f[j]);
                            Vector4f speed = tmpVelocities[j].DuplicateLow();
                            speed *= speed;
                            speed = speed.Shuffle(ShuffleSel.RotateRight);
                            speed1[j] = (Single)Math.Sqrt(speed.HorizontalAdd(speed).X);
                            compoundWaterResistance[j] *= (speed0[j] * isPosYLessEqualWaterLevelEpsillon[j]);
                            fWeight[j] += weightTmpMasses[j] * (buoyancy[j] + scaledBuoyancySpeedMultiplier[j]) * posYReal[j];
                            scaledBuoyancySpeedMultiplier[j] *= speed1[j];
                            scaledBuoyancySpeedMultiplier[j] = Math.Clamp(scaledBuoyancySpeedMultiplier[j], -1f * MaxDynamicBuoyancy4f[j], MaxDynamicBuoyancy4f[j]);
                        }
                        else {
                            compoundWaterResistance[j] = 0;
                        }
                    }

                    //speed0 = speed0.Sqrt().Max(WaterSpeedLaminar4f);
                    //speed1 = speed1.Sqrt();
                    //compoundWaterResistance *= speed0 * isPosYLessEqualWaterLevelEpsillon;

                    //scaledBuoyancySpeedMultiplier = scaledBuoyancySpeedMultiplier.Min(MaxDynamicBuoyancy4f).Max(MaxDynamicBuoyancy4f.Negative());
                    airDragConstant *= Vector4fExtensions.one - posYReal;

                    for (Int32 k = 0; k < 4; k++) {
                        if (tmpMasses[k] is VerletMass) {
                            tmpMasses[k].perPeriodForcesCache4f = new Vector4f(0f - compoundWaterResistance[k]) * tmpVelocities[k];
                        }
                        else {
                            tmpMasses[k].perPeriodForcesCache4f = Vector4f.Zero;
                        }
                        tmpMasses[k].perPeriodForcesCache4f = tmpMasses[k].perPeriodForcesCache4f + new Vector4f(0f, fWeight[k], 0f, 0f) - (tmpMasses[k].Velocity4f - tmpMasses[k].WindVelocity) * new Vector4f(airDragConstant[k]);
                    }
                }

                for (Int32 l = 0; l < 4; l++) {
                    Vector4f force = tmpMasses[l].perPeriodForcesCache4f + tmpMasses[l].Motor4f;
                    if (0.0125f >= tmpMasses[l].Position.y) {
                        force += tmpMasses[l].WaterMotor4f;
                    }

                    tmpMasses[l].ApplyForce(force);
                }
            }
        }

        public virtual void ApplyForcesToMass(MassObject mass) {
            if (mass.IsKinematic) {
                return;
            }

            if (base.FrameIterationIndex == 0) {
                mass.perFrameForcesCache4f = Vector4fExtensions.down * mass.MassValue4f * GravityAcceleration4f;
            }

            Boolean flag = base.FrameIterationIndex % 3 == 0;
            if (flag) {
                mass.perPeriodForcesCache4f = Vector4f.Zero;
            }

            Vector4f perFrameForcesCache4f = mass.perFrameForcesCache4f;
            if (!mass.IgnoreEnvForces) {
                if (mass.Position.y <= mass.WaterHeight + 0.0125f) {
                    Vector4f vector4f = mass.Velocity4f - mass.FlowVelocity;
                    vector4f.W = 0f;
                    Single num = 0.5f * (1f - Mathf.Clamp(mass.Position.y, -0.0125f, 0.0125f) / 0.0125f);
                    Single num2 = vector4f.Magnitude();
                    if (num2 < 1f) {
                        num2 = 1f;
                    }

                    perFrameForcesCache4f -= vector4f * mass.MassValue4f * (mass.WaterDragConstant4f + new Vector4f(mass.BuoyancySpeedMultiplier * GravityAcceleration)) * new Vector4f(num2);
                    if (flag) {
                        Single num3 = (mass.Buoyancy + 1f) * num;
                        Single num4 = 0f;
                        if (mass.BuoyancySpeedMultiplier != 0f && mass.Position.y < 0f) {
                            num4 += new Vector2(mass.Velocity4f.X - mass.FlowVelocity.X, mass.Velocity4f.Z - mass.FlowVelocity.Z).magnitude * mass.BuoyancySpeedMultiplier * BuoyancySpeedMultiplierFactor * num;
                        }

                        num4 = Mathf.Clamp(num4, -1000f, 1000f);
                        mass.perPeriodForcesCache4f += Vector4fExtensions.up * (mass.MassValue4f * GravityAcceleration4f * new Vector4f(num3 + num4));
                    }
                }

                if (mass.Position.y >= 0f && flag) {
                    mass.perPeriodForcesCache4f -= (mass.Velocity4f - mass.WindVelocity) * mass.AirDragConstant4f;
                }

                perFrameForcesCache4f += mass.perPeriodForcesCache4f;
            }

            perFrameForcesCache4f += mass.Motor4f;
            if (mass.Position.y <= mass.WaterHeight + 0.0125f) {
                perFrameForcesCache4f += mass.WaterMotor4f;
            }

            mass.ApplyForce(perFrameForcesCache4f);
        }

        private void RefreshConnectionsAndObjectsArrays() {
            if (ArrayConnections == null) {
                ArrayConnections = new ConnectionBase[ConnectionsMaxCount];
            }

            if (ArrayVerletConstraints == null) {
                ArrayVerletConstraints = new VerletConstraint[VerletConstraintsMaxCount];
            }

            if (ArrayImpulseBasedSprings == null) {
                ArrayImpulseBasedSprings = new IImpulseConstraint[1024];
            }

            if (ArrayConnectionsLength > base.ConnectionList.Count) {
                for (Int32 i = base.ConnectionList.Count; i < ArrayConnectionsLength; i++) {
                    ArrayConnections[i] = null;
                }
            }

            ArrayConnectionsLength = base.ConnectionList.Count;
            Int32 arrayVerletConstraintsLength = ArrayVerletConstraintsLength;
            ArrayVerletConstraintsLength = 0;
            Int32 arrayImpulseBasedSpringsLength = ArrayImpulseBasedSpringsLength;
            ArrayImpulseBasedSpringsLength = 0;
            Int32 count = 0;
            for (Int32 j = 0; j < base.ConnectionList.Count; j++) {
                if (!base.ConnectionList[j].Mass1.DisableSimulation && (base.ConnectionList[j].Mass1.SourceMass == null || !base.ConnectionList[j].Mass1.SourceMass.DisableSimulation) && !base.ConnectionList[j].Mass2.DisableSimulation && (base.ConnectionList[j].Mass2.SourceMass == null || !base.ConnectionList[j].Mass2.SourceMass.DisableSimulation)) {
                    ArrayConnections[count] = base.ConnectionList[j];
                    count++;
                    if (base.ConnectionList[j] is VerletConstraint) {
                        ArrayVerletConstraints[ArrayVerletConstraintsLength] = base.ConnectionList[j] as VerletConstraint;
                        ArrayVerletConstraintsLength++;
                    }

                    if (base.ConnectionList[j] is IImpulseConstraint) {
                        ArrayImpulseBasedSprings[ArrayImpulseBasedSpringsLength] = base.ConnectionList[j] as IImpulseConstraint;
                        ArrayImpulseBasedSpringsLength++;
                    }
                }
            }

            ArrayConnectionsLength = count;
            if (arrayVerletConstraintsLength > ArrayVerletConstraintsLength) {
                for (Int32 k = ArrayVerletConstraintsLength; k < arrayVerletConstraintsLength; k++) {
                    ArrayVerletConstraints[k] = null;
                }
            }

            if (arrayImpulseBasedSpringsLength > ArrayImpulseBasedSpringsLength) {
                for (Int32 l = ArrayImpulseBasedSpringsLength; l < arrayImpulseBasedSpringsLength; l++) {
                    ArrayImpulseBasedSprings[l] = null;
                }
            }
        }

        public void ModifyObjectArrays(IList<MassObject> createdMasses, IList<MassObject> destroyedMasses, IList<ConnectionBase> createdConnections, IList<ConnectionBase> destroyedConnections, IList<PhysicsObject> createdObjects, IList<PhysicsObject> destroyedObjects) {
            ModifyObjectArrays(createdMasses, destroyedMasses);
            RefreshConnectionsAndObjectsArrays();
            ModifyConnectionsDict(createdConnections, destroyedConnections);
            ModifyObjectsDict(createdObjects, destroyedObjects);
        }

        public void ModifyConnectionsDict(IList<ConnectionBase> createdConnections, IList<ConnectionBase> destroyedConnections) {
            if (DictConnections == null) {
                DictConnections = new Dictionary<Int32, ConnectionBase>();
            }

            for (Int32 i = 0; i < destroyedConnections.Count; i++) {
                DictConnections.Remove(destroyedConnections[i].UID);
            }

            for (Int32 j = 0; j < createdConnections.Count; j++) {
                ConnectionBase connectionBase = createdConnections[j];
                DictConnections[connectionBase.UID] = connectionBase;
            }
        }

        public void ModifyObjectsDict(IList<PhysicsObject> createdObjects, IList<PhysicsObject> destroyedObjects) {
            for (Int32 i = 0; i < destroyedObjects.Count; i++) {
                DictObjects.Remove(destroyedObjects[i].UID);
            }

            for (Int32 j = 0; j < createdObjects.Count; j++) {
                PhysicsObject phyObject = createdObjects[j];
                DictObjects[phyObject.UID] = phyObject;
            }
        }

        public override void RefreshObjectArrays(Boolean updateDict = true) {
            base.RefreshObjectArrays(updateDict);
            RefreshConnectionsAndObjectsArrays();
            if (DictConnections == null) {
                DictConnections = new Dictionary<Int32, ConnectionBase>();
            }

            if (!updateDict) {
                return;
            }

            removeKeyDict.Clear();
            foreach (Int32 key in DictConnections.Keys) {
                removeKeyDict.Add(key, null);
            }

            for (Int32 i = 0; i < base.ConnectionList.Count; i++) {
                if (!DictConnections.ContainsKey(base.ConnectionList[i].UID)) {
                    DictConnections[base.ConnectionList[i].UID] = base.ConnectionList[i];
                }
                else {
                    removeKeyDict.Remove(base.ConnectionList[i].UID);
                }
            }

            foreach (Int32 key2 in removeKeyDict.Keys) {
                DictConnections.Remove(key2);
            }

            removeKeyDict.Clear();
            foreach (Int32 key3 in DictObjects.Keys) {
                removeKeyDict.Add(key3, null);
            }

            for (Int32 j = 0; j < Objects.Count; j++) {
                if (!DictObjects.ContainsKey(Objects[j].UID)) {
                    DictObjects[Objects[j].UID] = Objects[j];
                }
                else {
                    removeKeyDict.Remove(Objects[j].UID);
                }
            }

            foreach (Int32 key4 in removeKeyDict.Keys) {
                DictObjects.Remove(key4);
            }
        }

        public void RemoveConnection(ConnectionBase connection) {
            base.ConnectionList.Remove(connection);
            IPhyActionsListener.DestroyAConnection(connection);
        }

        public void RemoveObject(PhysicsObject obj) {
            Objects.Remove(obj);
            DictObjects.Remove(obj.UID);
            IPhyActionsListener.DestroyAPhysicsObject(obj);
        }

        public override Single CalculateSystemPotentialEnergy() {
            Single num = 0f;
            for (Int32 i = 0; i < base.MassList.Count; i++) {
                num += base.MassList[i].MassValue * GravityAcceleration * base.MassList[i].Position4f.Y;
            }

            return base.CalculateSystemPotentialEnergy() + num;
        }

#region Debug

        private void ShowListDiffMass(IEnumerable<MassObject> mass1, IEnumerable<MassObject> mass2, String set1, String set2) {
            HashSet<MassObject> hashSet = new HashSet<MassObject>(mass1);
            HashSet<MassObject> hashSet2 = new HashSet<MassObject>(mass2);
            StringBuilder stringBuilder = new StringBuilder("showListDiffMass:");
            foreach (MassObject item in mass1) {
                if (item != null && !hashSet2.Contains(item)) {
                    stringBuilder.AppendFormat($"\n{item.Type} {item.UID}:{item.IID} is present in {set1} but is absent from {set2}.");
                }
            }

            foreach (MassObject item2 in mass2) {
                if (item2 != null && !hashSet.Contains(item2)) {
                    stringBuilder.AppendFormat($"\n{item2.Type} {item2.UID}:{item2.IID} is present in {set1} but is absent from {set2}.");
                }
            }

            Debug.LogError(stringBuilder.ToString());
        }

        private void ShowListDiffConnections(IEnumerable<ConnectionBase> c1, IEnumerable<ConnectionBase> c2, String set1, String set2) {
            HashSet<ConnectionBase> hashSet = new HashSet<ConnectionBase>(c1);
            HashSet<ConnectionBase> hashSet2 = new HashSet<ConnectionBase>(c2);
            StringBuilder stringBuilder = new StringBuilder("showListDiffConnections:");
            foreach (ConnectionBase item in c1) {
                if (item != null && !hashSet2.Contains(item)) {
                    stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {3} but is absent from {4}.", item.GetType(), item.UID, item.IID, set1, set2);
                }
            }

            foreach (ConnectionBase item2 in c2) {
                if (item2 != null && !hashSet.Contains(item2)) {
                    stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {4} but is absent from {3}.", item2.GetType(), item2.UID, item2.IID, set1, set2);
                }
            }

            Debug.LogError(stringBuilder.ToString());
        }

        private void ShowListDiffObjects(IEnumerable<PhysicsObject> c1, IEnumerable<PhysicsObject> c2, String set1, String set2) {
            HashSet<PhysicsObject> hashSet = new HashSet<PhysicsObject>(c1);
            HashSet<PhysicsObject> hashSet2 = new HashSet<PhysicsObject>(c2);
            StringBuilder stringBuilder = new StringBuilder("showListDiffObjects:");
            foreach (PhysicsObject item in c1) {
                if (item != null && !hashSet2.Contains(item)) {
                    stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {3} but is absent from {4}.", item.GetType(), item.UID, item.IID, set1, set2);
                }
            }

            foreach (PhysicsObject item2 in c2) {
                if (item2 != null && !hashSet.Contains(item2)) {
                    stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {4} but is absent from {3}.", item2.GetType(), item2.UID, item2.IID, set1, set2);
                }
            }

            Debug.LogError(stringBuilder.ToString());
        }

        public void DetectRefLeaks() {
            Boolean flag = false;
            if (massAmount != base.MassList.Count || massAmount != MassDict.Count) {
                Debug.LogError("DetectRefLeaks FAIL: masses containers do not match: ArrayMassesLength = " + massAmount + " DictMasses.Count = " + MassDict.Count + " Masses.Count = " + base.MassList.Count);
                flag = true;
                if (massAmount != base.MassList.Count) {
                    ShowListDiffMass(masses, base.MassList, "ArrayMasses", "Masses");
                }

                if (massAmount != MassDict.Count) {
                    ShowListDiffMass(masses, MassDict.Values, "ArrayMasses", "DictMasses");
                }
            }

            if (ArrayConnectionsLength != base.ConnectionList.Count || ArrayConnectionsLength != DictConnections.Count) {
                Debug.LogError("DetectRefLeaks FAIL: connections containers do not match: ArrayConnectionsLength = " + ArrayConnectionsLength + " DictConnections.Count = " + DictConnections.Count + " Connections.Count = " + base.ConnectionList.Count);
                flag = true;
                if (ArrayConnectionsLength != base.ConnectionList.Count) {
                    ShowListDiffConnections(ArrayConnections, base.ConnectionList, "ArrayConnections", "Connections");
                }

                if (ArrayConnectionsLength != DictConnections.Count) {
                    ShowListDiffConnections(ArrayConnections, DictConnections.Values, "ArrayConnections", "DictConnections");
                }
            }

            if (DictObjects.Count != Objects.Count) {
                Debug.LogError("DetectRefLeaks FAIL: objects containers do not match: Objects.Count = " + Objects.Count + " DictObjects.Count = " + DictObjects.Count);
                flag = true;
                ShowListDiffObjects(DictObjects.Values, Objects, "DictObjects", "Objects");
            }

            for (Int32 i = 0; i < massAmount; i++) {
                if (!MassDict.ContainsKey(masses[i].UID)) {
                    Debug.LogError("DetectRefLeaks FAIL: Mass_" + masses[i].UID + " does not present in the dict");
                    flag = true;
                }
            }

            for (Int32 j = 0; j < ArrayConnectionsLength; j++) {
                if (!DictConnections.ContainsKey(ArrayConnections[j].UID)) {
                    Debug.LogError("DetectRefLeaks FAIL: Connection_" + ArrayConnections[j].UID + " does not present in the dict");
                    flag = true;
                }
            }

            foreach (PhysicsObject @SObject in Objects) {
                if (!DictObjects.ContainsKey(@SObject.UID)) {
                    Debug.LogError("DetectRefLeaks FAIL: PhyObject_" + @SObject.UID + " does not present in the dict");
                    flag = true;
                }
            }

            for (Int32 k = 0; k < base.MassList.Count; k++) {
                if (base.MassList[k].Sim != this) {
                    Debug.LogError(String.Concat("DetectRefLeaks FAIL: [Masses] Mass ", base.MassList[k].Type, " ", base.MassList[k].UID, " has invalid Sim reference"));
                    flag = true;
                }
            }

            for (Int32 l = 0; l < massAmount; l++) {
                if (masses[l].Sim != this) {
                    Debug.LogError(String.Concat("DetectRefLeaks FAIL: [ArrayMasses] Mass ", masses[l].Type, " ", masses[l].UID, " has invalid Sim reference"));
                    flag = true;
                }
            }

            foreach (MassObject value in MassDict.Values) {
                if (value.Sim != this) {
                    Debug.LogError(String.Concat("DetectRefLeaks FAIL: [DictMasses] Mass ", value.Type, " ", value.UID, " has invalid Sim reference"));
                    flag = true;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            List<MassObject> list = new List<MassObject>();
            List<ConnectionBase> list2 = new List<ConnectionBase>();
            List<PhysicsObject> list3 = new List<PhysicsObject>();
            for (Int32 m = 0; m < base.MassList.Count; m++) {
                base.MassList[m].DetectRefLeaks(list, list2, list3, stringBuilder);
            }

            for (Int32 n = 0; n < base.ConnectionList.Count; n++) {
                base.ConnectionList[n].DetectRefLeaks(list, list2, list3, stringBuilder);
            }

            for (Int32 num = 0; num < Objects.Count; num++) {
                Objects[num].DetectRefLeaks(list, list2, list3, stringBuilder);
            }

            if (stringBuilder.Length > 0) {
                Debug.LogError(stringBuilder);
            }

            if (list.Count > 0) {
                StringBuilder stringBuilder2 = new StringBuilder("DetectRefLeaks FAIL: Following referenced masses do not present in the system:\n");
                foreach (MassObject item in list) {
                    stringBuilder2.AppendLine(String.Concat(item.Type, " ", item.UID));
                }

                Debug.LogError(stringBuilder2);
                flag = true;
            }

            if (list2.Count > 0) {
                StringBuilder stringBuilder3 = new StringBuilder("DetectRefLeaks FAIL: Following referenced connections do not present in the system:\n");
                foreach (ConnectionBase item2 in list2) {
                    stringBuilder3.AppendLine(item2.GetType().ToString() + " " + item2.UID);
                }

                Debug.LogError(stringBuilder3);
                flag = true;
            }

            if (list3.Count <= 0) {
                return;
            }

            StringBuilder stringBuilder4 = new StringBuilder("DetectRefLeaks FAIL: Following referenced objects do not present in the system:\n");
            foreach (PhysicsObject item3 in list3) {
                stringBuilder4.AppendLine(String.Concat(item3.Type, " ", item3.UID));
            }

            Debug.LogError(stringBuilder4);
            flag = true;
        }

        public Single MassToMassSpringDistance(MassObject lowerMass, MassObject upperMass, Single load = 0f) {
            Single num = 0f;
            MassObject mass = lowerMass;
            while (mass.NextSpring != null && mass != upperMass) {
                num = ((load != 0f) ? (num + mass.NextSpring.EquilibrantLength(load * GravityAcceleration)) : (num + mass.NextSpring.SpringLength));
                mass = mass.NextSpring.Mass2;
            }

            return (mass != upperMass) ? (-1f) : num;
        }

        public void EnableMassTracer(Int32 traceScanPeriod, Int32[] tracedMasses) {
            this.traceScanPeriod = traceScanPeriod;
            skipCounter = 0;
            this.tracedMasses = tracedMasses;
            Traces = new Vector3[tracedMasses.Length][];
            for (Int32 i = 0; i < Traces.Length; i++) {
                Traces[i] = new Vector3[150 / traceScanPeriod + 1];
            }
        }

        public void SyncMassTracer(SimulationSystem sourceSim) {
            if (tracedMasses != null && sourceSim.Traces != null) {
                for (Int32 i = 0; i < Traces.Length; i++) {
                    Array.Copy(sourceSim.Traces[i], Traces[i], Traces[i].Length);
                }
            }
        }

        public void DebugSnapshotLog() {
            StringBuilder stringBuilder = new StringBuilder("----- Masses -----\n");
            for (Int32 i = 0; i < base.MassList.Count; i++) {
                stringBuilder.AppendLine(base.MassList[i].ToString());
            }

            stringBuilder.AppendLine();
            Debug.Log(stringBuilder.ToString());
            stringBuilder = new StringBuilder("----- Connections -----\n");
            for (Int32 j = 0; j < base.ConnectionList.Count; j++) {
                stringBuilder.AppendLine(base.ConnectionList[j].ToString());
            }

            Debug.Log(stringBuilder.ToString());
        }

#endregion
    }
}