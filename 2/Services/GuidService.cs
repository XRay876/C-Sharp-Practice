public interface IGuidService { string GetGuid(); }

public class GuidService : IGuidService
{
    private readonly string _id;
    public GuidService() => _id = Guid.NewGuid().ToString()[..8]; 
    public string GetGuid() => _id;
}