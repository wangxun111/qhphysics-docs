using System;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public class VerletSpring : VerletConstraint {
        public Single length;
        private Single lengthsqr;
        public Vector4f friction;
        public Vector4f newFriction=Vector4f.One;
        public Boolean compressible;
        private VerletMass vmass1;
        private VerletMass vmass2;
        private Single invMassSum;
        
        

        public Boolean IsRepulsive {
            get { return !compressible; }
            set { compressible = !value; }
        }

        public VerletSpring(VerletMass mass1, VerletMass mass2, Single length, Single friction, Boolean compressible = false) : base(mass1, mass2) {
            base.UID = mass1.Sim.NewUID;
            base.Mass1 = mass1;
            base.Mass2 = mass2;
            vmass1 = mass1;
            vmass2 = mass2;
            this.length = length;
            this.compressible = compressible;
            this.friction = new Vector4f(friction);
            lengthsqr = length * length;
            invMassSum = 1f / (vmass1.InvMassValue + vmass2.InvMassValue);
        }

        public VerletSpring(Simulation sim, VerletSpring source) : base(sim.MassDict[source.Mass1.UID] as VerletMass, sim.MassDict[source.Mass2.UID] as VerletMass, source.UID) {
            Sync(source);
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            VerletSpring verletSpring = source as VerletSpring;
            vmass1 = base.Mass1 as VerletMass;
            vmass2 = base.Mass2 as VerletMass;
            length = verletSpring.length;
            compressible = verletSpring.compressible;
            friction = verletSpring.friction;
            lengthsqr = length * length;
            invMassSum = 1f / (vmass1.InvMassValue + vmass2.InvMassValue);
        }

        public override void SetMasses(MassObject mass1, MassObject mass2 = null) {
            if (mass1 != null) {
                base.Mass1 = mass1;
                vmass1 = mass1 as VerletMass;
            }

            if (mass2 != null) {
                base.Mass2 = mass2;
                vmass2 = mass2 as VerletMass;
            }

            ConnectionNeedSyncMark();
            invMassSum = 1f / (vmass1.InvMassValue + vmass2.InvMassValue);
        }

        public override void Satisfy() {
            bool isCloseSatisfy = vmass1.FishData!=null&&!vmass1.FishData.isOpenVerletSatisfy || vmass2.FishData!=null&&!vmass2.FishData.isOpenVerletSatisfy;
            
            if (isCloseSatisfy) return;
            
            Vector4f vector4f = vmass2.Position4f - vmass1.Position4f;
            Single num = Vector4fExtensions.Dot(vector4f, vector4f);
            
            float length=vmass1.FishData!=null?vmass1.FishData.verletSpringlength:vmass2.FishData!=null?vmass2.FishData.verletSpringlength:0;
            
            if (length != 0)
            {
                lengthsqr=length;
            }
            
            if (!compressible || num > lengthsqr) {
                Single num2 = (lengthsqr / (num + lengthsqr) - 0.5f) * invMassSum;
                
                if (!vmass1.IsKinematic) {
                    vmass1.Position4f -= vector4f * new Vector4f(num2 * vmass1.InvMassValue);
                }

                if (!vmass2.IsKinematic) {
                    vmass2.Position4f += vector4f * new Vector4f(num2 * vmass2.InvMassValue);
                }
            }
        }
        
        Vector4f SetNewFriction(float friction) {
            newFriction.X= friction;
            newFriction.Y= friction;
            newFriction.Z= friction;
            newFriction.W= friction;
            return newFriction;
        }
        
        public override void Solve() {
            bool isCloseVerletSpring = vmass1.FishData!=null&&!vmass1.FishData.isOpenVerletSpring || vmass2.FishData!=null&&!vmass2.FishData.isOpenVerletSpring;
            if (isCloseVerletSpring) return;
            
            Vector4f vector4f = vmass1.Position4f - vmass2.Position4f;
            Single num = vector4f.SqrMagnitude();

            friction= vmass1.FishData!=null&&vmass1.FishData.verletSpringfriction>0?SetNewFriction(vmass1.FishData.verletSpringfriction):vmass2.FishData!=null&&vmass2.FishData.verletSpringfriction>0?SetNewFriction(vmass2.FishData.verletSpringfriction):friction;

            if (!Mathf.Approximately(0f, num)) {
                vector4f /= new Vector4f(Mathf.Sqrt(num));
                Single f = Vector4fExtensions.Dot(vmass1.PositionDelta4f, vector4f) - Vector4fExtensions.Dot(vmass2.PositionDelta4f, vector4f);
                Vector4f vector4f2 = vector4f * new Vector4f(f) * friction;
                if (!vmass1.IsKinematic) {
                    vmass1.Position4f -= vector4f2;
                }

                if (!vmass2.IsKinematic) {
                    vmass2.Position4f += vector4f2;
                }
            }
        }
    }
}