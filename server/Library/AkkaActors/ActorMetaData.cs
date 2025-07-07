namespace Library.AkkaActors;

public class ActorMetaData
{
    private readonly string Root = $"akka://Server/user";
    public ActorMetaData(string name, string parentName)
    {
        _name = name;
        _parentName = $"{Root}/{parentName}";

        if (string.IsNullOrEmpty(parentName))
        {
            _fullPath = $"{Root}/{Name}";
        }
        else
        {
            _fullPath = $"{_parentName}/{Name}";
        }
    }
    private readonly string _name = string.Empty;
    private readonly string _parentName = string.Empty;
    private readonly string _fullPath = string.Empty;
    public string Name => _name;

    public string Path => _fullPath;

}
