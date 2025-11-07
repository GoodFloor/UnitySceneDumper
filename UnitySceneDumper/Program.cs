using System.Text;
using UnitySceneDumper;


if(args.Length != 2)
{
  Console.WriteLine("Incorrect number of arguments");
  return;
}

// Check for source directory

var assetsPath = Path.Join(args[0], "Assets");
Console.WriteLine(assetsPath);
if (!Directory.Exists(assetsPath))
{
  Console.WriteLine($"Directory {assetsPath} does not exist");
  return;
}
// Check (and create) target directory
if (!Directory.Exists(args[1]))
  Directory.CreateDirectory(args[1]);

// Hierarchy variables
var transforms = new Dictionary<string, Transform>();
var gameObjectNames = new Dictionary<string, string>();
var roots = new List<string>();

// Scripts usage variables
var scriptNames = new Dictionary<string, string>();
var scriptsUsed = new HashSet<string>();

// Found files data
var scenesPath = Path.Join(assetsPath, "Scenes");
var scriptsPath = Path.Join(assetsPath, "Scripts");
var sceneFiles = Directory.GetFiles(scenesPath, "*.unity");
var scriptFiles = new List<string>();
var scriptSubfolders = new List<string>();
GetScriptsData(scriptsPath);

// Get all script names and guids (ignore meta files of directories)
foreach (var scriptFile in scriptFiles.Where(
           s => !scriptSubfolders.Contains(Path.Join(Path.GetDirectoryName(s), Path.GetFileNameWithoutExtension(s)))))
{
  var lines = File.ReadAllLines(scriptFile);
  foreach (var line in lines.Where(l => l.StartsWith("guid")))
  {
    scriptNames.Add(line[6..], Path.Join(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile)));
    break;
  }
}

// Get all necessary data from scene files
foreach (var file in sceneFiles)
{
  CollectData(file);
  var outputFile = Path.Join(args[1], Path.GetFileName(file) + ".dump");
  using (var writer = new StreamWriter(outputFile))
  {
    foreach (var root in roots)
      writer.Write(PrintObject(transforms[root]));
  }
  transforms.Clear();
  gameObjectNames.Clear();
  roots.Clear();
}

using (var writer = new StreamWriter(Path.Join(args[1], "UnusedScripts.csv")))
{
  writer.WriteLine("Relative Path,GUID");
  foreach (var script in scriptNames)
    if (!scriptsUsed.Contains(script.Key))
      writer.WriteLine($"{Path.GetRelativePath(args[0], script.Value)},{script.Key}");
}

return;



void GetScriptsData(string path)
{
  scriptFiles.AddRange(Directory.GetFiles($"{path}", "*.meta"));
  foreach (var subdirectory in Directory.GetDirectories(path))
  {
    GetScriptsData(subdirectory);
    scriptSubfolders.Add(subdirectory);
  }
}

// Function that collects all the necessary data from a single scene file
void CollectData(string sourceFile)
{
  var sceneObjects = new StreamReader(sourceFile).ReadToEnd().Split("--- !u!");
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
        var scriptData = "";
        
        // Look for line with data about used script
        foreach (var attribute in entry.Where(a => a.StartsWith("m_Script")))
        {
          scriptData = attribute;
          break;
        }

        if (scriptData == "")
          throw new ArgumentException("Script data is incomplete (or different than expected)");
        // Look for script guid
        foreach (var guid in scriptData.Split(", ").ToList().Where(a => a.StartsWith("guid")))
        {
          scriptsUsed.Add(guid[6..]);
          break;
        }
        break;
    }
  }
}


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
