using System;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>Per-piece color permissions from a *_CSET.LEB file.</summary>
	public sealed class CSET
	{
		private readonly CSet m_inner;
		public string ChassisTag { get; }
		public IReadOnlyDictionary<string, HashSet<string>> ValidColors { get { return m_inner.ValidColorsByPiece; } }

		public CSET(string p_filepath)
		{
			m_inner = new CSet(p_filepath);
			ChassisTag = m_inner.ChassisTag;
		}

		public bool IsColorValid(string p_pieceName, string p_colorName)
		{
			return p_pieceName != null && p_colorName != null && m_inner.ValidColorsByPiece.TryGetValue(p_pieceName, out HashSet<string> colors) && colors.Contains(p_colorName);
		}

		public void Write(string p_filepath)
		{
			using (FileStream stream = File.Create(p_filepath))
				m_inner.Save(stream);
		}
	}
}
