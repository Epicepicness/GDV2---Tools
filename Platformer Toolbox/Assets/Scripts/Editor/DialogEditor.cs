using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class DialogEditor : EditorWindow {

	private List <string> allXmlFileNames = new List <string> ();
	private SceneDialogs currentSceneDialog;
	private string xmlDirectory = "Resources/XML_Files/SceneDialogs";
	private string actorDirectory = "Resources/DialogActors";

	private string selectedSceneName;
	private int selectedSceneIndexInput = 0;							//Holds the input for the XML drop-down menu.
	private string createNewFileNameInput = "";

	private Dialog selectedDialog;
	private Vector2 scrollPosition = Vector2.zero;						//Used for the list of Dialogs on the left side
	private int dialogIDInput;
	private string dialogDescriptionInput;

	private Vector2 gridScrollPosition = Vector2.zero;
	private Texture2D gridBackground;
	private List<DialogGridNode> gridNodes = new List<DialogGridNode> ();
	private List<Rect> windows = new List<Rect>();
	private Rect startNode;
	private Rect endNode;

	private DialogLine selectedDialogLine;

	private List<Actors> availibleActorsList = new List<Actors> ();
	private List<string> availibleActorNames = new List<string> ();
	private Actors selectedActor;
	private KeyValuePair<string, Sprite> selectedSprite;
	private int actorInputIndex = 0;									//Holds the input for the actors drop-down menu.
	private int spriteInputIndex = 0;                                   //Holds the input for the actors drop-down menu.
	private Sprite lineSprite;
	private string lineTextInput;
	private int followUpLineInput;
	private int responseCountInput;

	private List<string> responseTextInput = new List<string> ();
	private List<int> responseFollowUpIDInput = new List<int> ();


	[MenuItem ("Tools/Dialog/DialogEditor")]
	public static void Init () {
		EditorWindow.GetWindow (typeof(DialogEditor));
	}

	private void OnEnable () {
		startNode = new Rect (10, 10, 50, 50);
		endNode = new Rect (500, 500, 50, 50);

		ReloadData ();
	}

#region  //--- Data / Loading Functions ---------------------------------------------------------------------------------------------------
	// Makes a list of all the currently existing XML files and Actor list, and loads in the first XML-file.
	private void ReloadData () {
		LoadXmlFileList ();
		if (allXmlFileNames.Count != 0) {
			LoadXmlFileData (allXmlFileNames [0]);
			selectedSceneName = allXmlFileNames [0];
		}

		Actors[] actors = ObjectLoader.GetAtPath<Actors> (actorDirectory);
		availibleActorsList.AddRange (actors);
		foreach (Actors a in availibleActorsList) {
			availibleActorNames.Add (a.actorName);
		}
	}

	private void LoadXmlFileList () { 
		allXmlFileNames.Clear ();
		DirectoryInfo dir = new DirectoryInfo (Path.Combine ("Assets", xmlDirectory));
		FileInfo[] info = dir.GetFiles ("*.xml");
		foreach (FileInfo f in info) {
			allXmlFileNames.Add (f.Name);
		}
	}

	// Reads and Serializes a specific XML file
	private void LoadXmlFileData (string fileName) {
		string xmlPath = Path.Combine (xmlDirectory, fileName);

		if (Path.GetExtension (xmlPath) == string.Empty) {
			xmlPath += ".xml";
		} else if (Path.GetExtension (xmlPath) != ".xml") {
			Path.ChangeExtension (xmlPath, ".xml");
		}

		if (File.Exists (Path.Combine (Application.dataPath, xmlPath))) {
			currentSceneDialog = XML_Loader.Deserialize <SceneDialogs> ((Path.Combine (Application.dataPath, xmlPath)));
			if (currentSceneDialog.allDialogsInScene [0] != null) {
				SelectDialogByID (0);
				selectedDialogLine = null;
				/*if (selectedDialog.DialogLines.Count != 0) {
					SelectDialogLine (0);
				}*/
			}
		} else {
			Debug.LogError ("XML File: " + (Path.Combine (Application.dataPath, xmlPath)) + " is not found.");
		}
	}
#endregion

#region  //--- XML-File Functions ---------------------------------------------------------------------------------------------------
	private void CreateNewXMLFile (string name) {
		// Making sure the file directory exists
		if (!AssetDatabase.IsValidFolder (xmlDirectory)) {
			Directory.CreateDirectory (Path.Combine (Application.dataPath, xmlDirectory));
		}

		// Making sure the given file-name is legit
		string path = Path.Combine (Application.dataPath, Path.Combine (xmlDirectory, name));
		if (Path.GetExtension (path) == string.Empty) {
			path += ".xml";
		}
		else if (Path.GetExtension (path) != ".xml") {
			Path.ChangeExtension (path, ".xml");
		}
		
		XmlDocument doc = new XmlDocument ();
		XmlNode rootNode = doc.CreateElement ("SceneDialogs");
		doc.AppendChild (rootNode);

		XmlNode dialogNode = doc.CreateElement ("Dialog");
		XmlAttribute IDattribute = doc.CreateAttribute ("ID");
		IDattribute.Value = "0";
		dialogNode.Attributes.Append (IDattribute);
		XmlAttribute bigSprites = doc.CreateAttribute ("BigSprites");
		bigSprites.Value = "true";
		dialogNode.Attributes.Append (bigSprites);
		rootNode.AppendChild (dialogNode);

		XmlNode description = doc.CreateElement ("Description");
		description.InnerText = "DialogDescription";
		dialogNode.AppendChild (description);

		doc.Save (path);

		ReloadData ();
	}

	private void SafeToXMLFile () {
		// Making sure the file directory exists
		if (!AssetDatabase.IsValidFolder (xmlDirectory)) {
			Directory.CreateDirectory (Path.Combine (Application.dataPath, xmlDirectory));
		}

		// Making sure the given file-name is legit
		string path = Path.Combine (Application.dataPath, Path.Combine (xmlDirectory, selectedSceneName));
		if (Path.GetExtension (path) == string.Empty) {
			path += ".xml";
		}
		else if (Path.GetExtension (path) != ".xml") {
			Path.ChangeExtension (path, ".xml");
		}

		XML_Loader.Serialize (currentSceneDialog, path);
	}

	private void DeleteCurrentXMLFile () {
		string path = Path.Combine (Application.dataPath, Path.Combine (xmlDirectory, allXmlFileNames [selectedSceneIndexInput]));
		File.Delete (path);
		File.Delete (path + ".meta");

		ReloadData ();
	}
#endregion

#region  //--- Dialog Functions ---------------------------------------------------------------------------------------------------
	private void CreateNewDialog () {
		Dialog newDialog = new Dialog (currentSceneDialog.allDialogsInScene.Count);
		currentSceneDialog.allDialogsInScene.Add (newDialog);
		// ZORG DAT DE NIEUWE DIALOG ID NIET EEN ANDER KAN OVERWRITEN
	}

	private void RemoveSelectedDialog () {
		foreach (Dialog dialog in currentSceneDialog.allDialogsInScene) {
			if (dialog.ID == selectedDialog.ID) {
				currentSceneDialog.allDialogsInScene.Remove (dialog);
				break;
			}
		}
	}

	// Called from the Dialog-Selection-List Buttons; sets the selected Dialog.
	private void SelectDialogByID (int id) {
		foreach (Dialog d in currentSceneDialog.allDialogsInScene) {
			if (d.ID == id) {
				selectedDialog = d;
			}
		}

		dialogDescriptionInput = selectedDialog.dialogDescription;
		dialogIDInput = selectedDialog.ID;

		gridNodes.Clear (); windows.Clear ();
		int i = 0;		// Creates the associated grid-windows for each DialogLine
		foreach (DialogLine line in selectedDialog.DialogLines) {
			Rect r = new Rect (10 + 100 * i, 70, 100, 100);
			windows.Add (r);
			gridNodes.Add (new DialogGridNode (line, r, SelectDialogLine));
			i++;
		}
		selectedDialogLine = null;

		/*if (selectedDialog.DialogLines.Count > 0) {
			SelectDialogLine (0);
		}*/
	}

	// Changes the selected Dialog's ID and description
	private void AdjustDialogSettings () {
		if (dialogIDInput != selectedDialog.ID) {
			bool IDAlreadyExists = false;
			foreach (Dialog d in currentSceneDialog.allDialogsInScene) {
				if (d.ID == dialogIDInput) {
					IDAlreadyExists = true;
				}
			}
			if (IDAlreadyExists) {
				Debug.Log ("ID EXISTS");
				// GEEF POPUP MET 'ID ALREADY EXISTS'.
				return;
			}
		}
		selectedDialog.ID = dialogIDInput;
		selectedDialog.dialogDescription = dialogDescriptionInput;
	}
#endregion

#region //--- DialogLine Functions ---------------------------------------------------------------------------------------------------
	private void CreateNewDialogLine () {
		DialogLine newLine = new DialogLine (selectedDialog.DialogLines.Count);
		selectedDialog.DialogLines.Add (newLine);
		// ZORG DAT DE NIEUWE DIALOG ID NIET EEN ANDER KAN OVERWRITEN

		Rect r = new Rect (110, 250, 100, 100);
		windows.Add (r);
		gridNodes.Add (new DialogGridNode (newLine, r, SelectDialogLine));
	}

	private void SelectDialogLine (int id) {
		selectedDialogLine = selectedDialog.DialogLines [id];

		// Selecting the Actor
		if (availibleActorsList.Count != 0) {
			selectedActor = availibleActorsList [0];
			int i = 0;
			foreach (Actors a in availibleActorsList) {
				if (a.actorName == selectedDialogLine.actorObjectName) {
					selectedActor = a;
					actorInputIndex = i;
					break;
				}
				i++;
			}

			// Selecting the sprite
			if (selectedActor.actorSprites.Count != 0) {
				selectedSprite = new KeyValuePair<string, Sprite> (selectedActor.actorSpriteNames [0], selectedActor.actorSprites [0]);

				int x = 0;
				foreach (string s in selectedActor.actorSpriteNames) {
					if (s == selectedDialogLine.spriteObjectName && selectedActor.actorSprites[x] != null) {
						selectedSprite = new KeyValuePair<string, Sprite> (selectedActor.actorSpriteNames [0], selectedActor.actorSprites [0]); ;
						spriteInputIndex = x;
						break;
					}
					x++;
				}
			}
		} else {
			selectedActor = null;
		}
		
		lineTextInput = selectedDialogLine.text;
		responseCountInput = selectedDialogLine.responses.Count;
		followUpLineInput = selectedDialogLine.followUpLine;

		responseTextInput.Clear ();
		responseFollowUpIDInput.Clear ();
		foreach (Response r in selectedDialogLine.responses) {
			responseTextInput.Add (r.text);
			responseFollowUpIDInput.Add (r.followupLine);
		}
	}

	// Adds/Removes from the response lists to make sure their length is equal to the responseCount
	private void AdjustResponseLists () {
		if (Mathf.Sign (responseCountInput - responseTextInput.Count) == 1) {
			while (responseCountInput != responseTextInput.Count) {
				responseTextInput.Add ("");
				responseFollowUpIDInput.Add (-1);
			}
		} else {
			while (responseCountInput != responseTextInput.Count) {
				responseTextInput.RemoveAt (responseTextInput.Count - 1);
				responseFollowUpIDInput.RemoveAt (responseFollowUpIDInput.Count - 1);
			}
		}
	}

	// Sets the Line/Response objects to be equal to the given values
	private void ApplyDialogLineChanges () {
		selectedDialogLine.text = lineTextInput;
		selectedDialogLine.followUpLine = followUpLineInput;

		selectedDialogLine.actorObjectName = selectedActor.actorName;
		selectedDialogLine.spriteObjectName = selectedSprite.Key;

		if (responseCountInput > 0) {
			for (int i = 0; i < responseCountInput; i++) {
				if (i > selectedDialogLine.responses.Count -1) {
					selectedDialogLine.responses.Add (new Response (i, responseTextInput [i], responseFollowUpIDInput [i]));
				} else {
					selectedDialogLine.responses [i].text = responseTextInput [i];
					selectedDialogLine.responses [i].responseID = responseFollowUpIDInput [i];
				}
			}
		}
		if (responseCountInput < selectedDialogLine.responses.Count) {
			while (responseCountInput != responseTextInput.Count) {
				responseTextInput.RemoveAt (responseTextInput.Count - 1);
				responseFollowUpIDInput.RemoveAt (responseFollowUpIDInput.Count - 1);
			}
		}
	}
	#endregion

#region  //--- Grid/Map Functions ---------------------------------------------------------------------------------------------------
	private void SetBackgroundTexture () {
		gridBackground = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		gridBackground.SetPixel (0, 0, new Color (0.2f, 0.2f, 0.2f, 1f));
		gridBackground.Apply ();
	}

	// Draws the Nodes and Connection lines in the grid view
	private void FillNodeGrid () {
		startNode = GUI.Window (-1, startNode, StartEndNodes, "Start");
		endNode = GUI.Window (-2, endNode, StartEndNodes, "End");

		if (selectedDialog.DialogLines.Count == 0 || windows.Count == 0)
			return;

		// Draws the windows in the grid
		int i = 0;
		foreach (Rect r in windows) {
			windows [i] = GUI.Window (i, windows [i], DrawNodeWindow, "Line" + i);
			i++;
		}

		// Draws the lines between the windows
		DrawNodeCurve (startNode, windows [0]);
		foreach (DialogLine line in selectedDialog.DialogLines) {
			if (line.responses.Count == 0) {
				DrawNodeCurve (windows [line.lineID], (line.followUpLine != -1) ? windows [line.followUpLine] : endNode);
			} else {
				foreach (Response r in line.responses) {
					if (windows [line.lineID] != null) {
						if (r.followupLine < 0)			// Draws the nodes, but checks if the required windows exist
							DrawNodeCurve (windows [line.lineID], endNode);
						else if (windows [r.followupLine] != null)
							DrawNodeCurve (windows [line.lineID], windows [r.followupLine]);
					}
				}
			}
		}
	}

	// Creates the Start and End nodes
	private void StartEndNodes (int id) {
		if (id == -1)
			GUILayout.Label ("Start", EditorStyles.boldLabel);
		else
			GUILayout.Label ("End", EditorStyles.boldLabel);
		GUI.DragWindow ();
	}

	private void DrawNodeWindow (int id) {
		DialogLine dL = gridNodes [id].dialogLine;

		GUILayout.Label ("Line ID: " + dL.lineID, EditorStyles.boldLabel);
		GUILayout.Label (dL.text, EditorStyles.label);
		GUILayout.Label ("#responses: " + dL.responses.Count, EditorStyles.label);
		if (GUILayout.Button ("Select", GUILayout.ExpandWidth (false))) {
			SelectDialogLine (dL.lineID);
		}

		GUI.DragWindow ();
	}

	private void DrawNodeCurve (Rect start, Rect end) {
		Vector3 startPos = new Vector3 (start.x + start.width, start.y + start.height / 2, 0);
		Vector3 endPos = new Vector3 (end.x, end.y + end.height / 2, 0);
		Vector3 startTan = startPos + Vector3.right * 50;
		Vector3 endTan = endPos + Vector3.left * 50;
		Color shadowCol = new Color (0, 0, 0, 0.06f);

		for (int i = 0; i < 3; i++) {
			Handles.DrawBezier (startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
		}

		Handles.DrawBezier (startPos, endPos, startTan, endTan, Color.black, null, 1);
	}

	private void ProcessNodeEvents (Event e) {	// Not in use atm
		if (gridNodes == null)					// Was used for dialogLine selection when clicking on windows
			return;

		for (int i = gridNodes.Count - 1; i >= 0; i--) {
			gridNodes [i].ProcessEvents (e);
		}
	}
	#endregion

#region //--- OnGUI ---------------------------------------------------------------------------------------------------
	private void OnGUI () {

		EditorGUIUtility.labelWidth = 75f;

		//--- The XML-File Manipulation Tool at the top ---------------------------------------------------------------------------------------------------
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ();
		GUILayout.Label ("Scene Settings", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal ();
		{
			// The XML-File Manipulation Tools
			if (GUILayout.Button ("Delete Current XMLFile", GUILayout.ExpandWidth (false))) {
				//GEEF DIT EEN CONFIRMATION BUTTON
				DeleteCurrentXMLFile ();
			}
			if (GUILayout.Button ("Save Current XMLFile", GUILayout.ExpandWidth (false))) {
				SafeToXMLFile ();
			}
			if (GUILayout.Button ("Create New XMLFile", GUILayout.ExpandWidth (false))) {
				if (createNewFileNameInput != "") {
					//DOE HEIR EEN CHECK OF DAT HET BESTAND AL BESTAAT
					CreateNewXMLFile (createNewFileNameInput);
				} else {
					//GEEF HIER 'NIET GELUKT' FEEDBACK POPUP
				}
			}
			createNewFileNameInput = EditorGUILayout.TextField ("File Name: ", createNewFileNameInput, GUILayout.Width (300));
		}
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		{
			//--- The XML-File selection Tool ---------------------------------------------------------------------------------------------------
			GUILayout.Label ("Select XML File", GUILayout.Width (100));
			if (allXmlFileNames.Count == 0) {
				GUILayout.Label ("Create an xml file above", EditorStyles.label);
			} else {
				int previouslySelected = selectedSceneIndexInput;
				selectedSceneIndexInput = EditorGUILayout.Popup (selectedSceneIndexInput, allXmlFileNames.ToArray (), GUILayout.Width (200));
				if (previouslySelected != selectedSceneIndexInput) {
					selectedSceneName = allXmlFileNames [selectedSceneIndexInput];
					LoadXmlFileData (allXmlFileNames [selectedSceneIndexInput]);
				}
				GUILayout.Label (selectedSceneName, EditorStyles.label, GUILayout.Width (150));
				GUILayout.Label ("Index: " + selectedSceneIndexInput.ToString (), EditorStyles.label, GUILayout.Width (75));
				GUILayout.Label ("Number of Dialogs: " + currentSceneDialog.allDialogsInScene.Count, EditorStyles.label, GUILayout.Width (150));
			}
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
		EditorGUILayout.Space ();

		if (allXmlFileNames.Count == 0)
			return;

		// Main Content Area
		GUILayout.BeginHorizontal ();
		{
			//--- Left Area, containing the Scene's Dialog Tree ---------------------------------------------------------------------------------------------------
			GUILayout.BeginVertical ();
			{
				//--- The buttons at the top ---
				GUILayout.Label ("Dialog Options", EditorStyles.boldLabel, GUILayout.Width (300));
				if (GUILayout.Button ("Create New Dialog", GUILayout.ExpandWidth (false))) {
					CreateNewDialog ();
				}
				if (GUILayout.Button ("Remove Selected Dialog", GUILayout.ExpandWidth (false))) {
					//GEEF DIT EEN CONFIRMATION BUTTON
					RemoveSelectedDialog ();
				}
				EditorGUILayout.Space ();

				//--- The Dialog Button List ---
				GUILayout.Label ("Dialogs in Scene", EditorStyles.boldLabel, GUILayout.Width (300));
				if (currentSceneDialog != null) {
					int dialogTreeViewLength = currentSceneDialog.allDialogsInScene.Count * 25;
					scrollPosition = GUI.BeginScrollView (new Rect (0, 100, 325, 1000), scrollPosition, new Rect (0, 95, 325, dialogTreeViewLength));
					{
						Color c = GUI.backgroundColor;
						foreach (Dialog dialog in currentSceneDialog.allDialogsInScene) {
							if (dialog == selectedDialog) {
								GUI.backgroundColor = Color.grey;
							}
							if (GUILayout.Button (dialog.ID + ": " + dialog.dialogDescription, GUILayout.ExpandWidth (false), GUILayout.Width (300))) {
								SelectDialogByID (dialog.ID);
							}
							GUI.backgroundColor = c;
						}
					}
					GUI.EndScrollView ();
				}
			}
			GUILayout.EndVertical ();

			if (selectedDialog == null)
				return;
			
			//--- Middle Area, containing the Dialog Settings and Dialog Map ---------------------------------------------------------------------------------------------------
			GUILayout.BeginVertical (GUILayout.Width (1000));
			{
				//--- Buttons to adjust the Selected Dialog ---
				GUILayout.Label ("DialogSettings", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal ();
				{
					dialogIDInput = EditorGUILayout.IntField ("ID:", dialogIDInput, GUILayout.Width (100));
					dialogDescriptionInput = EditorGUILayout.TextField ("Description:", dialogDescriptionInput, GUILayout.ExpandWidth (false), GUILayout.Width (350));
					if (GUILayout.Button ("Update Dialog", GUILayout.ExpandWidth (false), GUILayout.Width (200))) {
						AdjustDialogSettings ();
					}
				}
				GUILayout.EndHorizontal ();
				if (GUILayout.Button ("Create New Dialog Line", GUILayout.ExpandWidth (false), GUILayout.Width (300))) {
					CreateNewDialogLine ();
				}
				EditorGUILayout.Space ();


				//--- Grid to represent all the DialogLines in the selected Dialog ---
				if (gridBackground == null)
					SetBackgroundTexture ();

				//Pas de volgende scroll view groote aan op aantal dialoog dingen
				gridScrollPosition = GUI.BeginScrollView (new Rect (325, 175, position.width - 800, position.height - 175), gridScrollPosition, new Rect (0, 0, 300, 300), true, true);
				GUI.DrawTexture (new Rect (0, 0, position.width - 800, position.height - 150), gridBackground, ScaleMode.StretchToFill);

				BeginWindows ();
				FillNodeGrid ();	// Function that draws the node windows and lines.
				EndWindows ();

				GUI.EndScrollView ();
			}
			GUILayout.EndVertical ();

			if (selectedDialogLine == null)
				return;

			//--- The Right Area, containing the DialogLine Inspector ---------------------------------------------------------------------------------------------------
			GUILayout.BeginVertical ();
			{
				GUILayout.Label ("Dialog Line Inspector", EditorStyles.boldLabel);

				GUILayout.Label ("Line ID: " + selectedDialogLine.lineID, EditorStyles.label);

				//--- Actor Selector ---------------------------------------------------------------------------------------------------
				GUILayout.Label ("Select Actor", GUILayout.Width (100));
				if (availibleActorsList.Count == 0) {
					GUILayout.Label ("Create an actor using the Actor Wizard", EditorStyles.label);
				}
				else {
					int previouslySelected = actorInputIndex;
					actorInputIndex = EditorGUILayout.Popup (actorInputIndex, availibleActorNames.ToArray (), GUILayout.Width (200));
					if (previouslySelected != actorInputIndex) {
						selectedActor = availibleActorsList [actorInputIndex];
					}

					GUILayout.Label ("Select Sprite", GUILayout.Width (100));
					if (selectedActor == null || selectedActor.actorSprites.Count == 0) {
						GUILayout.Label ("Add a sprite to an actor using the Actor Wizard", EditorStyles.label);
					}
					else {
						int lastSelected = spriteInputIndex;
						string [] spriteNameArray = selectedActor.actorSpriteNames.ToArray ();
						spriteInputIndex = EditorGUILayout.Popup (spriteInputIndex, spriteNameArray, GUILayout.Width (200));
						if (lastSelected != spriteInputIndex) {
							selectedSprite = new KeyValuePair<string, Sprite> (spriteNameArray [spriteInputIndex], selectedActor.actorSprites [spriteInputIndex]);
						}
						GUILayout.Label (selectedActor.actorName, EditorStyles.label, GUILayout.Width (150));
					}
				}

				lineTextInput = EditorGUILayout.TextField ("Line Text: ", lineTextInput);

				if (responseCountInput <= 0) {
					GUILayout.Label ("The ID of the Dialog Line that comes after this one if no responses are added.");
					followUpLineInput = EditorGUILayout.IntField ("Followup Line ID: ", followUpLineInput, GUILayout.Width (200));
				}
				responseCountInput = EditorGUILayout.IntField ("Response Count: ", responseCountInput, GUILayout.Width (200));
				GUILayout.Label ("Current Response Count: " + selectedDialogLine.responses.Count, EditorStyles.label);

				if (responseCountInput != responseTextInput.Count)
					AdjustResponseLists ();

				if (responseCountInput > 0) {
					EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

					for (int i = 0; i < responseCountInput; i++) {
						GUILayout.Label ("Response ID: " + i, EditorStyles.label);

						responseTextInput[i] = EditorGUILayout.TextField ("Response Text: ", responseTextInput[i]);
						GUILayout.Label ("The ID of the Dialog Line that comes if this reply is clicked.");
						responseFollowUpIDInput [i] = EditorGUILayout.IntField ("Followup ID: ", responseFollowUpIDInput[i], GUILayout.Width (100));

						EditorGUILayout.Space ();
					}
				}

				if (GUILayout.Button ("Apply", GUILayout.ExpandWidth (false))) {
					ApplyDialogLineChanges ();
				}

			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndHorizontal ();
	}
	#endregion

}
