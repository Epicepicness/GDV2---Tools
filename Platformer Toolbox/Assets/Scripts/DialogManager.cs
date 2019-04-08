using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class DialogManager : MonoBehaviour{

	private SceneDialogs sceneDialogs;
	private Dialog currentDialog;
	private DialogLine currentDialogLine;

	[SerializeField] private GameObject dialogCanvas;
	[SerializeField] private GameObject bigSpeakerObject;
	[SerializeField] private GameObject smallSpeakerObject;
	[SerializeField] private Image bigSpriteLeft;
	[SerializeField] private Image bigSpriteRight;
	[SerializeField] private Text nameText;
	[SerializeField] private Text dialogText;
	[SerializeField] private GameObject pressToContinue;
	[SerializeField] private Button[] responseButtons = new Button[4];

	private List<Actors> allActors = new List<Actors> ();


	// Loads the Dialogs out of an XML file from "Resources/XML_Files/SceneDialogs", and the passed file name.
	// This is called during OnNewLevelLoaded function in the GameManager
	public void LoadDialog (string fileName) {
		//Creates a path to an xml file, and only an xml file.
		string xmlPath = Path.Combine ("Resources/XML_Files/SceneDialogs", fileName);
		if (Path.GetExtension (xmlPath) == string.Empty) {
			xmlPath += ".xml";
		} else if (Path.GetExtension (xmlPath) != ".xml") {
			Path.ChangeExtension (xmlPath, ".xml");
		}

		if (File.Exists (Path.Combine (Application.dataPath, xmlPath))) {
			sceneDialogs = XML_Loader.Deserialize <SceneDialogs> ((Path.Combine (Application.dataPath, xmlPath)));
		} else {
			Debug.LogError ("XML File: " + (Path.Combine (Application.dataPath, xmlPath)) + " is not found.");
		}
	}

	public void LoadActors () {
		Actors[] actors = ObjectLoader.GetAtPath<Actors> ("Resources/DialogActors");
		allActors.AddRange (actors);
	}

	///<summary>
	/// Opens the Dialogwindow, and starts the dialog with the passed ID.
	///</summary>
	public void StartDialog (int dialogID) {
		currentDialog = sceneDialogs.allDialogsInScene [dialogID];

		//Check for type of dialog, and to use big/small sprites.
		if (currentDialog.BigSprites == true) {
			bigSpeakerObject.SetActive (true);
			smallSpeakerObject.SetActive (false);
			bigSpriteLeft.gameObject.SetActive (false);
			bigSpriteLeft.gameObject.SetActive (false);
		}
		else {
			bigSpeakerObject.SetActive (false);
			smallSpeakerObject.SetActive (true);
		}
		dialogCanvas.SetActive (true);

		NextDialogLine (0);
	}

	public void EndDialog () {
		dialogCanvas.SetActive (false);
		UIManager.instance.EndDialog ();
	}

	public void NextDialogLine () {
		NextDialogLine (currentDialogLine.followUpLine);
	}

	private void NextDialogLine (int lineID) {
		//If given negative ID, end the dialog
		if (lineID < 0) {
			EndDialog ();
			return;
		}

		currentDialogLine = currentDialog.DialogLines [lineID];

		Actors selectedActor = null;

		foreach (Actors a in allActors) {
			if (a.actorName == currentDialogLine.actorObjectName) {
				selectedActor = a;
				break;
			}
		}

		nameText.text = selectedActor.actorName;
		dialogText.text = currentDialogLine.text;

		if (currentDialogLine.responses.Count == 0) {
			pressToContinue.SetActive (true);
			for (int i = 0; i <= 3; i++) {
				responseButtons [i].gameObject.SetActive (false);
			}
		} else {
			pressToContinue.SetActive (false);

			for (int i = 0; i <= 3; i++) {
				if (i <= currentDialogLine.responses.Count - 1) {
					responseButtons [i].onClick.RemoveAllListeners ();
					responseButtons [i].GetComponentInChildren<Text> ().text = currentDialogLine.responses [i].text;
					int local_i = i;
					responseButtons [i].onClick.AddListener (delegate {
						NextDialogLine (currentDialogLine.responses [local_i].followupLine);
					});
					responseButtons [i].gameObject.SetActive (true);
				} else {
					responseButtons [i].gameObject.SetActive (false);
				}
			}
		}

		Sprite selectedSprite = null;
		int x = 0;
		foreach (string s in selectedActor.actorSpriteNames) {
			if (s == currentDialogLine.spriteObjectName) {
				selectedSprite = selectedActor.actorSprites[x];
				break;
			}
			x++;
		}

		//Setting up Sprites
		if (currentDialog.BigSprites) {
			if (!bigSpriteLeft.IsActive ()) {
				bigSpriteLeft.gameObject.SetActive (true);
				bigSpriteLeft.sprite = selectedSprite;
			} else {
				bigSpriteRight.gameObject.SetActive (true);
				bigSpriteRight.sprite = selectedSprite;
			}
		}
	}

}
