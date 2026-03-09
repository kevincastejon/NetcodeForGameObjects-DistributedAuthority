using Unity.Netcode;
using UnityEngine;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody2D
{
    public struct CompleteRigidbody2DPose : INetworkSerializable
    {
        public Vector2 position;
        public float rotation;
        public Vector3 scale;
        public Vector2 linearVelocity;
        public float angularVelocity;

        public static CompleteRigidbody2DPose FromRigidbody(Rigidbody2D rigidbody)
        {
            return new CompleteRigidbody2DPose() { position = rigidbody.position, rotation = rigidbody.rotation, scale = rigidbody.transform.localScale, linearVelocity = rigidbody.linearVelocity, angularVelocity = rigidbody.angularVelocity };
        }
        public void FeedRigidbody(Rigidbody2D rigidbody)
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
