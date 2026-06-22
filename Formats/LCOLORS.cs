using System;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>Global ordered palette from MENUDATA\PIECEDB\L_COLORS.LEB.</summary>
	public sealed class LCOLORS
	{
		private static readonly string[] s_defaultNames = { "black", "white", "biege", "ltgray", "dkgray", "red", "brown", "yellow", "green", "blue" };
		private readonly LColors m_inner;

		public IReadOnlyList<string> Names { get { return m_inner.Names; } }
		public static IReadOnlyList<string> DefaultNames { get { return s_defaultNames; } }

		public LCOLORS(string p_filepath)
		{
			m_inner = new LColors(p_filepath);
			if (m_inner.Names.Count != 10)
				throw new InvalidDataException("L_COLORS.LEB must contain its 10-entry color array.");
		}

		public bool TryGetName(byte p_index, out string p_name)
		{
			if (p_index < m_inner.Names.Count) { p_name = m_inner.Names[p_index]; return true; }
			p_name = null;
			return false;
		}

		/// <summary>Writes the original token-encoded bytes unchanged; palette mutation is intentionally not supported.</summary>
		public void Write(string p_filepath)
		{
			using (FileStream stream = File.Create(p_filepath))
				m_inner.Save(stream);
		}
	}
}
