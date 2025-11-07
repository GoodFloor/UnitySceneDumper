using System.Text;
using UnitySceneDumper;

var sceneObjects = new StreamReader("./../../../SecondScene.unity").ReadToEnd().Split("--- !u!");

// Hierarchy
var transforms = new Dictionary<string, Transform>();
var gameObjectNames = new Dictionary<string, string>();
var roots = new List<string>();

// Scripts usage
var scriptNames = new Dictionary<string, string>();
var scriptUsed = new Dictionary<string, bool>();


// Getting scene hierarchy
for (var i = 1; i < sceneObjects.Length; i++)
{
  List<string> entry;
  // Switch on type of object (we're interested in 4 different types)
  switch (sceneObjects[i][..sceneObjects[i].IndexOf(' ')])
  {
    // SCENE HIERARCHY
    // GameObject - object name
    case "1":
      // Split each property into separate list item
      entry = sceneObjects[i].Split("\n  ").ToList();
      
      // Object id is in the first line after '&'
      var id = entry[0][(entry[0].IndexOf('&') + 1)..entry[0].IndexOf('\n')];
      
      // Look for property defining object's name (assume it is only defined once) 
      entry.Where(e => e.StartsWith("m_Name"))
        .ToList()
        .ForEach(e => gameObjectNames.Add(id, e[(e.IndexOf(':') + 2)..]));
      break;
    
    // Transform - object hierarchy (object's father and children)
    case "4":
      entry = sceneObjects[i].Split("\n  ").ToList();
      var t = new Transform(entry);
      transforms.Add(t.Id, t);
      break;
    
    // SceneRoots - objects without father
    case "1660057539":
      entry = sceneObjects[i].Split("\n  ").ToList();
      var foundRoots = false;
      // Look for m_Roots attribute, then read all of its values
      foreach (var l in entry.TakeWhile(l => !foundRoots || l.StartsWith("- {fileID: ")))
      {
        if (foundRoots)
          roots.Add(l[(l.IndexOf(':') + 2)..(l.IndexOf('}'))]);
        else if (l.StartsWith("m_Roots"))
          foundRoots = true;
      }
      break;
    
    // USED SCRIPTS
    // MonoBehaviour
    case "114":
      entry = sceneObjects[i].Split("\n  ").ToList();
      
      break;
      
  }
}

foreach (var root in roots)
  Console.Write(PrintObject(transforms[root]));
return;

// Function to convert hierarchy tree to text
string PrintObject(Transform transform, int depth = 0)
{
  var sb = new StringBuilder();
  for (var i = 0; i < depth; i++)
    sb.Append("--");
  sb.Append(gameObjectNames[transform.GameObjectId] + "\n");
  foreach (var child in transform.Children)
    sb.Append(PrintObject(transforms[child], depth + 1));
  return sb.ToString();
}

