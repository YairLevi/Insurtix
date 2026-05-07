namespace Api.Reports;

public class ReportGeneratorFactory
{
    private readonly IReportGenerator _xml  = new XmlReportGenerator();
    private readonly IReportGenerator _html = new HtmlReportGenerator();

    public IReportGenerator Get(string format) => format.ToLowerInvariant() switch
    {
        "xml"  => _xml,
        "html" => _html,
        _      => throw new ArgumentException($"Unsupported format: {format}")
    };
}
