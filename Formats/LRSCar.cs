using System.Collections.Generic;

namespace LibLR1
{
	public class LRSCar
	{
		private string m_name;
		private byte[] m_configBytes;
		private byte m_recordId;
		private string m_chassisTag;
		private byte m_brickCount;
		private byte[] m_headerPad;
		private byte[] m_kartMetadata;
		private List<BrickPlacement> m_bricks;
		private byte[] m_trailingBytes;

		/// <summary>UTF-16LE name, up to 14 characters (e.g. "JOAN OF KART", "PLAYER").</summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// 4 bytes at record offset 0x1C: Head, Hat, Legs, Body indexes into BODYPART.PCB. Stored raw for round-trip fidelity.
		/// </summary>
		public byte[] ConfigBytes
		{
			get { return m_configBytes; }
			set { m_configBytes = value; }
		}

		/// <summary>
		/// Raw record byte at offset 0x20. Bit 7 is the live flag; the low nibble is
		/// the immutable face slot selected during character creation.
		/// </summary>
		public byte RecordIdRaw
		{
			get { return m_recordId; }
			set { m_recordId = value; }
		}

		/// <summary>True when RecordIdRaw bit 7 is set.</summary>
		public bool IsLive { get { return (m_recordId & 0x80) != 0; } }
		/// <summary>Character-creation face slot stored in RecordIdRaw bits 0-3.</summary>
		public byte FaceSlot { get { return (byte)(m_recordId & 0x0F); } }
		/// <summary>Undecoded RecordIdRaw bits 4-6; observed as zero.</summary>
		public byte ReservedBits { get { return (byte)((m_recordId >> 4) & 0x07); } }

		/// <summary>ASCII chassis tag, e.g. "dachas0", "crchas0", "PLAYER".</summary>
		public string ChassisTag
		{
			get { return m_chassisTag; }
			set { m_chassisTag = value; }
		}

		/// <summary>Record offset 0x2A. Includes the chassis as implicit brick #0.</summary>
		public byte BrickCount
		{
			get { return m_brickCount; }
			set { m_brickCount = value; }
		}

		/// <summary>5 bytes at record offsets 0x2B-0x2F. Preserve raw.</summary>
		public byte[] HeaderPad
		{
			get { return m_headerPad; }
			set { m_headerPad = value; }
		}

		/// <summary>3 bytes at record offsets 0x30-0x32. Preserve raw; byte 0 is exposed as TemplateMarker.</summary>
		public byte[] KartMetadata
		{
			get { return m_kartMetadata; }
			set { m_kartMetadata = value; }
		}

		/// <summary>
		/// Identifier inherited from the source QuickBuild template when a kart is loaded or edited.
		/// Chassis-scoped value; not a global ordinal — different templates on the same chassis may share the same value.
		/// Semantics are partial: the byte appears to encode a build-style or handling category, but the exact runtime interpretation is not yet known.
		/// </summary>
		public byte TemplateMarker
		{
			get { return m_kartMetadata != null && m_kartMetadata.Length > 0 ? m_kartMetadata[0] : (byte)0; }
			set { if (m_kartMetadata == null || m_kartMetadata.Length != 3) m_kartMetadata = new byte[3]; m_kartMetadata[0] = value; }
		}

		/// <summary>Decoded standard 8-byte brick placements. Unsupported stock tail forms are preserved in TrailingBytes.</summary>
		public List<BrickPlacement> Bricks
		{
			get { return m_bricks; }
			set { m_bricks = value; }
		}

		/// <summary>Bytes after the last stored brick through the end of the 640-byte record.</summary>
		public byte[] TrailingBytes
		{
			get { return m_trailingBytes; }
			set { m_trailingBytes = value; }
		}

		/// <summary>
		/// Conservative FACE snapshot heuristic. This intentionally prefers false negatives over classifying live racers as snapshots.
		/// </summary>
		public bool IsSnapshot
		{
			get
			{
				if (m_brickCount != 1 || m_recordId < 0x82 || m_configBytes == null || m_configBytes.Length != 4)
					return false;
				if (m_configBytes[0] != 0 || m_configBytes[1] != 0 || m_configBytes[2] != 0 || m_configBytes[3] != 0)
					return false;
				return !string.IsNullOrWhiteSpace(m_name) && m_name.StartsWith("FACE", System.StringComparison.OrdinalIgnoreCase);
			}
		}
	}
}
