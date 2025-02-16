using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;

namespace CoverMe.Backend.Core.Services.Abstractions;

public interface ICoverageService
{
    Task<List<Project>> GetTestsProjects(Solution solution);
    Task<List<CoverageNode>> RunCoverage(Solution solution, Project project, CoverageOptions? options = null);
    Task<List<CoverageNode>> ParseLastCoverage(Solution solution, CoverageOptions? options = null);
    Task<bool?> IsLineCovered(string projectRootPath, string filePath, int line);
    Task<string> GenerateReport(Solution solution, bool detailed = false);
}