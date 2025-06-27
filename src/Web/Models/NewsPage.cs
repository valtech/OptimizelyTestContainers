namespace Optimizely.TestContainers.Models;

[ContentType(
    GUID = "7B873919-11AC-4DF4-B9E8-09F414F76164",
    DisplayName = "News Page")]
public class NewsPage : PageData
{
    public virtual string Title { get; set; }
}