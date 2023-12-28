using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace uk.novavoidhowl.dev.unitydebugtools
{
  public class ListInstalledPackages : EditorWindow
  {
    private ScrollView scrollView;
    private ListRequest Request;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/UPM Tools/List Installed Packages")]
    public static void ShowWindow()
    {
      GetWindow<ListInstalledPackages>("List Installed Packages");
    }

    public void OnEnable()
    {
      RenderUI();
      Request = UnityEditor.PackageManager.Client.List(true);
      EditorApplication.update += Progress;
    }

    private void Progress()
    {
      if (Request.IsCompleted)
      {
        if (Request.Status == StatusCode.Success)
        {
          foreach (var package in Request.Result)
          {
            AddPackageToScrollView(package.name, package.version);
          }
        }
        else if (Request.Status >= StatusCode.Failure)
        {
          Debug.Log(Request.Error.message);
        }

        EditorApplication.update -= Progress;
      }
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