using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests; 
using System.Collections.Generic;

namespace uk.novavoidhowl.dev.unitydebugtools
{
  public class ListManifestPackages : EditorWindow
  {
    private ScrollView scrollView;
    private ListRequest Request;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/UPM Tools/List Manifest Packages")]
    public static void ShowWindow()
    {
      GetWindow<ListManifestPackages>("List Manifest Packages");
    }

    public void OnEnable()
    {
      RenderUI();
      Request = UnityEditor.PackageManager.Client.List(true);
      EditorApplication.update += Progress;
    }

    private void Progress()
    {
      if (Request == null)
      {
        return;
      }

      if (Request.IsCompleted)
      {
        if (Request.Status == StatusCode.Success)
        {
          string path = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
          if (File.Exists(path))
          {
            string json = File.ReadAllText(path);
            var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(json);
            if (manifest != null)
            {
              if (manifest.dependencies != null)
              {
                Debug.Log("Number of dependencies: " + manifest.dependencies.Count);
                foreach (var package in manifest.dependencies)
                {
                  Debug.Log("Adding package: " + package.Key);
                  AddPackageToScrollView(package.Key, package.Value);
                }
              }
              else
              {
                Debug.Log("Dependencies is null");
              }
            }
            else
            {
              Debug.Log("Manifest is null");
            }
          }
          else
          {
            Debug.Log("Manifest file does not exist");
          }
        }
        else if (Request.Status >= StatusCode.Failure)
        {
          Debug.Log(Request.Error.message);
        }

        EditorApplication.update -= Progress;
      }
    }

    [Serializable]
    public class Manifest
    {
      public Dictionary<string, string> dependencies;
    }

    private void AddPackageToScrollView(string packageName, string packageVersion)
    {
      // Create a new VisualElement for each package
      var packageElement = new VisualElement();
      packageElement.style.flexDirection = FlexDirection.Row;
      packageElement.style.borderTopWidth = 1;
      packageElement.style.borderBottomWidth = 1;
      var borderColor = new Color(0.8f, 0.8f, 0.8f);
      packageElement.style.borderBottomColor = borderColor;
      packageElement.style.marginTop = 5; // Add a small gap between items
      packageElement.style.paddingLeft = 10; // Add 10px padding to the left
      packageElement.style.paddingRight = 10; // Add 10px padding to the right

      // Create a Label for the package name and add it to the packageElement
      var nameLabel = new Label(packageName);
      nameLabel.style.flexGrow = 1;
      nameLabel.style.fontSize = 14; // Increase the font size
      packageElement.Add(nameLabel);

      // Create a Label for the package version and add it to the packageElement
      var versionLabel = new Label(packageVersion);
      versionLabel.style.flexGrow = 1;
      versionLabel.style.unityTextAlign = TextAnchor.MiddleRight;
      versionLabel.style.fontSize = 14; // Increase the font size
      packageElement.Add(versionLabel);

      // Add the packageElement to the scrollView
      scrollView.Add(packageElement);
    }

    private void RenderUI()
    {
      var root = rootVisualElement;
      root.Clear();

      scrollView = new ScrollView();
      root.Add(scrollView);
    }
  }
}