using System;
using UnityEngine;

namespace QH.Physics {
    public interface IPhysicsEngineListener {
        void CreateAMass(MassObject mass);
        void CreateAConnection(ConnectionBase connection);
        void CreateAPhysicsObject(PhysicsObject phyObj);
        void ChangeMassValue(Int32 uid, Single massValue);
        void ChangeMassPosition(Int32 uid, Vector3 newPosition);
        void ChangeMassRotation(Int32 uid, Quaternion newRotation);
        void ChangeMassVelocity(Int32 uid, Vector3 newVelocity);
        void ChangeMassAppliedForce(Int32 uid, Vector3 force);
        void ChangeMassMotor(Int32 uid, Vector3 motor);
        void ChangeMassWaterMotor(Int32 uid, Vector3 watermotor);
        void ChangeMassIsKinematic(Int32 uid, Boolean isKinematic);
        void ChangeMassIsFreeze(Int32 uid, Boolean isKinematic);
        void ChangeMassIsRef(Int32 uid, Boolean isRef);
        void ChangeMassVisualPositionOffset(Int32 uid, Vector3 visualPositionOffset);
        void ChangeMassAirDragConstant(Int32 uid, Single airDrag);
        void ChangeMassWaterDragConstant(Int32 uid, Single waterDrag);
        void ChangeMassBuoyancy(Int32 uid, Single buoyancy);
        void ChangeMassBuoyancySpeedFactor(Int32 uid, Single factor);
        void ChangeMassVelocityLimit(Int32 uid, Single velocity);
        void ChangeMassType(Int32 uid, EMassObjectType massType);
        void ChangeMassDisableSimualtion(Int32 uid, Boolean disabled);
        void ChangeMassCollisionType(Int32 uid, ECollisionType newCollisionType);
        void ChangeMassForceFactor(Int32 uid, Single factor);
        void StopMass(Int32 uid);
        void SetStopMassState(Int32 uid,Boolean state);
        void ResetMass(Int32 uid);

        void ChangeMassLocalPosition(Int32 uid, Vector3 newLocalPosition);

        void DestroyAConnection(ConnectionBase connection);
        void DestroyAPhysicsObject(PhysicsObject phyObj);
        void Clear();
        void DestroyAMass(MassObject mass);
        void GlobalReset();
        void ChangeVelocityLimit(Single limit);

        void VisualPositionOffsetGlobalChanged(Vector3 vpo);

        void MassKinematicTranslate(Int32 uid, Vector3 translate);
        void ChangeConnectionNeedSyncMark(Int32 uid);
        void ChangePhysicsObjectNeedSyncMark(Int32 uid);
        void SetTopWaterStrikeFlg(Int32 uid);
        void SetTopWaterLiftValue(Int32 uid, Single value);
        void ChangeFishIsHooked(Int32 uid, Int32 hookState);
    }
}