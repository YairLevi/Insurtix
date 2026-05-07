using Api.Reports;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReportsController(IBooksService service, ReportGeneratorFactory factory) : ControllerBase
{
    [HttpGet("{format}")]
    public IActionResult Generate(string format)
    {
        IReportGenerator generator;
        try { generator = factory.Get(format); }
        catch (ArgumentException) { return BadRequest($"Unsupported format: {format}"); }

        var content = generator.Generate(service.GetAll());
        return File(Encoding.UTF8.GetBytes(content), generator.ContentType, generator.FileName);
    }
}
