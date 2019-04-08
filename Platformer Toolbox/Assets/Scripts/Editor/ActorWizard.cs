using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ActorWizard : ScriptableWizard {

	private string fileDirectory = "Assets/Resources/DialogActors";

	public string actorName;
	public Sprite actorAvatar;
	public List<string> actorSpriteNames = new List<string> ();
	public List<Sprite> actorSprites = new List<Sprite> ();

	[MenuItem ("Tools/Dialog/Actor Wizard")]
	public static void CreateWizard () {
		ScriptableWizard.DisplayWizard<ActorWizard> ("Create Light", "Create");
	}

	private void OnWizardCreate () {

		Actors asset = ScriptableObject.CreateInstance<Actors> ();
		asset.actorAvatar = actorAvatar;
		asset.actorName = actorName;

		for (int i = 0; i < actorSpriteNames.Count; i++) {
			if (actorSprites.Count <= i + 1) {	//Keeps spriteNames and sprites equal in number
				asset.actorSpriteNames.Add (actorSpriteNames [i]);
				asset.actorSprites.Add (actorSprites [i]);
			}
		}

		// Making sure the Actor Folder exists
		string folderPath = Path.Combine (Application.dataPath, "Resources/DialogActors");
		if (!AssetDatabase.IsValidFolder (folderPath)) {
			Directory.CreateDirectory (folderPath);
		}

		// Making sure the given file-name is legit
		string path = Path.Combine (fileDirectory, actorName);
		if (Path.GetExtension (path) == string.Empty) {
			path += ".asset";
		}
		else if (Path.GetExtension (path) != ".asset") {
			Path.ChangeExtension (path, ".asset");
		}

		AssetDatabase.CreateAsset (asset, path);
		AssetDatabase.SaveAssets ();

		EditorUtility.FocusProjectWindow ();

		Selection.activeObject = asset;
	}

}
