using System;
using System.Linq;
using UnityEngine;
using Mono.Simd.Math;

namespace QH.Physics {
    public class VerletObject : PhysicsObject {
        public VerletObject(EPhysicsObjectType type, SimulationSystem sim) : base(type, sim) { }
        public VerletObject(SimulationSystem sim, VerletObject source) : base(sim, source) { }

        protected VerletMass AddMass(Single mass, Vector3 position, EMassObjectType type) {
            VerletMass verletMass = new VerletMass(Sim, mass, position, type);
            verletMass.Sim = Sim;
            verletMass.Radius = 0.05f;
            verletMass.Buoyancy = 0f;
            verletMass.StopMass();
            verletMass.IgnoreEnvironment = true;
            verletMass.Collision = ECollisionType.FullBody;
            verletMass.StaticFrictionFactor = 0.01f;
            verletMass.SlidingFrictionFactor = 0.01f;
            verletMass.SetStopMass(false);
            base.Masses.Add(verletMass);
            return verletMass;
        }

        protected VerletSpring AddSpring(VerletMass m1, VerletMass m2, Single friction, Single customLength = -1f, bool compressible = false) {
            Single length = ((!(customLength > 0f)) ? (m1.Position4f - m2.Position4f).Magnitude() : customLength);
            VerletSpring verletSpring = new VerletSpring(m1, m2, length, friction, compressible);
            base.Connections.Add(verletSpring);
            return verletSpring;
        }

        protected int AddTetrahedron(Single mass, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Single customEdgeLength = -1f) {
            int count = base.Masses.Count;
            Single mass2 = mass * 0.25f;
            VerletMass m = AddMass(mass2, a, EMassObjectType.Fish);
            VerletMass verletMass = AddMass(mass2, b, EMassObjectType.Fish);
            VerletMass verletMass2 = AddMass(mass2, c, EMassObjectType.Fish);
            VerletMass verletMass3 = AddMass(mass2, d, EMassObjectType.Fish);
            AddSpring(m, verletMass, 0.1f, customEdgeLength);
            AddSpring(m, verletMass2, 0.1f, customEdgeLength);
            AddSpring(m, verletMass3, 0.1f, customEdgeLength);
            AddSpring(verletMass, verletMass2, 0.1f, customEdgeLength);
            AddSpring(verletMass2, verletMass3, 0.1f, customEdgeLength);
            AddSpring(verletMass3, verletMass, 0.1f, customEdgeLength);
            return count;
        }

        protected TetrahedronWithBall AddBallJoint(Int32 t1, Int32 t2, Single stiffness, Single friction) {
            TetrahedronWithBall tetrahedronBallJoint = new TetrahedronWithBall(
                new MassObject[4] { base.Masses[t1], base.Masses[t1 + 1], base.Masses[t1 + 2], base.Masses[t1 + 3] }.Cast<VerletMass>().ToArray(),
                new MassObject[4] { base.Masses[t2], base.Masses[t2 + 1], base.Masses[t2 + 2], base.Masses[t2 + 3] }.Cast<VerletMass>().ToArray(),
                stiffness,
                friction);
            base.Connections.Add(tetrahedronBallJoint);
            return tetrahedronBallJoint;
        }
    }
}