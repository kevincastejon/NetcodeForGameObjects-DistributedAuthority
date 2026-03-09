using Unity.Netcode;
using UnityEngine;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody_3D
{
    public struct CompleteRigidbodyPose : INetworkSerializable
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;


        public static CompleteRigidbodyPose FromRigidbody(Rigidbody rigidbody)
        {
            return new CompleteRigidbodyPose() { position = rigidbody.position, rotation = rigidbody.rotation, scale = rigidbody.transform.localScale, linearVelocity = rigidbody.linearVelocity, angularVelocity = rigidbody.angularVelocity };
        }
        public void FeedRigidbody(Rigidbody rigidbody)
        {
            rigidbody.position = position;
            rigidbody.rotation = rotation;
            rigidbody.transform.localScale = scale;
            rigidbody.linearVelocity = linearVelocity;
            rigidbody.angularVelocity = angularVelocity;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out position);
                reader.ReadValueSafe(out rotation);
                reader.ReadValueSafe(out scale);
                reader.ReadValueSafe(out linearVelocity);
                reader.ReadValueSafe(out angularVelocity);
            }
            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(position);
                writer.WriteValueSafe(rotation);
                writer.WriteValueSafe(scale);
                writer.WriteValueSafe(linearVelocity);
                writer.WriteValueSafe(angularVelocity);
            }
        }
    }
}
