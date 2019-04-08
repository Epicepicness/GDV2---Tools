using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Actors : ScriptableObject {

	public string actorName = "";
	public Sprite actorAvatar;
	public List<string> actorSpriteNames = new List<string> ();
	public List<Sprite> actorSprites = new List<Sprite> ();

}
