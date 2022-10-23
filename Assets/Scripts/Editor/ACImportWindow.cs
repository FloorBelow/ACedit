using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ACSharp;
using ACSharp.ResourceTypes;
using System;
using UnityEditor.VersionControl;
using System.IO;
using UnityEngine.WSA;

public class ACImportWindow : EditorWindow {

    enum SelectedGame {
        AC1,
        AC2
    }

    string consoleText = "";
    Material mat;
    Material mat2;
    Dictionary<string, Forge> forges = new Dictionary<string, Forge>();
    SelectedGame selectedGame;
    [MenuItem("Window/AC Importer")]
    static void Init() {
        ACImportWindow window = (ACImportWindow)EditorWindow.GetWindow(typeof(ACImportWindow));
        window.mat = (Material)AssetDatabase.LoadAssetAtPath<Material>(@"Assets\New Material.mat");
        window.mat2 = (Material)AssetDatabase.LoadAssetAtPath<Material>(@"Assets\mat2.mat");
        window.Show();
    }
    private void OnGUI() {
		GUILayout.Label("AC Importer");
        selectedGame = (SelectedGame)EditorGUILayout.EnumPopup(selectedGame);
        if (GUILayout.Button("Do a thing")) Test();
        if (GUILayout.Button("Mesh Test")) TestImportMesh();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("NO HIERARCHY")) TestNoEmpties();
        if (GUILayout.Button("Clear Console")) consoleText = "";
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.TextArea(consoleText);
	}
    void Log(string s) { consoleText = String.Concat(consoleText, s, "\n");  }

    Game CurrentGame() {
        if (selectedGame == SelectedGame.AC1) return Games.AC1;
        return Games.AC2;
    }

    void TestImportMesh() {
        string forgePath = EditorUtility.OpenFilePanel("Import forge meshes", "", "forge");
        AssetPathObject assetPaths = (AssetPathObject)AssetDatabase.LoadAssetAtPath(@$"Assets\{CurrentGame().name}_AssetPaths.asset", typeof(AssetPathObject));
        Forge f = new Forge(forgePath, CurrentGame());
        Dictionary<uint, ForgeFile> fileDict = new Dictionary<uint, ForgeFile>();
        for (int i = 0; i < f.datafileTable.Length; i++) f.OpenDatafile(i, fileDict);
        try {
            string folderpath = $@"Assets\{CurrentGame().name}\{Path.GetFileNameWithoutExtension(f.name)}";
            Directory.CreateDirectory(folderpath);
            AssetDatabase.StartAssetEditing();
            foreach (ForgeFile file in fileDict.Values) {
                if (file.fileType != ACSharp.ResourceTypes.Mesh.id) continue;
                //if (!file.name.StartsWith("House")) continue;
                if (assetPaths.ids.Contains(file.fileID)) continue;
                ACSharp.ResourceTypes.Mesh mesh = (ACSharp.ResourceTypes.Mesh)file.resource;
                if (mesh.compiledMesh == null) continue;
                Vector3[] verts = new Vector3[mesh.compiledMesh.verts.Length];
                for (int i = 0; i < mesh.compiledMesh.verts.Length; i++) {
                    float[] pos = mesh.compiledMesh.verts[i].getPosition();
                    verts[i] = new Vector3(pos[0], pos[1], pos[2]);
                }

                UnityEngine.Mesh m = new UnityEngine.Mesh();
                m.name = file.name.Replace('*','x');
                m.SetVertices(verts);
                m.SetTriangles(mesh.compiledMesh.idx, 0);
                m.RecalculateNormals();
                string assetPath = $@"{folderpath}\{m.name}_{file.fileID}.mesh";
                assetPaths.ids.Add(file.fileID);
                assetPaths.paths.Add(assetPath);
                EditorUtility.SetDirty(assetPaths);
                AssetDatabase.CreateAsset(m, assetPath);

                //break;
            }
        } finally {
            AssetDatabase.StopAssetEditing();
        }
        
        
    }

    void TestNoEmpties() {
        string forgePath = EditorUtility.OpenFilePanel("Load Datablocks", "", "forge");
        Forge f = new Forge(forgePath);
        AssetPathObject assetPaths = (AssetPathObject)AssetDatabase.LoadAssetAtPath<AssetPathObject>(@$"Assets\{CurrentGame().name}_AssetPaths.asset");
        Dictionary<uint, ForgeFile> fileDict = new Dictionary<uint, ForgeFile>();
        for (int i = 0; i < f.datafileTable.Length; i++) f.OpenDatafile(i, fileDict);
        foreach (ForgeFile file in fileDict.Values) {
            if (file.fileType != DataBlock.id) continue;
            DataBlock dataBlock = (DataBlock)file.resource;
            for (int j = 0; j < dataBlock.files.Length; j++) {
                if (!fileDict.ContainsKey(dataBlock.files[j])) continue;
                ForgeFile dblockFile = fileDict[dataBlock.files[j]];
                if (dblockFile.fileType == Entity.id) CreateEntityNoEmpty(dblockFile.name, (Entity)dblockFile.resource, assetPaths, mat);
                else if (dblockFile.fileType == EntityGroup.id) {
                    EntityGroup eg = (EntityGroup)dblockFile.resource;
                    for (int k = 0; k < eg.entities.Count; k++) {
                        CreateEntityNoEmpty($"{dblockFile.name}_{k}", eg.entities[k], assetPaths, mat);
                    }
                }
            }
        }
    }

    void CreateEntityNoEmpty(string name, Entity entity, AssetPathObject assetPaths, Material mat) {
        uint meshid = entity.GetMeshID();
        if (meshid != 0) {
            if (assetPaths.ids.Contains(meshid)) {
                //Debug.Log($"Found mesh {meshid}");
                GameObject fileObject = new GameObject(name);
                MatrixUtil.SetTransform(fileObject.transform, entity.transformationMatrix);         
                MeshFilter mf = fileObject.AddComponent<MeshFilter>();
                mf.sharedMesh = (UnityEngine.Mesh)AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(assetPaths.paths[assetPaths.ids.IndexOf(meshid)]);
                MeshRenderer mr = fileObject.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
            } //else Debug.Log($"Could not find mesh {meshid}");
        }
    }

    void Test() {
        string forgePath = EditorUtility.OpenFilePanel("Load Datablocks", "", "forge");
        Forge f = new Forge(forgePath, CurrentGame());
        AssetPathObject assetPaths = (AssetPathObject)AssetDatabase.LoadAssetAtPath<AssetPathObject>(@$"Assets\{CurrentGame().name}_AssetPaths.asset");
        GameObject forgeObject = new GameObject(Path.GetFileNameWithoutExtension(f.name));
        ForgeComponent forgeComponent = forgeObject.AddComponent<ForgeComponent>();
        forgeComponent.forge = f;
        Dictionary<uint, ForgeFile> fileDict = new Dictionary<uint, ForgeFile>();
        for (int i = 0; i < f.datafileTable.Length; i++) f.OpenDatafile(i, fileDict);
        foreach (ForgeFile file in fileDict.Values) {
            if (file.fileType != DataBlock.id) continue;
            Console.WriteLine($"{file.name}");
            DataBlock dataBlock = (DataBlock)file.resource;
            GameObject dblockObj = new GameObject(file.name);
            dblockObj.transform.SetParent(forgeObject.transform);
            for (int j = 0; j < dataBlock.files.Length; j++) {
                if (!fileDict.ContainsKey(dataBlock.files[j])) continue;
                ForgeFile dblockFile = fileDict[dataBlock.files[j]];
                if (dblockFile.fileType == Entity.id) {
                    Material matref = ((Entity)dblockFile.resource).entityDescriptor == null ? mat : mat2;
                    CreateEntity(dblockFile.name, dblockFile, (Entity)dblockFile.resource, dblockObj.transform, assetPaths);
                } else if (dblockFile.fileType == EntityGroup.id) {
                    EntityGroup eg = (EntityGroup)dblockFile.resource;
                    GameObject egObject = new GameObject(dblockFile.name);
                    egObject.transform.SetParent(dblockObj.transform);
                    for (int k = 0; k < eg.entities.Count; k++) {
                        //Material matref = eg.entities[k].entityDescriptor == null ? mat : mat2;
                        CreateEntity($"{dblockFile.name}_{k}", dblockFile, eg.entities[k], egObject.transform, assetPaths);
                    }
                }
                
                //Console.Write(dblockFile.name);
                //for (int h = 0; h < 16; h++) Console.Write(" " + ((Entity)dblockFile.resource).transformationMatrix[h]);
                //Console.WriteLine();
            }
            Console.WriteLine();
        }
        forgeObject.transform.Rotate(new Vector3(-90, 0, 0));
        forgeObject.transform.localScale = new Vector3(1, -1, 1);
    }

    void CreateEntity(string name, ForgeFile forgeFile, Entity entity, Transform parent, AssetPathObject assetPaths) {
        //Entity entity = (Entity)forgeFile.resource;
        GameObject fileObject = new GameObject(name);
        fileObject.transform.SetParent(parent);
        MatrixUtil.SetTransform(fileObject.transform, entity.transformationMatrix);

        Material material = mat;
        if (entity.entityDescriptor != null) {
            EntityComponent entityComponent = fileObject.AddComponent<EntityComponent>();
            entityComponent.id = forgeFile.fileID;
            entityComponent.datafileID = forgeFile.datafileID;
            entityComponent.datafileOffset = forgeFile.datafileOffset;
            entityComponent.gameStateTransformMatrixOffset = entity.entityDescriptor.gameStateData.transformationMatrixOffset;
            entityComponent.transformMatrix = entity.transformationMatrix;
            material = mat2;
        }

        uint meshid = entity.GetMeshID();
        if (meshid != 0) {
            if (assetPaths.ids.Contains(meshid)) {
                //Debug.Log($"Found mesh {meshid}");
                MeshFilter mf = fileObject.AddComponent<MeshFilter>();
                mf.sharedMesh = (UnityEngine.Mesh)AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(assetPaths.paths[assetPaths.ids.IndexOf(meshid)]);
                MeshRenderer mr = fileObject.AddComponent<MeshRenderer>();
                mr.sharedMaterial = material;
            } //else Debug.Log($"Could not find mesh {meshid}");
        }
    }
}
