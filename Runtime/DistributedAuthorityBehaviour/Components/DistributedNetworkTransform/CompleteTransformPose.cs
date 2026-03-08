using Unity.Netcode;
using UnityEngine;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    public struct CompleteTransformPose : INetworkSerializable
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public static CompleteTransformPose FromTransform(Transform transform)
        {
            return new CompleteTransformPose() { position = transform.localPosition, rotation = transform.localRotation, scale = transform.localScale };
        }
        public void FeedTransform(Transform transform)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter 
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out position);
                reader.ReadValueSafe(out rotation);
                reader.ReadValueSafe(out scale);
            }
            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(position);
                writer.WriteValueSafe(rotation);
                writer.WriteValueSafe(scale);
            }
        }
    }
}
