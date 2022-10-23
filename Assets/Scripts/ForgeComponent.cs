using ACSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForgeComponent : MonoBehaviour
{
	public Forge forge;
	//public List<EntityComponent> entities;

	public bool save;

	private void Update() {
		if(save) {
			save = false;
			Save();
		}
	}

	void Save() {
		EntityComponent[] entities = GetComponentsInChildren<EntityComponent>();
		for(int i = 0; i < entities.Length; i++) {
			if(entities[i].edit) {
				Forge.EntityEditEntry edit = new Forge.EntityEditEntry() {
					datafile = (int)entities[i].datafileID,
					datafileOffset = entities[i].datafileOffset,
					transformOffset = entities[i].gameStateTransformMatrixOffset,
					x = entities[i].transform.localPosition.x,
					y = entities[i].transform.localPosition.y,
					z = entities[i].transform.localPosition.z
				};
				Debug.Log($"datafile:  {edit.datafile}");
				Debug.Log($"datafileoffset:  {edit.datafileOffset}");
				Debug.Log($"transformOffset: {edit.transformOffset}");
				Debug.Log($"x: {edit.x}");
				Debug.Log($"y: {edit.y}");
				Debug.Log($"z: {edit.z}");
				forge.EditEntity(edit);
				break;
			}
		}
	}

}
