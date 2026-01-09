public interface IHelperService { string GetGuidFromHelper(); }

public class HelperService : IHelperService
{
    private readonly IGuidService _guidService;
    public HelperService(IGuidService guidService) => _guidService = guidService;

    public string GetGuidFromHelper() => _guidService.GetGuid();
}