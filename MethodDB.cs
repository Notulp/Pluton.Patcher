namespace Pluton.Patcher
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using Reflection;
	using System.Security.Cryptography;

	public class MethodDB
	{
		Hashtable methoddb;
		public static MethodDB instance;
		static string PATH;
		static int TABLE;

		static string ACS_SHA1 = GetSha1Hash("Assembly-CSharp.dll");

		const int Version = 1;

		const string orgPrfx = "_orig",
			edtdPrfx = "_edtd";

		public object this[int tablename, object key] {
			get {
				return Get(tablename, key);
			}
			set {
				Add(tablename, key, value);
			}
		}

		public object this[string tablename, object key] {
			get {
				return Get(tablename, key);
			}
			set {
				Add(tablename, key, value);
			}
		}

		#region create/getinstance/load/save

		public MethodDB(string path)
		{
			methoddb = new Hashtable();
			PATH = path;
			Load();
		}

		public static MethodDB GetInstance()
		{
			if (instance == null)
				instance = new MethodDB("Methods.db");

			return instance;
		}

		public void Load()
		{
			if (!MainClass.gendiffs)
				return;

			if (File.Exists(PATH)) {
				try {
					methoddb = HashtableFromFile(PATH);

					object v = Get("Config", "Version");
					TABLE = ((int)Get("Config", "LatestBuild")) + 1;

					if (TABLE > 1 && (string)Get("Config", "LatestACS-SHA1") == ACS_SHA1) {
						MainClass.gendiffs = false;
						return;
					}

					if (TABLE == 21) {
						MoveTables(2, 21);
						TABLE = 20;
					}

					if ((int)v != Version)
						MainClass.LogErrorLine($"MethodDB version differs. [file: {v} assembly: {Version}]");

					MainClass.LogLine("MethodDB loaded from: " + PATH);
				} catch (Exception ex) {
					MainClass.LogException(ex);
				}
			} else {
				Add("Config", "Version", Version);
				Add("Config", "LatestBuild", 0);
				Save();
				TABLE = 1;
			}
		}

		public void Save()
		{
			if (!MainClass.gendiffs)
				return;

			Add("Config", "LatestBuild", TABLE);
			Add("Config", "LatestACS-SHA1", ACS_SHA1);
			HashtableToFile(methoddb, PATH);
		}

		#endregion

		#region methodspecific

		public static bool CheckMethod(MethodPatcher method, bool original)
		{
			if (TABLE == 1)
				return true;

			return (string)instance.Get(TABLE - 1, GetKeyName(method, original)) == method.methodDefinition.PrintCSharp();
		}

		public static string GetDifferences()
		{
			string result = String.Empty;

			if (TABLE > 1) {
				Hashtable previous = instance.GetTable(TABLE - 1);

				foreach (DictionaryEntry entry in instance.GetTable(TABLE).AsParallel()) {
					if (!((string)entry.Key).Contains(edtdPrfx) && previous.ContainsKey(entry.Key)) { // Check if we patched that method earlier
						var prv = (string)previous[entry.Key];
						var cur = (string)entry.Value;
						if (prv != cur) {
							result += MainClass.GetHtmlDiff(prv, cur);
						}
					}
				}
			}
			return result;
		}

		public static void StoreMethod(MethodPatcher method, bool original)
		{
			if (MainClass.gendiffs)
				instance.Add(TABLE, GetKeyName(method, original), method.methodDefinition.PrintCSharp());
		}

		static string GetKeyName(MethodPatcher method, bool original) => $"{method.FriendlyName}{(original ? orgPrfx : edtdPrfx)}";

		#endregion

		#region adders

		public void Add(int tablename, object key, object val)
		{
			if (key == null)
				throw new NullReferenceException("object 'key' is null in MethodDB.Add()");

			if (!MainClass.gendiffs)
				return;

			var hashtable = (Hashtable)methoddb[tablename];
			if (hashtable == null) {
				hashtable = new Hashtable();
				methoddb.Add(tablename, hashtable);
			}

			hashtable[key] = val;
		}

		public void Add(string tablename, object key, object val)
		{
			if (key == null)
				throw new NullReferenceException("object 'key' is null in MethodDB.Add()");

			if (!MainClass.gendiffs)
				return;

			var hashtable = (Hashtable)methoddb[tablename];
			if (hashtable == null) {
				hashtable = new Hashtable();
				methoddb.Add(tablename, hashtable);
			}

			hashtable[key] = val;
		}

		public void MoveTables(int from, int till)
		{
			for (int i = from; i < till; i++)
				MoveTable(i, i - 1);
		}

		public void MoveTable(int tableFrom, int tableTo)
		{
			if (methoddb.ContainsKey(tableFrom)) {
				if (methoddb.ContainsKey(tableTo))
					methoddb[tableTo] = methoddb[tableFrom];
				else
					methoddb.Add(tableTo, methoddb[tableFrom]);
				methoddb.Remove(tableFrom);
			}
		}

		#endregion

		#region getters

		public object Get(int tablename, object key)
		{
			if (key == null)
				return null;

			var hashtable = (Hashtable)methoddb[tablename];
			return hashtable == null ? null : hashtable[key];
		}

		public object Get(string tablename, object key)
		{
			if (key == null)
				return null;

			var hashtable = (Hashtable)methoddb[tablename];
			return hashtable == null ? null : hashtable[key];
		}

		public Hashtable GetTable(int tablename)
		{
			if (methoddb.ContainsKey(tablename))
				return (Hashtable)methoddb[tablename];

			return null;
		}

		#endregion

		#region misc

		public static Hashtable HashtableFromFile(string path)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open))
				return (Hashtable)new BinaryFormatter().Deserialize(stream);
		}

		public static void HashtableToFile(Hashtable ht, string path)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create))
				new BinaryFormatter().Serialize(stream, ht);
		}

		public static string GetSha1Hash(string filePath)
		{
			using (FileStream fs = File.OpenRead(filePath))
				return BitConverter.ToString(new SHA1Managed().ComputeHash(fs));
		}

		#endregion
	}
}

