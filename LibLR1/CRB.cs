using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Circuit list format.
	/// </summary>
	public class CRB
	{
		private const byte
			ID_CIRCUIT = 0x27;

		private Dictionary<string, CRB_Circuit> m_circuits;

		public Dictionary<string, CRB_Circuit> Circuits
		{
			get { return m_circuits; }
			set { m_circuits = value; }
		}

		public CRB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public CRB(LRBinaryReader p_reader)
		{
			m_circuits = new Dictionary<string, CRB_Circuit>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_CIRCUIT:
					{
						m_circuits = p_reader.ReadDictionaryBlock<CRB_Circuit>(
							CRB_Circuit.Read,
							ID_CIRCUIT
						);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(
							blockId,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
		}

		public void Save(string p_filepath)
		{
			using (LRBinaryWriter writer = new LRBinaryWriter(File.OpenWrite(p_filepath)))
			{
				Save(writer);
			}
		}

		public void Save(LRBinaryWriter p_writer)
		{
			p_writer.WriteByte(ID_CIRCUIT);
			p_writer.WriteDictionaryBlock<CRB_Circuit>(
				CRB_Circuit.Write,
				m_circuits,
				ID_CIRCUIT
			);
		}
	}

	public class CRB_Circuit
	{
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

		public CRB_Circuit(string[] p_players, int p_circuitnumber, int p_circuitnumberagain, bool p_hasunlock, string p_unlock)
		{
			Players            = p_players;
			CircuitNumber      = p_circuitnumber;
			CircuitNumberAgain = p_circuitnumberagain;
			HasUnlock          = p_hasunlock;
			Unlock             = p_unlock;
		}

		public static CRB_Circuit Read(LRBinaryReader p_reader)
		{
			CRB_Circuit val = new CRB_Circuit();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_PLAYERS:
					{
						val.Players = p_reader.ReadStringArrayBlock();
						break;
					}
					case PROPERTY_CIRCUIT_NUMBER:
					{
						val.CircuitNumber = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_CIRCUIT_NUMBER_AGAIN:
					{
						val.CircuitNumberAgain = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_CIRCUIT_UNLOCK:
					{
						val.HasUnlock = true;
						val.Unlock = p_reader.ReadStringWithHeader(); 
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(
							propertyId,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, CRB_Circuit p_value)
		{
			p_writer.WriteByte(PROPERTY_PLAYERS);
			p_writer.WriteStringArrayBlock(p_value.Players);
			p_writer.WriteByte(PROPERTY_CIRCUIT_NUMBER);
			p_writer.WriteIntWithHeader(p_value.CircuitNumber);
			p_writer.WriteByte(PROPERTY_CIRCUIT_NUMBER_AGAIN);
			p_writer.WriteIntWithHeader(p_value.CircuitNumberAgain);
			if (p_value.HasUnlock)
			{
				p_writer.WriteByte(PROPERTY_CIRCUIT_UNLOCK);
				p_writer.WriteStringWithHeader(p_value.Unlock);
			}
		}
	}
}