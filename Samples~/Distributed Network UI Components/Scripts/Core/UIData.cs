using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkUIComponents
{
    public struct UIData : INetworkSerializable
    {
        public UIDataInternalType _internalType;
        public int intValue;

        public UIData(int intValue) : this()
        {
            _internalType = UIDataInternalType.INT;
            this.intValue = intValue;
        }

        public float floatValue;

        public UIData(float floatValue) : this()
        {
            _internalType = UIDataInternalType.FLOAT;
            this.floatValue = floatValue;
        }

        public bool boolValue;

        public UIData(bool boolValue) : this()
        {
            _internalType = UIDataInternalType.BOOL;
            this.boolValue = boolValue;
        }

        public FixedString128Bytes stringValue;

        public UIData(FixedString128Bytes stringValue) : this() 
        {
            _internalType = UIDataInternalType.STRING;
            this.stringValue = stringValue;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _internalType);
                switch (_internalType)
                {
                    case UIDataInternalType.INT:
                        reader.ReadValueSafe(out intValue);
                        break;
                    case UIDataInternalType.FLOAT:
                        reader.ReadValueSafe(out floatValue);
                        break;
                    case UIDataInternalType.BOOL:
                        reader.ReadValueSafe(out boolValue);
                        break;
                    case UIDataInternalType.STRING:
                        reader.ReadValueSafe(out stringValue);
                        break;
                    default:
                        break;
                }
            }
            if (serializer.IsWriter)
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_internalType);
                switch (_internalType)
                {
                    case UIDataInternalType.INT:
                        writer.WriteValueSafe(intValue);
                        break;
                    case UIDataInternalType.FLOAT:
                        writer.WriteValueSafe(floatValue);
                        break;
                    case UIDataInternalType.BOOL:
                        writer.WriteValueSafe(boolValue);
                        break;
                    case UIDataInternalType.STRING:
                        writer.WriteValueSafe(stringValue);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public enum UIDataInternalType : byte
    {
        INT,
        FLOAT,
        BOOL,
        STRING
    }
}

