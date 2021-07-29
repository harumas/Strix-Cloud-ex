using SoftGear.Strix.Client.Core.Time;
using SoftGear.Strix.Unity.Runtime;
using UnityEngine;

namespace StrixEx {
    public class StrixRigidbodySynchronizer : StrixBehaviour {
        Rigidbody owner;
        StrixReplicator replicator;
        SyncTimeClient syncTimeClient;

        Vector3 m_NetworkPosition;
        Quaternion m_NetworkRotation;

        float m_Distance;
        float m_Angle;
        int sentServerTime;

        bool isfirstTake = true;

        void Awake() {
            owner = GetComponent<Rigidbody>();
            replicator = owner.GetComponent<StrixReplicator>();
            syncTimeClient = StrixNetwork.instance.roomSession.syncTimeClient;
        }

        public void FixedUpdate() {
            if (isfirstTake && isLocal) return;

            owner.position = Vector3.MoveTowards(owner.position, m_NetworkPosition, m_Distance * (1f / replicator.sendRate));
            owner.rotation = Quaternion.RotateTowards(owner.rotation, m_NetworkRotation, m_Angle * (1f / replicator.sendRate));
        }

        public override void OnStrixSerialize(StrixSerializationProperties properties) {
            if (isfirstTake) isfirstTake = false;

            try {
                properties.SetVector3(StrixExIndexes.Position, owner.position, "Position");
                properties.SetQuaternion(StrixExIndexes.Rotation, owner.rotation, "Rotation");
                properties.SetVector3(StrixExIndexes.Velocity, owner.velocity, "Direction");
                properties.SetVector3(StrixExIndexes.AngularVelocity, owner.angularVelocity, "AngularVelocity");

                properties.Set(StrixExIndexes.ServerTime, syncTimeClient.SychronizedTime.Millisecond, "ServerTime");
            }
            catch (ConflictingPropertyException ex) {
                Debug.LogError((object)
                    $"The key {(object) ex.Key} for reserved property \"{(object) ex.AddedValueDescription}\" is conflicting with \"{(object) ex.ExistingValueDescription}\". MovementSynchronizer will not work properly. Consider changing the key of the conflicting property: {(object) ex.Key} (\"{(object) ex.ExistingValueDescription}\")");
            }
        }

        Vector3 _velocity;
        Vector3 _angularVelocity;


        public override void OnStrixDeserialize(StrixSerializationProperties properties) {
            if (isfirstTake) isfirstTake = false;

            properties.Get(StrixExIndexes.ServerTime, ref sentServerTime);
            double _sentServerTime = ((double) sentServerTime) * 0.001d;

            bool flag = properties.GetVector3(StrixExIndexes.Position, ref m_NetworkPosition);

            if (properties.GetQuaternion(StrixExIndexes.Rotation, ref m_NetworkRotation))
                flag = true;

            if (properties.GetVector3(StrixExIndexes.Velocity, ref _velocity))
                flag = true;

            if (properties.GetVector3(StrixExIndexes.AngularVelocity, ref _angularVelocity))
                flag = true;

            if (!flag) return;

            //現在のサーバータイムの取得
            uint u = (uint) syncTimeClient.SychronizedTime.Millisecond;
            double t = u;
            double time = t * 0.001d;

            float lag = Mathf.Abs((float) (time - _sentServerTime));

            owner.velocity = _velocity;
            m_NetworkPosition += owner.velocity * lag;
            m_Distance = Vector3.Distance(owner.position, m_NetworkPosition);

            owner.angularVelocity = _angularVelocity;
            m_NetworkRotation = Quaternion.Euler(owner.angularVelocity * lag) * m_NetworkRotation;
            m_Angle = Quaternion.Angle(owner.rotation, m_NetworkRotation);
        }
    }
}