using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    /// <summary>
    /// Circuit list format.
    /// </summary>
    public class CRB {
        private const byte
            ID_CIRCUIT = 0x27;

        private Dictionary<string, CRB_Circuit> m_Circuits;

        public Dictionary<string, CRB_Circuit> Circuits {
            get { return m_Circuits; }
            set { m_Circuits = value; }
        }

        public CRB(Stream stream) {
            m_Circuits = new Dictionary<string, CRB_Circuit>();
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_CIRCUIT:
                        m_Circuits = BinaryFileHelper.ReadDictionaryBlock<CRB_Circuit>(
                            stream,
                            new BinaryFileHelper.ReadObject<CRB_Circuit>(
                                CRB_Circuit.FromStream
                            ),
                            ID_CIRCUIT
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public CRB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_CIRCUIT);
            BinaryFileHelper.WriteDictionaryBlock<CRB_Circuit>(
                stream,
                new BinaryFileHelper.WriteObject<CRB_Circuit>(
                    CRB_Circuit.ToStream
                ),
                m_Circuits,
                ID_CIRCUIT
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class CRB_Circuit {
        private const byte
            PROPERTY_PLAYERS = 0x28,
            PROPERTY_CIRCUIT_NUMBER = 0x29,
            PROPERTY_CIRCUIT_NUMBER_AGAIN = 0x2A,
            PROPERTY_CIRCUIT_UNLOCK = 0x2B;

        public string[] Players;
        public int CircuitNumber;
        public int CircuitNumberAgain;
        public bool HasUnlock;
        public string Unlock;

        public CRB_Circuit()
            : this(new string[0], 0, 0, false, "") { }

        public CRB_Circuit(string[] players, int circuitnumber, int circuitnumberagain, bool hasunlock, string unlock) {
            Players = players;
            CircuitNumber = circuitnumber;
            CircuitNumberAgain = circuitnumberagain;
            HasUnlock = hasunlock;
            Unlock = unlock;
        }

        public static CRB_Circuit FromStream(Stream stream) {
            CRB_Circuit val = new CRB_Circuit();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_PLAYERS: {
                            val.Players = BinaryFileHelper.ReadStringArrayBlock(stream);
                        } break;
                    case PROPERTY_CIRCUIT_NUMBER: {
                            val.CircuitNumber = BinaryFileHelper.ReadIntWithHeader(stream);
                        } break;
                    case PROPERTY_CIRCUIT_NUMBER_AGAIN: {
                            val.CircuitNumberAgain = BinaryFileHelper.ReadIntWithHeader(stream);
                        } break;
                    case PROPERTY_CIRCUIT_UNLOCK: {
                            val.HasUnlock = true;
                            val.Unlock = BinaryFileHelper.ReadStringWithHeader(stream);
                        } break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, CRB_Circuit value) {
            stream.WriteByte(PROPERTY_PLAYERS);
            BinaryFileHelper.WriteStringArrayBlock(
                stream,
                value.Players
            );
            stream.WriteByte(PROPERTY_CIRCUIT_NUMBER);
            BinaryFileHelper.WriteIntWithHeader(
                stream,
                value.CircuitNumber
            );
            stream.WriteByte(PROPERTY_CIRCUIT_NUMBER_AGAIN);
            BinaryFileHelper.WriteIntWithHeader(
                stream,
                value.CircuitNumberAgain
            );
            if (value.HasUnlock) {
                stream.WriteByte(PROPERTY_CIRCUIT_UNLOCK);
                BinaryFileHelper.WriteStringWithHeader(
                    stream,
                    value.Unlock
                );
            }
        }
    }
}