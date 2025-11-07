namespace UnitySceneDumper;

public class Transform
{
    public string Id { get; }
    public string GameObjectId { get; }
    public string FatherId { get; }
    public List<string> Children { get; }

    public Transform(List<string> source)
    {
        Children = [];
        if (!source[0].StartsWith("4 "))
            throw new ArgumentException("Invalid Transform source");
        Id = source[0][(source[0].IndexOf('&') + 1)..source[0].IndexOf('\n')];
        var g = "";
        var f = "";
        var readingChildren = false;
        for (var i = 2; i < source.Count; i++)
        {
            var line = source[i];

            switch (readingChildren)
            {
                case true when !line.StartsWith('-'):
                    readingChildren = false;
                    break;
                case true:
                    Children.Add(line.Split(':')[1].Trim('}', ' '));
                    continue;
            }

            if (line.StartsWith("m_GameObject:"))
                g = line.Split(':')[2].Trim('}', ' ');
            else if (line.StartsWith("m_Father:"))
                f = line.Split(':')[2].Trim('}', ' ');
            else if (line.StartsWith("m_Children:"))
                readingChildren = true;
        }
        if (g == "" || f == "")
            throw new ArgumentException($"Invalid Transform source (not enough attributes)");
        GameObjectId = g;
        FatherId = f;
    }

    public override string ToString()
    {
        return $"\nTransform {Id}:\n\tGameObject: {GameObjectId}\n\tFather: {FatherId}\n\tChildren: {Children.Count}";
    }

}