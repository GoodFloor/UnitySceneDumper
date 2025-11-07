using UnitySceneDumper;

var lines = new StreamReader("./../../../SecondScene.unity").ReadToEnd().Split("\n");
var lastLine = lines[0];
// var transforms = new List<Transform>();
var transforms = new Dictionary<string, Transform>();
var gameObjectNames = new Dictionary<string, string>();

for (int i = 1; i < lines.Length; i++)
{
  var line = lines[i];
  switch (line)
  {
    case "Transform:":
      var t = new Transform(lines, i);
      transforms.Add(t.Id, t);
      break;
    case "GameObject:":
      Console.WriteLine("GameObject:");
      break;
    case "SceneRoots:":
      Console.WriteLine("SceneRoots:");
      break;
  }
  lastLine = line;
}
