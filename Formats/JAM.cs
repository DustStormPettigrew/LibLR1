using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LibLR1
{
	/// <summary>
	/// LEGO Racers LJAM archive reader and writer. Entries are stored uncompressed.
	/// </summary>
	public sealed class JAM
	{
		private const int NameFieldSize = 12;
		private const int FileRecordSize = 20;
		private const int DirectoryRecordSize = 16;
		private const int MaximumDirectoryDepth = 128;

		private readonly DirectoryNode m_root;
		private readonly List<JAMFile> m_files;
		private readonly List<string> m_directories;

		public IReadOnlyList<JAMFile> Files { get { return m_files.AsReadOnly(); } }
		public IReadOnlyList<string> Directories { get { return m_directories.AsReadOnly(); } }

		public JAM(string p_filepath)
			: this(File.ReadAllBytes(p_filepath))
		{
		}

		public JAM(Stream p_stream)
			: this(ReadAllBytes(p_stream))
		{
		}

		private JAM(byte[] p_data)
		{
			if (p_data == null || p_data.Length < 4 || p_data[0] != (byte)'L' || p_data[1] != (byte)'J' || p_data[2] != (byte)'A' || p_data[3] != (byte)'M')
			{
				throw new InvalidDataException("Not an LJAM archive.");
			}

			HashSet<uint> activeDirectories = new HashSet<uint>();
			m_root = ReadDirectory(p_data, 4, activeDirectories, 0);
			m_files = new List<JAMFile>();
			m_directories = new List<string>();
			RebuildIndexes();
		}

		private JAM(DirectoryNode p_root)
		{
			m_root = p_root ?? throw new ArgumentNullException(nameof(p_root));
			m_files = new List<JAMFile>();
			m_directories = new List<string>();
			RebuildIndexes();
		}

		/// <summary>Creates an archive model from a directory tree. Every file and directory component must fit the 12-byte ASCII JAM name field.</summary>
		public static JAM FromDirectory(string p_directory)
		{
			if (string.IsNullOrWhiteSpace(p_directory))
			{
				throw new ArgumentException("A source directory is required.", nameof(p_directory));
			}
			DirectoryInfo root = new DirectoryInfo(p_directory);
			if (!root.Exists)
			{
				throw new DirectoryNotFoundException("JAM source directory was not found: " + p_directory);
			}
			return new JAM(CreateDirectoryNode(root, true));
		}

		/// <summary>Writes this archive in a deterministic, valid LJAM layout.</summary>
		public void Write(string p_filepath)
		{
			if (string.IsNullOrWhiteSpace(p_filepath))
			{
				throw new ArgumentException("An output file path is required.", nameof(p_filepath));
			}
			string parent = Path.GetDirectoryName(Path.GetFullPath(p_filepath));
			if (!string.IsNullOrEmpty(parent))
			{
				Directory.CreateDirectory(parent);
			}
			using (FileStream stream = File.Create(p_filepath))
			{
				Write(stream);
			}
		}

		public void Write(Stream p_stream)
		{
			if (p_stream == null || !p_stream.CanWrite)
			{
				throw new ArgumentException("A writable output stream is required.", nameof(p_stream));
			}

			long cursor = 4;
			AssignDirectoryOffsets(m_root, ref cursor);
			AssignFileOffsets(m_root, ref cursor);
			if (cursor > uint.MaxValue)
			{
				throw new InvalidDataException("JAM archives cannot exceed 4 GiB.");
			}

			using (BinaryWriter writer = new BinaryWriter(p_stream, Encoding.ASCII, true))
			{
				writer.Write(new byte[] { (byte)'L', (byte)'J', (byte)'A', (byte)'M' });
				WriteDirectoryTables(writer, m_root);
				WriteFileData(writer, m_root);
			}
		}

		/// <summary>Extracts the archive without permitting paths outside the requested output directory.</summary>
		public void Extract(string p_outputDirectory, bool p_overwrite)
		{
			if (string.IsNullOrWhiteSpace(p_outputDirectory))
			{
				throw new ArgumentException("An output directory is required.", nameof(p_outputDirectory));
			}
			if (Directory.Exists(p_outputDirectory) && !p_overwrite)
			{
				throw new IOException("Output directory already exists. Use an empty path or explicitly allow overwrite: " + p_outputDirectory);
			}

			string root = Path.GetFullPath(p_outputDirectory);
			Directory.CreateDirectory(root);
			foreach (string directory in m_directories)
			{
				Directory.CreateDirectory(ResolveExtractionPath(root, directory));
			}
			foreach (JAMFile file in m_files)
			{
				string output = ResolveExtractionPath(root, file.Path);
				string parent = Path.GetDirectoryName(output);
				if (!string.IsNullOrEmpty(parent))
				{
					Directory.CreateDirectory(parent);
				}
				if (File.Exists(output) && !p_overwrite)
				{
					throw new IOException("Refusing to overwrite extracted file: " + output);
				}
				File.WriteAllBytes(output, file.Data);
			}
		}

		/// <summary>Replaces a file payload while retaining the archive's directory structure.</summary>
		public void ReplaceFile(string p_archivePath, byte[] p_data)
		{
			if (p_data == null)
			{
				throw new ArgumentNullException(nameof(p_data));
			}
			string path = NormalizeArchivePath(p_archivePath);
			JAMFile file = m_files.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));
			if (file == null)
			{
				throw new FileNotFoundException("Archive entry was not found: " + path, path);
			}
			file.SetData(p_data);
		}

		private static DirectoryNode ReadDirectory(byte[] p_data, uint p_offset, HashSet<uint> p_activeDirectories, int p_depth)
		{
			if (p_depth > MaximumDirectoryDepth)
			{
				throw new InvalidDataException("JAM directory nesting exceeds the supported limit.");
			}
			if (!p_activeDirectories.Add(p_offset))
			{
				throw new InvalidDataException(string.Format("JAM directory table cycle at offset 0x{0:X}.", p_offset));
			}
			try
			{
				int position = CheckedOffset(p_data, p_offset, 4, "directory file count");
				uint fileCountValue = ReadUInt32(p_data, ref position);
				int fileCount = CheckedRecordCount(fileCountValue, p_data.Length - position, FileRecordSize, "file");
				DirectoryNode directory = new DirectoryNode();
				HashSet<string> fileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < fileCount; i++)
				{
					RequireRange(p_data, position, FileRecordSize, "file record");
					string name = ReadName(p_data, position);
					position += NameFieldSize;
					uint dataOffset = ReadUInt32(p_data, ref position);
					uint size = ReadUInt32(p_data, ref position);
					if (!fileNames.Add(name))
					{
						throw new InvalidDataException("JAM directory contains duplicate file name: " + name);
					}
					int payloadOffset = CheckedOffset(p_data, dataOffset, size, "file payload");
					byte[] payload = new byte[size];
					Buffer.BlockCopy(p_data, payloadOffset, payload, 0, (int)size);
					directory.Files.Add(new JAMFile(name, payload, dataOffset));
				}

				RequireRange(p_data, position, 4, "directory count");
				uint directoryCountValue = ReadUInt32(p_data, ref position);
				int directoryCount = CheckedRecordCount(directoryCountValue, p_data.Length - position, DirectoryRecordSize, "directory");
				HashSet<string> directoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				List<KeyValuePair<string, uint>> childTables = new List<KeyValuePair<string, uint>>(directoryCount);
				for (int i = 0; i < directoryCount; i++)
				{
					RequireRange(p_data, position, DirectoryRecordSize, "directory record");
					string name = ReadName(p_data, position);
					position += NameFieldSize;
					uint childOffset = ReadUInt32(p_data, ref position);
					if (!directoryNames.Add(name) || fileNames.Contains(name))
					{
						throw new InvalidDataException("JAM directory contains duplicate entry name: " + name);
					}
					childTables.Add(new KeyValuePair<string, uint>(name, childOffset));
				}

				foreach (KeyValuePair<string, uint> child in childTables)
				{
					DirectoryNode childDirectory = ReadDirectory(p_data, child.Value, p_activeDirectories, p_depth + 1);
					childDirectory.Name = child.Key;
					directory.Directories.Add(childDirectory);
				}
				return directory;
			}
			finally
			{
				p_activeDirectories.Remove(p_offset);
			}
		}

		private static DirectoryNode CreateDirectoryNode(DirectoryInfo p_directory, bool p_isRoot)
		{
			DirectoryNode node = new DirectoryNode { Name = p_isRoot ? string.Empty : ValidateName(p_directory.Name) };
			foreach (FileInfo file in p_directory.GetFiles().OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
			{
				node.Files.Add(new JAMFile(ValidateName(file.Name), File.ReadAllBytes(file.FullName), 0));
			}
			foreach (DirectoryInfo child in p_directory.GetDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
			{
				node.Directories.Add(CreateDirectoryNode(child, false));
			}
			return node;
		}

		private void RebuildIndexes()
		{
			m_files.Clear();
			m_directories.Clear();
			IndexDirectory(m_root, string.Empty);
		}

		private void IndexDirectory(DirectoryNode p_directory, string p_parentPath)
		{
			foreach (JAMFile file in p_directory.Files)
			{
				file.Path = CombineArchivePath(p_parentPath, file.Name);
				m_files.Add(file);
			}
			foreach (DirectoryNode child in p_directory.Directories)
			{
				string path = CombineArchivePath(p_parentPath, child.Name);
				m_directories.Add(path);
				IndexDirectory(child, path);
			}
		}

		private static void AssignDirectoryOffsets(DirectoryNode p_directory, ref long p_cursor)
		{
			p_directory.TableOffset = CheckedUInt32(p_cursor, "directory table offset");
			p_cursor = checked(p_cursor + 8L + p_directory.Files.Count * FileRecordSize + p_directory.Directories.Count * DirectoryRecordSize);
			if (p_cursor > uint.MaxValue)
			{
				throw new InvalidDataException("JAM directory tables exceed the 4 GiB address space.");
			}
			foreach (DirectoryNode child in p_directory.Directories)
			{
				AssignDirectoryOffsets(child, ref p_cursor);
			}
		}

		private static void AssignFileOffsets(DirectoryNode p_directory, ref long p_cursor)
		{
			foreach (JAMFile file in p_directory.Files)
			{
				file.DataOffset = CheckedUInt32(p_cursor, "file data offset");
				p_cursor = checked(p_cursor + file.Data.LongLength);
				if (p_cursor > uint.MaxValue)
				{
					throw new InvalidDataException("JAM file data exceeds the 4 GiB address space.");
				}
			}
			foreach (DirectoryNode child in p_directory.Directories)
			{
				AssignFileOffsets(child, ref p_cursor);
			}
		}

		private static void WriteDirectoryTables(BinaryWriter p_writer, DirectoryNode p_directory)
		{
			if (p_writer.BaseStream.Position != p_directory.TableOffset)
			{
				throw new InvalidDataException("Internal JAM directory-table offset calculation failed.");
			}
			p_writer.Write((uint)p_directory.Files.Count);
			foreach (JAMFile file in p_directory.Files)
			{
				WriteName(p_writer, file.Name);
				p_writer.Write(file.DataOffset);
				p_writer.Write((uint)file.Data.Length);
			}
			p_writer.Write((uint)p_directory.Directories.Count);
			foreach (DirectoryNode child in p_directory.Directories)
			{
				WriteName(p_writer, child.Name);
				p_writer.Write(child.TableOffset);
			}
			foreach (DirectoryNode child in p_directory.Directories)
			{
				WriteDirectoryTables(p_writer, child);
			}
		}

		private static void WriteFileData(BinaryWriter p_writer, DirectoryNode p_directory)
		{
			foreach (JAMFile file in p_directory.Files)
			{
				if (p_writer.BaseStream.Position != file.DataOffset)
				{
					throw new InvalidDataException("Internal JAM file-data offset calculation failed.");
				}
				p_writer.Write(file.Data);
			}
			foreach (DirectoryNode child in p_directory.Directories)
			{
				WriteFileData(p_writer, child);
			}
		}

		private static byte[] ReadAllBytes(Stream p_stream)
		{
			if (p_stream == null || !p_stream.CanRead)
			{
				throw new ArgumentException("A readable input stream is required.", nameof(p_stream));
			}
			using (MemoryStream copy = new MemoryStream())
			{
				p_stream.CopyTo(copy);
				return copy.ToArray();
			}
		}

		private static uint ReadUInt32(byte[] p_data, ref int p_position)
		{
			RequireRange(p_data, p_position, 4, "32-bit value");
			uint value = (uint)(p_data[p_position] | (p_data[p_position + 1] << 8) | (p_data[p_position + 2] << 16) | (p_data[p_position + 3] << 24));
			p_position += 4;
			return value;
		}

		private static string ReadName(byte[] p_data, int p_position)
		{
			RequireRange(p_data, p_position, NameFieldSize, "name");
			int length = 0;
			while (length < NameFieldSize && p_data[p_position + length] != 0)
			{
				if (p_data[p_position + length] > 0x7F)
				{
					throw new InvalidDataException("JAM entry names must be ASCII.");
				}
				length++;
			}
			if (length == 0)
			{
				throw new InvalidDataException("JAM entries cannot have an empty name.");
			}
			return ValidateName(Encoding.ASCII.GetString(p_data, p_position, length));
		}

		private static void WriteName(BinaryWriter p_writer, string p_name)
		{
			byte[] name = Encoding.ASCII.GetBytes(ValidateName(p_name));
			p_writer.Write(name);
			p_writer.Write(new byte[NameFieldSize - name.Length]);
		}

		private static string ValidateName(string p_name)
		{
			if (string.IsNullOrWhiteSpace(p_name) || p_name == "." || p_name == ".." || p_name.IndexOf('/') >= 0 || p_name.IndexOf('\\') >= 0)
			{
				throw new InvalidDataException("JAM entry names must be a single non-empty path component.");
			}
			if (p_name.Any(c => c > 0x7F) || Encoding.ASCII.GetByteCount(p_name) > NameFieldSize)
			{
				throw new InvalidDataException(string.Format("JAM entry name `{0}` must be ASCII and no longer than {1} bytes.", p_name, NameFieldSize));
			}
			return p_name;
		}

		private static string NormalizeArchivePath(string p_path)
		{
			if (string.IsNullOrWhiteSpace(p_path))
			{
				throw new ArgumentException("An archive entry path is required.", nameof(p_path));
			}
			string[] components = p_path.Replace('\\', '/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (components.Length == 0)
			{
				throw new InvalidDataException("An archive entry path is required.");
			}
			return string.Join("/", components.Select(ValidateName));
		}

		private static string CombineArchivePath(string p_parent, string p_name)
		{
			return string.IsNullOrEmpty(p_parent) ? p_name : p_parent + "/" + p_name;
		}

		private static string ResolveExtractionPath(string p_root, string p_archivePath)
		{
			string root = Path.GetFullPath(p_root);
			string path = root;
			foreach (string component in NormalizeArchivePath(p_archivePath).Split('/'))
			{
				path = Path.Combine(path, component);
			}
			string fullPath = Path.GetFullPath(path);
			string rootPrefix = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? root : root + Path.DirectorySeparatorChar;
			if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("JAM extraction path escapes the requested output directory.");
			}
			return fullPath;
		}

		private static int CheckedOffset(byte[] p_data, uint p_offset, uint p_length, string p_description)
		{
			long end = (long)p_offset + p_length;
			if (p_offset > p_data.Length || end > p_data.Length)
			{
				throw new InvalidDataException(string.Format("JAM {0} at offset 0x{1:X} exceeds the archive length.", p_description, p_offset));
			}
			return (int)p_offset;
		}

		private static int CheckedRecordCount(uint p_count, int p_remaining, int p_recordSize, string p_description)
		{
			if (p_count > (uint)(p_remaining / p_recordSize))
			{
				throw new InvalidDataException(string.Format("JAM {0} count {1} exceeds the remaining directory table data.", p_description, p_count));
			}
			return (int)p_count;
		}

		private static uint CheckedUInt32(long p_value, string p_description)
		{
			if (p_value < 0 || p_value > uint.MaxValue)
			{
				throw new InvalidDataException("JAM " + p_description + " exceeds the 32-bit format limit.");
			}
			return (uint)p_value;
		}

		private static void RequireRange(byte[] p_data, int p_offset, int p_length, string p_description)
		{
			if (p_offset < 0 || p_length < 0 || p_offset > p_data.Length - p_length)
			{
				throw new InvalidDataException("JAM " + p_description + " exceeds the archive length.");
			}
		}

		private sealed class DirectoryNode
		{
			public string Name = string.Empty;
			public uint TableOffset;
			public List<JAMFile> Files = new List<JAMFile>();
			public List<DirectoryNode> Directories = new List<DirectoryNode>();
		}
	}

	/// <summary>File payload and path metadata inside a <see cref="JAM"/> archive.</summary>
	public sealed class JAMFile
	{
		private byte[] m_data;

		internal JAMFile(string p_name, byte[] p_data, uint p_dataOffset)
		{
			Name = p_name;
			SetData(p_data);
			DataOffset = p_dataOffset;
		}

		public string Name { get; private set; }
		public string Path { get; internal set; }
		public uint DataOffset { get; internal set; }
		public long Size { get { return m_data.LongLength; } }
		public byte[] Data { get { return (byte[])m_data.Clone(); } }

		internal void SetData(byte[] p_data)
		{
			m_data = (byte[])p_data.Clone();
		}
	}
}
