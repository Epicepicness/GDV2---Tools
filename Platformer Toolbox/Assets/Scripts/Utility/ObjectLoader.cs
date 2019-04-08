using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

	// Loads in all the asset files from a given Directory, and returns an array of those.

public class ObjectLoader {

	public static T [] GetAtPath<T> (string path) where T : Object {
		List<T> al = new List<T> ();
		//ArrayList al = new ArrayList ();
		string [] fileEntries = Directory.GetFiles (Application.dataPath + "/" + path);

		foreach (string fileName in fileEntries) {
			string temp = fileName.Replace ("\\", "/");
			int index = temp.LastIndexOf ("/");
			string localPath = "Assets/" + path;

			if (index > 0)
				localPath += temp.Substring (index);

			Object t = AssetDatabase.LoadAssetAtPath (localPath, typeof (T));

			if (t != null)
				al.Add ((T)t);

		}

		T [] result = new T [al.Count];

		for (int i = 0; i < al.Count; i++)
			result [i] = (T) al [i];

		return result;
	}


}
