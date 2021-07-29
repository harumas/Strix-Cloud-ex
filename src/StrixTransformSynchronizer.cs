using SoftGear.Strix.Client.Core.Time;
using SoftGear.Strix.Unity.Runtime;
using UnityEngine;

namespace StrixEx {
    public class StrixTransformSynchronizer : StrixBehaviour {
        SyncTimeClient syncTimeClient;
        Transform owner;
        StrixReplicator replicator;
        float lastSyncTime;

        Quaternion nextRotation;
        Vector3 nextPosition;
        Vector3 nextRotationEuler;
        Vector3 nextDirection;

        float m_Distance;
        float m_Angle;

        Vector3 m_StoredPosition;
        Vector3 m_Direction;

        float sendMultiplier;
        int sentServerTime;
        bool isfirstTake = true;

        void Awake() {
            owner = transform;
            replicator = owner.GetComponent<StrixReplicator>();
            syncTimeClient = StrixNetwork.instance.roomSession.syncTimeClient;

            if (!isLocal) {
                Reset(owner.position, owner.rotation);
            }
        }

        void Update() {
            if (isfirstTake && isLocal) return;

            owner.localPosition = Vector3.MoveTowards(owner.position, nextPosition, m_Distance * (1f / replicator.sendRate));
            owner.localRotation = Quaternion.RotateTowards(owner.rotation, nextRotation, m_Angle * (1f / replicator.sendRate));
        }

        public void Teleport(Vector3 position, Vector3 velocity, Quaternion rotation, bool isLocal) {
            owner.position = position;
            owner.rotation = rotation;

            if (!isLocal) {
                Reset(position, rotation);
            }
        }

        public override void OnStrixSerialize(StrixSerializationProperties properties) {
            if (isfirstTake) isfirstTake = false;

            Vector3 localPosition = owner.localPosition;
            Vector3 eulerAngles = owner.localRotation.eulerAngles;

            this.m_Direction = localPosition - this.m_StoredPosition;
            this.m_StoredPosition = localPosition;

            try {
                properties.SetVector3(StrixExIndexes.Position, localPosition, "Position");
                properties.SetVector3(StrixExIndexes.Direction, m_Direction, "Direction");
                properties.SetVector3(StrixExIndexes.Rotation_Euler, eulerAngles, "Rotation");

                properties.Set(StrixExIndexes.ServerTime, syncTimeClient.SychronizedTime.Millisecond, "ServerTime");
            }
            catch (ConflictingPropertyException ex) {
                Debug.LogError((object)
                    $"The key {(object) ex.Key} for reserved property \"{(object) ex.AddedValueDescription}\" is conflicting with \"{(object) ex.ExistingValueDescription}\". MovementSynchronizer will not work properly. Consider changing the key of the conflicting property: {(object) ex.Key} (\"{(object) ex.ExistingValueDescription}\")");
            }
        }

        public override void OnStrixDeserialize(StrixSerializationProperties properties) {
            if (isfirstTake) isfirstTake = false;

            properties.Get(StrixExIndexes.ServerTime, ref sentServerTime);

            double _sentServerTime = ((double) sentServerTime) * 0.001d;

            bool flag = properties.GetVector3(StrixExIndexes.Position, ref nextPosition);

            if (properties.GetVector3(StrixExIndexes.Rotation_Euler, ref nextRotationEuler))
                flag = true;

            if (properties.GetVector3(StrixExIndexes.Direction, ref nextDirection))
                flag = true;

            //現在のサーバータイムの取得
            uint u = (uint) syncTimeClient.SychronizedTime.Millisecond;
            double t = u;
            double time = t * 0.001d;

            float lag = Mathf.Abs((float) (time - _sentServerTime));

            if (!flag) return;

            Quaternion quaternion = Quaternion.Euler(nextRotationEuler);

            if ((double) lastSyncTime == 0.0) {
                owner.localPosition = nextPosition;
                owner.localRotation = quaternion;
            }

            nextPosition += nextDirection * lag;
            m_Distance = Vector3.Distance(owner.localPosition, nextPosition);
            nextRotation = quaternion;

            m_Angle = Quaternion.Angle(owner.localRotation, nextRotation);

            lastSyncTime = Time.time;
        }


        public void Reset(Vector3 position, Quaternion rotation) {
            nextPosition = position;
            nextRotation = rotation;
            nextRotationEuler = rotation.eulerAngles;
            lastSyncTime = 0.0f;
        }
    }
}