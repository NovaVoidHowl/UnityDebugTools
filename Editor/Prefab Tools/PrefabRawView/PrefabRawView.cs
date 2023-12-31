// Editor mode only script
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;
using StringSplitOptions = System.StringSplitOptions;

using Constants = uk.novavoidhowl.dev.unitydebugtools.Constants;

namespace uk.novavoidhowl.dev.unitydebugtools
{
  public class PrefabRawDataWindow : EditorWindow
  {
    private string prefabRawData;
    private Object prefab;
    private ScrollView scrollView;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Prefab Tools/Raw View (All)")]
    public static void ShowWindow()
    {
      GetWindow<PrefabRawDataWindow>("Prefab Raw View (All)");
    }

    public void OnEnable()
    {
      var root = rootVisualElement;

      var containerPrefabRawDataLoader = new VisualElement();
      containerPrefabRawDataLoader.AddToClassList("containerPrefabRawDataLoader"); // Add class to the container for styling

      var objectField = new ObjectField("Drag Prefab Here");
      objectField.objectType = typeof(GameObject); // Set the object type to GameObject
      objectField.AddToClassList("PrefabRawDataLoader");
      objectField.RegisterValueChangedCallback(evt =>
      {
        var gameObject = evt.newValue as GameObject;
        if (gameObject != null)
        {
          string path = AssetDatabase.GetAssetPath(gameObject);
          if (Path.GetExtension(path).ToLower() == ".fbx")
          {
            // The selected object is an FBX file
            scrollView.Clear();
            EditorUtility.DisplayDialog("Error", "FBX files are not allowed.", "OK");
          }
          else if (PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab)
          {
            // The selected object is a Prefab
            prefab = gameObject;
            prefabRawData = File.ReadAllText(path);
            UpdateScrollView();
          }
          else
          {
            // The selected object is not a Prefab
            scrollView.Clear();
            EditorUtility.DisplayDialog("Error", "Only Prefab objects are allowed.", "OK");
          }
        }
        else
        {
          // The selected object is null
          scrollView.Clear();
          prefabRawData = null;
        }
      });

      // the clear button
      var clearButton = new Button(() =>
      {
        objectField.value = null;
      })
      {
        text = "Clear"
      };
      clearButton.AddToClassList("PrefabRawDataClearButton");

      // add the object field and clear button to the container
      containerPrefabRawDataLoader.Add(objectField);
      containerPrefabRawDataLoader.Add(clearButton);

      // The scroll view container
      var containerScrollView = new VisualElement();
      containerScrollView.AddToClassList("containerScrollView"); // Add class to the container for styling
      // The scroll view
      scrollView = new ScrollView();
      scrollView.AddToClassList("PrefabRawDataScrollView");

      // Add the scroll view to the container
      containerScrollView.Add(scrollView);

      // Add the scroll view container to the root
      // (z-index -1)
      root.Add(containerScrollView);

      // add the control container to the root, so its on top of the scroll view (as you cant use z-index in uss)
      //(z-index 0)
      root.Add(containerPrefabRawDataLoader);

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>("UnityStyleSheets/PrefabRawData");
      root.styleSheets.Add(stylesheet);
    }

    protected virtual void UpdateScrollView()
    {
      scrollView.Clear();
      if (!string.IsNullOrEmpty(prefabRawData))
      {
        var chunks = SplitIntoChunks(prefabRawData);

        // Create a HashSet to store the unique types found in the chunks
        var types = new HashSet<string>();

        // Parse the chunks to identify the types
        foreach (var chunk in chunks)
        {
          var lines = chunk.Split(new[] { '\n' }, StringSplitOptions.None);
          if (lines.Length > 1)
          {
            var typeLine = lines[1];
            var typeEndIndex = typeLine.IndexOf(':');
            if (typeEndIndex != -1)
            {
              var type = typeLine.Substring(0, typeEndIndex).Trim();
              types.Add(type);
            }
          }
        }

        // container for the type labels
        var containerTypeLabels = new VisualElement();
        containerTypeLabels.AddToClassList("containerTypeLabels"); // Add class to the container for styling

        // display count for each unique type
        foreach (var type in types)
        {
          var count = 0;
          foreach (var chunk in chunks)
          {
            var lines = chunk.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length > 1)
            {
              var typeLine = lines[1];
              var typeEndIndex = typeLine.IndexOf(':');
              if (typeEndIndex != -1)
              {
                var chunkType = typeLine.Substring(0, typeEndIndex).Trim();
                if (chunkType == type)
                {
                  count++;
                }
              }
            }
          }
          var label = new Label();
          label.text = type + " (" + count + ")";
          label.AddToClassList("chunk-type-label"); // Add a specific class to the label for styling
          containerTypeLabels.Add(label);
        }

        // Add the type labels to the ScrollView
        scrollView.Add(containerTypeLabels);

        // Add the chunks to the ScrollView
        foreach (var chunk in chunks)
        {
          var textField = new TextField();
          textField.multiline = true;
          textField.value = chunk;
          textField.AddToClassList("chunk-text-field"); // Add a specific class to the text field for styling
          scrollView.Add(textField);
        }
      }
    }

    // Helper method to split a string into chunks of at section boarders (theres no way a singe one will be over max render limit)
    private static IEnumerable<string> SplitIntoChunks(string str)
    {
      var chunks = str.Split(new[] { "--- !u!" }, StringSplitOptions.None);

      foreach (var chunk in chunks)
      {
        yield return "--- !u!" + chunk.Trim(); // Add "--- !u!" back to the start of each chunk
      }
    }
  }
}
#endif
