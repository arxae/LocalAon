namespace LocalAon.Models;

public interface IPageModel
{
    int Id { get; set; }
    string Url { get; set; }
    int ProductId { get; set; }

    string Name { get; set; }
    string Source { get; set; }
}
