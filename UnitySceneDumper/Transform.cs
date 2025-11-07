namespace UnitySceneDumper;

public class Transform
{
    public string Id { get; }
    public string GameObjectId { get; }
    public string FatherId { get; }
    public List<string> Children { get; }

    public Transform(string[] source, int lineNumber)
    {
        Children = [];
        if (source[lineNumber] != "Transform:")
            throw new ArgumentException($"Invalid Transform source line {lineNumber}");
        Id = source[lineNumber - 1].Split('&')[1];
        var g = "";
        var f = "";
        var readingChildren = false;
        for (var i = lineNumber + 1; i < source.Length; i++)
        {
            var line = source[i];
            if (!line.StartsWith(' '))
                break;

            switch (readingChildren)
            {
                case true when !line.StartsWith("  -"):
                    readingChildren = false;
                    break;
                case true:
                    Children.Add(line.Split(':')[1].Trim('}', ' '));
                    continue;
            }

            if (line.StartsWith("  m_GameObject:"))
                g = line.Split(':')[2].Trim('}', ' ');
            else if (line.StartsWith("  m_Father:"))
                f = line.Split(':')[2].Trim('}', ' ');
            else if (line.StartsWith("  m_Children:"))
                readingChildren = true;
        }
        if (g == "" || f == "")
            throw new ArgumentException($"Invalid Transform source line {lineNumber} (not enough attributes)");
        GameObjectId = g;
        FatherId = f;
    }

    public override string ToString()
    {
        return $"Transform {Id}:\n\tGameObject: {GameObjectId}\n\tFather: {FatherId}\n\tChildren: {Children.Count}";
    }

}