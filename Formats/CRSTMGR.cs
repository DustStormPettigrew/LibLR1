using System;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>Ordered CSET filename registry from MENUDATA\PIECEDB\CRSTMGR.LEB. Registry tags are ordinals and are not LRS BrickPlacement.SetTag values.</summary>
	public sealed class CRSTMGR
	{
		private readonly CrstMgr m_inner;
		public IReadOnlyList<string> CsetFiles { get { return m_inner.CSetFilenames; } }

		public CRSTMGR(string p_filepath)
		{
			m_inner = new CrstMgr(p_filepath);
		}

		public bool TryGetCsetFile(byte p_setTag, out string p_fileName)
		{
			int index = p_setTag - 0x0B;
			if (index >= 0 && index < m_inner.CSetFilenames.Count) { p_fileName = m_inner.CSetFilenames[index]; return true; }
			p_fileName = null;
			return false;
		}

		/// <summary>Resolves an LRS BrickPlacement.SetTag through the validated SetTag registry.</summary>
		public bool TryGetCsetFileForLrsSetTag(byte p_setTag, out string p_fileName)
		{
			if (SetTagRegistry.TryGet(p_setTag, out SetTagDefinition definition) && definition.CsetFile != null)
			{
				p_fileName = definition.CsetFile;
				return true;
			}
			p_fileName = null;
			return false;
		}

		public void Write(string p_filepath)
		{
			using (FileStream stream = File.Create(p_filepath))
				m_inner.Save(stream);
		}
	}
}
