using LibLR1.Adapters;
using LibLR1.Contracts;
using LibLR1.Export;
using LibLR1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Tester
{
	internal static class ExportRunner
	{
		public static void Run(string[] p_args)
		{
			LocalSettings settings = LoadJson<LocalSettings>("locals.json");
			if (settings == null || string.IsNullOrEmpty(settings.InstallationPath))
			{
				throw new InvalidOperationException("locals.json must define InstallationPath.");
			}

			LocalGameFiles gameFiles = LoadOptionalJson<LocalGameFiles>("localGameFiles.json") ?? new LocalGameFiles();
			SceneExportSelection selection = ResolveSelection(settings.InstallationPath, gameFiles, p_args);
			TrackScene scene = BuildScene(selection);
			string outputPath = selection.OutputPath;

			if (string.IsNullOrEmpty(outputPath))
			{
				string exportFileName = scene.Name + ".json";
				outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports", exportFileName);
			}

			TrackSceneJsonExporter.ExportToFile(scene, outputPath);
			Console.WriteLine("Exported track scene JSON:");
			Console.WriteLine(outputPath);
		}

		private static TrackScene BuildScene(SceneExportSelection p_selection)
		{
			TrackScene scene = null;
			WDB wdb = null;

			if (!string.IsNullOrEmpty(p_selection.WdbPath))
			{
				wdb = new WDB(p_selection.WdbPath);
				scene = WDBAdapter.ToScene(wdb, Path.GetFileNameWithoutExtension(p_selection.WdbPath));
			}

			if (scene == null)
			{
				scene = new TrackScene();
				scene.Name = "ExportedScene";
				scene.SourceName = "Contracts";
			}

			List<string> gdbPaths = new List<string>();
			for (int i = 0; i < p_selection.GdbPaths.Count; i++)
			{
				if (!string.IsNullOrEmpty(p_selection.GdbPaths[i]))
				{
					gdbPaths.Add(p_selection.GdbPaths[i]);
				}
			}

			if (gdbPaths.Count == 0 && wdb != null && wdb.GDBs != null)
			{
				for (int i = 0; i < wdb.GDBs.Length; i++)
				{
					string resolved = FindFileByName(p_selection.InstallationPath, wdb.GDBs[i]);
					if (!string.IsNullOrEmpty(resolved))
					{
						gdbPaths.Add(resolved);
					}
				}
			}

			for (int i = 0; i < gdbPaths.Count; i++)
			{
				GDB gdb = new GDB(gdbPaths[i]);
				TrackScene gdbScene = GDBAdapter.ToScene(gdb, Path.GetFileNameWithoutExtension(gdbPaths[i]));
				MergeScene(scene, gdbScene, true, true, false, false, false, "GDB." + i);
			}

			if (!string.IsNullOrEmpty(p_selection.SkbPath))
			{
				SKB skb = new SKB(p_selection.SkbPath);
				TrackScene skbScene = SKBAdapter.ToScene(skb, Path.GetFileNameWithoutExtension(p_selection.SkbPath));
				MergeScene(scene, skbScene, false, true, false, false, true, "SKB");
			}

			if (!string.IsNullOrEmpty(p_selection.RrbPath))
			{
				RRB rrb = new RRB(p_selection.RrbPath);
				TrackScene rrbScene = RRBAdapter.ToScene(rrb, Path.GetFileNameWithoutExtension(p_selection.RrbPath));
				MergeScene(scene, rrbScene, true, false, true, true, false, "RRB");
			}

			scene.Metadata["Export.InstallationPath"] = p_selection.InstallationPath;
			return scene;
		}

		private static void MergeScene(
			TrackScene p_target,
			TrackScene p_source,
			bool p_includeMeshes,
			bool p_includeMaterials,
			bool p_includeObjects,
			bool p_includePaths,
			bool p_includeGradients,
			string p_metadataPrefix)
		{
			if (p_source == null)
			{
				return;
			}

			if (p_includeMeshes)
			{
				for (int i = 0; i < p_source.Meshes.Count; i++)
				{
					p_target.Meshes.Add(p_source.Meshes[i]);
				}
			}

			if (p_includeMaterials)
			{
				for (int i = 0; i < p_source.Materials.Count; i++)
				{
					p_target.Materials.Add(p_source.Materials[i]);
				}
			}

			if (p_includeObjects)
			{
				for (int i = 0; i < p_source.Objects.Count; i++)
				{
					p_target.Objects.Add(p_source.Objects[i]);
				}
			}

			if (p_includePaths)
			{
				for (int i = 0; i < p_source.Paths.Count; i++)
				{
					p_target.Paths.Add(p_source.Paths[i]);
				}
			}

			if (p_includeGradients)
			{
				for (int i = 0; i < p_source.Gradients.Count; i++)
				{
					p_target.Gradients.Add(p_source.Gradients[i]);
				}
			}

			foreach (KeyValuePair<string, string> pair in p_source.Metadata)
			{
				p_target.Metadata[p_metadataPrefix + "." + pair.Key] = pair.Value;
			}
		}

		private static SceneExportSelection ResolveSelection(string p_installationPath, LocalGameFiles p_gameFiles, string[] p_args)
		{
			SceneExportSelection selection = new SceneExportSelection();
			selection.InstallationPath = p_installationPath;
			selection.WdbPath = ResolveFilePath(p_installationPath, p_gameFiles.WdbPath, "*.WDB");
			selection.SkbPath = ResolveFilePath(p_installationPath, p_gameFiles.SkbPath, "*.SKB");
			selection.RrbPath = ResolveFilePath(p_installationPath, p_gameFiles.RrbPath, "*.RRB");
			selection.OutputPath = ResolveOutputPath(p_gameFiles.OutputPath, p_args);

			if (p_gameFiles.GdbPaths != null)
			{
				for (int i = 0; i < p_gameFiles.GdbPaths.Count; i++)
				{
					string resolved = ResolvePath(p_installationPath, p_gameFiles.GdbPaths[i]);
					if (!string.IsNullOrEmpty(resolved))
					{
						selection.GdbPaths.Add(resolved);
					}
				}
			}

			if (string.IsNullOrEmpty(selection.WdbPath) &&
				string.IsNullOrEmpty(selection.SkbPath) &&
				string.IsNullOrEmpty(selection.RrbPath) &&
				selection.GdbPaths.Count == 0)
			{
				throw new FileNotFoundException("No representative WDB, GDB, SKB, or RRB files could be resolved for export.");
			}

			return selection;
		}

		private static string ResolveOutputPath(string p_configuredOutputPath, string[] p_args)
		{
			if (p_args != null && p_args.Length > 1 && !string.IsNullOrEmpty(p_args[1]))
			{
				return Path.GetFullPath(p_args[1]);
			}

			if (string.IsNullOrEmpty(p_configuredOutputPath))
			{
				return null;
			}

			return Path.GetFullPath(p_configuredOutputPath);
		}

		private static string ResolveFilePath(string p_installationPath, string p_configuredPath, string p_fallbackPattern)
		{
			string resolved = ResolvePath(p_installationPath, p_configuredPath);
			if (!string.IsNullOrEmpty(resolved))
			{
				return resolved;
			}

			string[] matches = Directory.GetFiles(p_installationPath, p_fallbackPattern, SearchOption.AllDirectories);
			return matches.Length > 0 ? matches[0] : null;
		}

		private static string ResolvePath(string p_installationPath, string p_configuredPath)
		{
			if (string.IsNullOrEmpty(p_configuredPath))
			{
				return null;
			}

			if (Path.IsPathRooted(p_configuredPath))
			{
				return File.Exists(p_configuredPath) ? p_configuredPath : null;
			}

			string installationCandidate = Path.Combine(p_installationPath, p_configuredPath);
			if (File.Exists(installationCandidate))
			{
				return installationCandidate;
			}

			string repoCandidate = ResolveRepoFile(p_configuredPath);
			return !string.IsNullOrEmpty(repoCandidate) && File.Exists(repoCandidate) ? repoCandidate : null;
		}

		private static string FindFileByName(string p_root, string p_fileName)
		{
			if (string.IsNullOrEmpty(p_root) || string.IsNullOrEmpty(p_fileName))
			{
				return null;
			}

			string searchName = Path.GetFileName(p_fileName);
			string[] matches = Directory.GetFiles(p_root, searchName, SearchOption.AllDirectories);
			return matches.Length > 0 ? matches[0] : null;
		}

		private static T LoadJson<T>(string p_fileName) where T : class
		{
			string path = ResolveRepoFile(p_fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return null;
			}

			return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
		}

		private static T LoadOptionalJson<T>(string p_fileName) where T : class
		{
			return LoadJson<T>(p_fileName);
		}

		private static string ResolveRepoFile(string p_relativePath)
		{
			string[] candidates = new string[]
			{
				Path.Combine(Directory.GetCurrentDirectory(), p_relativePath),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p_relativePath),
				Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\", p_relativePath)),
				Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\", p_relativePath))
			};

			for (int i = 0; i < candidates.Length; i++)
			{
				if (File.Exists(candidates[i]))
				{
					return candidates[i];
				}
			}

			return candidates[0];
		}

		private sealed class LocalSettings
		{
			public string InstallationPath { get; set; }
		}

		private sealed class LocalGameFiles
		{
			public string WdbPath { get; set; }
			public List<string> GdbPaths { get; set; }
			public string SkbPath { get; set; }
			public string RrbPath { get; set; }
			public string OutputPath { get; set; }
		}

		private sealed class SceneExportSelection
		{
			public string InstallationPath { get; set; }
			public string WdbPath { get; set; }
			public List<string> GdbPaths { get; private set; }
			public string SkbPath { get; set; }
			public string RrbPath { get; set; }
			public string OutputPath { get; set; }

			public SceneExportSelection()
			{
				GdbPaths = new List<string>();
			}
		}
	}
}
