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
  public class PrefabRawViewMonoOnly : PrefabRawDataWindow
  {
    private HashSet<string> allowedTypes = new HashSet<string> { "MonoBehaviour" };

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Prefab Tools/Raw View (MonoBehaviour Only)")]
    public static void ShowWindow()
    {
      // Show existing window instance. If one doesn't exist, make one.
      GetWindow<PrefabRawViewMonoOnly>("Prefab Raw View (MonoBehaviour Only)");
    }

    protected override void UpdateScrollView()
    {
      // Call the base method to clear the scrollView and load the prefab data
      base.UpdateScrollView();

      // find the containerTypeLabels element
      var containerTypeLabels = rootVisualElement.Query<VisualElement>().Class("containerTypeLabels").First();

      // clear the content of the containerTypeLabels element
      containerTypeLabels.Clear();

      // create a new label and add it to the containerTypeLabels element
      var label = new Label("Only MonoBehaviours are shown");

      // add the label to the containerTypeLabels element
      containerTypeLabels.Add(label);

      // Get the scrollView from the base class
      var scrollView = GetScrollView();

      // Filter the scrollView's children
      for (int i = scrollView.childCount - 1; i >= 0; i--)
      {
        var child = scrollView[i];
        var textField = child as TextField;
        if (textField != null)
        {
          var lines = textField.value.Split(new[] { '\n' }, StringSplitOptions.None);
          if (lines.Length > 1)
          {
            var typeLine = lines[1];
            var typeEndIndex = typeLine.IndexOf(':');
            if (typeEndIndex != -1)
            {
              var type = typeLine.Substring(0, typeEndIndex).Trim();
              if (!allowedTypes.Contains(type))
              {
                scrollView.Remove(child);
              }
            }
          }
        }
      }
    }

    // Method to get the scrollView
    private ScrollView GetScrollView()
    {
      // Query the rootVisualElement for the ScrollView by its class name
      var scrollView = rootVisualElement.Query<ScrollView>().Class("PrefabRawDataScrollView").First();

      return scrollView;
    }
  }
}
#endif
