using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Text.RegularExpressions;

public class ShaderCodeExtractorWindow : EditorWindow
{
    Object      shader;
    string      outPath;

    List<string>    shaderIncludePaths = new List<string>();

    ReorderableList includePathList;

    [MenuItem("Window/ShaderCodeExtractor")]
    static void Init()
    {
        ShaderCodeExtractorWindow window = EditorWindow.GetWindow<ShaderCodeExtractorWindow>();
        window.Show();
    }

    private void OnEnable()
    {
        includePathList = new ReorderableList(shaderIncludePaths, typeof(string));

        includePathList.onAddCallback = (list) =>
        {
            shaderIncludePaths.Add("");
        };

        includePathList.drawElementCallback = (rect, index, active, selected) =>
        {
            Rect textRect = rect;
            Rect folderPicker = rect;
            textRect.xMax -= textRect.width / 2;
            folderPicker.xMin += textRect.width ;
            shaderIncludePaths[index] = EditorGUI.TextField(textRect, shaderIncludePaths[index]);
            if (GUI.Button(folderPicker, "Pick folder"))
                shaderIncludePaths[index] = EditorUtility.OpenFolderPanel("Shader Include Path", Application.dataPath, "");
        };
    }

    void OnGUI()
    {
        GUILayout.Label("Shader Code Extractor", EditorStyles.boldLabel);
        shader = EditorGUILayout.ObjectField("Shader/Compute", shader, typeof(Object), false);

        outPath = EditorGUILayout.TextField("Output file", outPath);

        includePathList.DoLayoutList();

        if (GUILayout.Button("Extract code"))
        {
            if (shader is ComputeShader || shader is Shader)
                ExtractShaderCode();
        }
    }

    string GetIncludeCode(string path)
    {
        // TODO
        return path;
    }

    string ResolveIncludes(string shaderCode)
    {
        Regex includeRegex = new Regex("#\\s*include\\s+\"(.*)\"");

        var matches = includeRegex.Matches(shaderCode);

        // TODO: add multi include protection

        Debug.Log("includes:");
        // Replace the include with the actual file code:
        foreach (Match match in matches)
        {
            var path = match.Groups[1].Captures[0].Value;

            string includeCode = GetIncludeCode(path);

            includeCode = ResolveIncludes(includeCode);

            includeRegex.Replace(shaderCode, includeCode, 1);
        }

        return shaderCode;
    }

    void ExtractShaderCode()
    {
        var path = AssetDatabase.GetAssetPath(shader);

        string shaderCode = File.ReadAllText(path);

        shaderCode = ResolveIncludes(shaderCode);

        Debug.Log(shaderCode);
    }
}
