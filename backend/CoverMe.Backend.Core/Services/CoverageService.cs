﻿using System.IO.Abstractions;
using System.Net;
using System.Text.RegularExpressions;
using CoverMe.Backend.Core.Enums.Coverage;
using CoverMe.Backend.Core.Enums.Process;
using CoverMe.Backend.Core.Extensions;
using CoverMe.Backend.Core.Helpers.Abstractions;
using CoverMe.Backend.Core.Managers.Abstractions;
using CoverMe.Backend.Core.Models;
using CoverMe.Backend.Core.Models.Coverage;
using CoverMe.Backend.Core.Models.Process;
using CoverMe.Backend.Core.Services.Abstractions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace CoverMe.Backend.Core.Services;

public partial class CoverageService : ICoverageService
{
    #region Constants

    private const string ReportsFolderName = "reports";

    [GeneratedRegex(@"\b(?:\w+\.)+(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CleanTypeRegex();

    private static readonly Regex RegCleanType = CleanTypeRegex();

    [GeneratedRegex("\\.[c]*ctor")]
    private static partial Regex RegexMethodConstructor();

    #endregion

    #region Members

    private readonly ICacheManager<Dictionary<int, bool?>?> _linesCoverageCache;
    private readonly ICacheManager<List<CoverageNode>> _coverageTreeCache;
    private readonly ILogger<CoverageService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IProcessHelper _processHelper;
    private readonly ISettingsService _settingsService;

    #endregion

    #region Constructors

    public CoverageService(
        ILogger<CoverageService> logger,
        IFileSystem fileSystem,
        ICacheManager<Dictionary<int, bool?>?> linesCoverageCache,
        ICacheManager<List<CoverageNode>> coverageTreeCache,
        IProcessHelper processHelper,
        ISettingsService settingsService
    )
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _linesCoverageCache = linesCoverageCache;
        _coverageTreeCache = coverageTreeCache;
        _processHelper = processHelper;
        _settingsService = settingsService;
    }

    #endregion

    #region Public methods

    public async Task<List<Project>> GetTestsProjects(Solution solution)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            var files = _fileSystem.Directory.EnumerateFiles(
                solution.FolderPath,
                settings.Coverage.ProjectsFilter,
                SearchOption.AllDirectories
            );

            return files.Select(e => new Project(e))
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get tests projects");
        }

        return [];
    }

    public async Task<List<CoverageNode>> RunCoverage(
        Solution solution,
        Project project,
        CoverageOptions? options = null
    )
    {
        options ??= new CoverageOptions();

        var outputFolder = Path.Join(solution.IdeaPluginFolder, ReportsFolderName);
        if (!_fileSystem.Directory.Exists(outputFolder))
        {
            _fileSystem.Directory.CreateDirectory(outputFolder);
        }

        var reportFilePath = Path.Join(
            outputFolder,
            $"{Guid.NewGuid():N}.xml"
        );

        var settings = await _settingsService.GetSettingsAsync();

        var response = await _processHelper.DotCoverCli(new DotCoverCliOptions
        {
            Command = DotCoverCommand.CoverDotnet,
            ReportType = DotCoverReportType.DetailedXml,
            HideAutoProperties = settings.Coverage.HideAutoProperties,
            NoBuild = !options.Rebuild,
            OutputPath = reportFilePath,
            ProjectFolderPath = project.FolderPath,
            CoverageFilter = settings.Coverage.CoverageFilter,
            TestsFilter = settings.Coverage.TestsFilter,
        });
        if (response.ExitCode != 0) return [];

        _coverageTreeCache.RemoveAll();
        _linesCoverageCache.RemoveAll();
        return await GetCoverageTree(reportFilePath, options);
    }

    public async Task<List<CoverageNode>> ParseLastCoverage(Solution solution, CoverageOptions? options = null)
    {
        var filePath = GetLastCoverageFilePath(solution);
        if (string.IsNullOrEmpty(filePath)) return [];

        options ??= new CoverageOptions();
        return await GetCoverageTree(filePath, options);
    }

    public async Task<bool?> IsLineCovered(string projectRootPath, string filePath, int line)
    {
        Solution solution = new(_fileSystem, projectRootPath);

        var windowsFilePath = filePath.ConvertPathToWindows();
        var cache = await _linesCoverageCache.GetOrSetAsync(
            windowsFilePath,
            () => GetLinesCoverage(solution, windowsFilePath)
        );

        return cache is null ? null : cache.TryGetValue(line, out var isLineCovered) ? isLineCovered : null;
    }

    public Task<string> GenerateReport(Solution solution, bool detailed = false)
    {
        return detailed ? GenerateDetailedReport(solution) : GenerateSimpleReport(solution);
    }

    #endregion

    #region Private methods

    private async Task<string> GenerateDetailedReport(Solution solution)
    {
        var lastReportFilePath = GetLastCoverageFilePath(solution);
        if (string.IsNullOrEmpty(lastReportFilePath)) return string.Empty;

        var tempFolder = Path.Join(solution.IdeaPluginFolder, "temp", Guid.NewGuid().ToString());
        if (_fileSystem.Directory.Exists(tempFolder))
        {
            _fileSystem.Directory.Delete(tempFolder, true);
        }

        _fileSystem.Directory.CreateDirectory(tempFolder);

        var response = await _processHelper.ReportGenerator(new ReportGeneratorOptions
        {
            ReportFilePath = lastReportFilePath,
            OutputFolderPath = tempFolder,
            ReportType = ReportGeneratorReportType.Html,
        });
        if (response.ExitCode != 0) return string.Empty;

        var reportFolder = Path.Join(tempFolder, "report");
        if (_fileSystem.Directory.Exists(reportFolder))
        {
            _fileSystem.Directory.Delete(reportFolder, true);
        }

        _fileSystem.Directory.CreateDirectory(reportFolder);

        foreach (var entry in _fileSystem.Directory.EnumerateFiles(tempFolder))
        {
            _fileSystem.File.Move(entry, Path.Join(reportFolder, Path.GetFileName(entry)));
        }

        return tempFolder;
    }

    private async Task<string> GenerateSimpleReport(Solution solution)
    {
        var tempFolder = Path.Join(solution.IdeaPluginFolder, "temp", Guid.NewGuid().ToString());
        if (_fileSystem.Directory.Exists(tempFolder))
        {
            _fileSystem.Directory.Delete(tempFolder, true);
        }

        _fileSystem.Directory.CreateDirectory(tempFolder);

        var reportFilePath = Path.Join(tempFolder, "report.html");

        var response = await _processHelper.DotCoverCli(new DotCoverCliOptions
        {
            Command = DotCoverCommand.CoverDotnet,
            ReportType = DotCoverReportType.Html,
            OutputPath = reportFilePath,
            ProjectFolderPath = solution.FolderPath,
            HideAutoProperties = true,
            NoBuild = true,
        });
        return response.ExitCode != 0 ? string.Empty : tempFolder;
    }

    private async Task<List<CoverageNode>> GetCoverageTree(
        string reportFilePath,
        CoverageOptions options
    )
    {
        return await _coverageTreeCache.GetOrSetAsync(
            reportFilePath,
            () => ParseCoverage(reportFilePath, options)!
        );
    }

    private async Task<Dictionary<int, bool?>?> GetLinesCoverage(Solution solution, string windowsFilePath)
    {
        var lastCoverageFilePath = GetLastCoverageFilePath(solution);
        if (string.IsNullOrEmpty(lastCoverageFilePath)) return null;

        var data = await _fileSystem.File.ReadAllTextAsync(lastCoverageFilePath);
        if (string.IsNullOrEmpty(data)) return null;

        var xml = new HtmlDocument();
        xml.LoadHtml(data);

        var files = xml.DocumentNode.SelectNodes("//file");
        if (files is null) return null;

        var statements = xml.DocumentNode.SelectNodes("//statement");
        if (statements is null) return null;

        var lineStatus = new Dictionary<int, bool?>();

        foreach (var statement in statements)
        {
            var fileIndex = statement.GetAttributeValue("FileIndex", 0);
            var file = files.FirstOrDefault(e => e.GetAttributeValue("Index", 0) == fileIndex);
            if (file is null) continue;

            var fileFilePath = file.GetAttributeValue("Name", string.Empty)
                .ConvertPathToWindows();
            if (string.IsNullOrEmpty(fileFilePath)) continue;
            if (fileFilePath != windowsFilePath) continue;

            var statementLine = statement.GetAttributeValue("Line", 0);
            if (statementLine <= 0) continue;

            var isCovered = statement.GetAttributeValue<bool?>("Covered", null);
            lineStatus.TryAdd(statementLine, isCovered);
        }

        return lineStatus;
    }

    private string GetLastCoverageFilePath(Solution solution)
    {
        var reportsFolder = Path.Join(
            solution.IdeaPluginFolder,
            ReportsFolderName
        );
        if (!_fileSystem.Directory.Exists(reportsFolder)) return string.Empty;

        var latestFileInfo = _fileSystem.Directory
            .EnumerateFiles(reportsFolder)
            .Select(e => new FileInfo(e))
            .OrderByDescending(e => e.CreationTime)
            .FirstOrDefault();
        if (latestFileInfo is null) return string.Empty;

        return string.IsNullOrEmpty(latestFileInfo.FullName) ? string.Empty : latestFileInfo.FullName;
    }

    private async Task<List<CoverageNode>> ParseCoverage(string filePath, CoverageOptions options)
    {
        var data = await _fileSystem.File.ReadAllTextAsync(filePath);
        if (string.IsNullOrEmpty(data)) return [];

        var xml = new HtmlDocument();
        xml.LoadHtml(data);

        var root = xml.DocumentNode.SelectSingleNode("/root");
        if (root is null) return [];

        List<CoverageNode> nodes =
        [
            new(0, "Solution", CoverageNodeIcon.Solution, CoverageNodeType.Solution)
            {
                Coverage = root.GetAttributeValue<int>("CoveragePercent", 0),
                CoveredStatements = root.GetAttributeValue<int>("CoveredStatements", 0),
                TotalStatements = root.GetAttributeValue<int>("TotalStatements", 0),
            }
        ];

        var files = xml.DocumentNode.SelectNodes("//file");
        if (files is null) return [];

        var fileIndices = files
            .ToDictionary(e =>
                    e.GetAttributeValue<int>("Index", -1),
                e => e.GetAttributeValue<string>("Name", string.Empty)
            );

        var assemblies = root.ChildNodes.Where(e => e.Name == "assembly");

        foreach (var assembly in assemblies)
        {
            ParseAssembly(assembly, nodes, fileIndices, options);
        }

        return nodes;
    }

    private static void ParseAssembly(
        HtmlNode assembly,
        List<CoverageNode> nodes,
        Dictionary<int, string> fileIndices,
        CoverageOptions options,
        int level = 1
    )
    {
        var name = assembly.GetAttributeValue<string>("name", string.Empty);
        if (!Regex.IsMatch(name, options.Filter)) return;

        nodes.Add(new CoverageNode(level, name, CoverageNodeIcon.Assembly, CoverageNodeType.Assembly)
        {
            Coverage = assembly.GetAttributeValue("CoveragePercent", 0),
            CoveredStatements = assembly.GetAttributeValue("CoveredStatements", 0),
            TotalStatements = assembly.GetAttributeValue("TotalStatements", 0),
        });

        var namespaces = assembly.ChildNodes.Where(e => e.Name == "namespace");

        foreach (var @namespace in namespaces)
        {
            ParseNamespace(@namespace, nodes, level + 1, fileIndices, options);
        }

        var types = assembly.ChildNodes.Where(e => e.Name == "type");

        foreach (var type in types)
        {
            ParseType(type, nodes, level + 1, fileIndices, options);
        }
    }

    private static void ParseNamespace(
        HtmlNode @namespace,
        List<CoverageNode> nodes,
        int level,
        Dictionary<int, string> fileIndices,
        CoverageOptions options
    )
    {
        var name = @namespace.GetAttributeValue("name", string.Empty);
        if (!Regex.IsMatch(name, options.Filter)) return;

        nodes.Add(new CoverageNode(level, name, CoverageNodeIcon.Namespace, CoverageNodeType.Namespace)
        {
            Coverage = @namespace.GetAttributeValue("CoveragePercent", 0),
            CoveredStatements = @namespace.GetAttributeValue("CoveredStatements", 0),
            TotalStatements = @namespace.GetAttributeValue("TotalStatements", 0),
        });

        var types = @namespace.ChildNodes.Where(e => e.Name == "type");

        foreach (var type in types)
        {
            ParseType(type, nodes, level + 1, fileIndices, options);
        }
    }

    private static void ParseType(
        HtmlNode type,
        List<CoverageNode> nodes,
        int level,
        Dictionary<int, string> fileIndices,
        CoverageOptions options
    )
    {
        var name = type.GetAttributeValue("name", string.Empty);
        if (!Regex.IsMatch(name, options.Filter)) return;

        var node = new CoverageNode(level, name, CoverageNodeIcon.Type, CoverageNodeType.Type)
        {
            Coverage = type.GetAttributeValue("CoveragePercent", 0),
            CoveredStatements = type.GetAttributeValue("CoveredStatements", 0),
            TotalStatements = type.GetAttributeValue("TotalStatements", 0),
            LineNumber = 1,
        };
        nodes.Add(node);

        var methods = type.ChildNodes.Where(e => e.Name == "method");

        foreach (var method in methods)
        {
            node.FilePath = ParseMethod(method, nodes, level + 1, fileIndices);
        }
    }

    private static string ParseMethod(
        HtmlNode method,
        List<CoverageNode> nodes,
        int level,
        Dictionary<int, string> fileIndices
    )
    {
        var name = MapEncodedCharacters(method.GetAttributeValue("name", string.Empty));
        var methodName = ParseMethodName(name);
        var arguments = ParseArguments(name);
        var returnTypeIndex = name.LastIndexOf(':');
        var returnType = returnTypeIndex < 0
            ? string.Empty
            : RegCleanType.Replace(name[(returnTypeIndex + 1)..], "$1");

        var firstStatement = method.ChildNodes
            .FirstOrDefault(e => e.Name == "statement");
        var filePath = string.Empty;
        var lineNumber = 0;

        if (firstStatement is not null)
        {
            var fileIndex = firstStatement.GetAttributeValue("FileIndex", 0);
            filePath = fileIndices.TryGetValue(fileIndex, out var path) ? path : string.Empty;
            lineNumber = firstStatement.GetAttributeValue("Line", 0) - 1;
        }

        nodes.Add(
            new CoverageNode(
                level,
                $"{methodName}({arguments}){(string.IsNullOrEmpty(returnType) ? string.Empty : $": {returnType}")}",
                CoverageNodeIcon.Method,
                CoverageNodeType.MethodPropertyField
            )
            {
                Coverage = method.GetAttributeValue("CoveragePercent", 0),
                CoveredStatements = method.GetAttributeValue("CoveredStatements", 0),
                TotalStatements = method.GetAttributeValue("TotalStatements", 0),
                FilePath = filePath,
                LineNumber = lineNumber,
            });

        return filePath;
    }

    private static string ParseMethodName(string name)
    {
        var nameIndex = name.IndexOf('(');
        var methodName = nameIndex < 0 ? name : name[..nameIndex];
        return RegexMethodConstructor().Replace(methodName, "ctor");
    }

    private static string ParseArguments(string name)
    {
        var openParenthesisIndex = name.IndexOf('(');
        if (openParenthesisIndex < 0) return string.Empty;

        var closeParenthesisIndex = name.LastIndexOf(')');
        if (closeParenthesisIndex < 0 || closeParenthesisIndex <= openParenthesisIndex) return string.Empty;

        var arguments = name[(openParenthesisIndex + 1)..closeParenthesisIndex].Split(',');

        return string.Join(
            ", ",
            arguments.Select(e =>
                RegCleanType.Replace(
                    e,
                    "$1"
                )
            )
        );
    }

    private static string MapEncodedCharacters(string value)
    {
        return WebUtility.HtmlDecode(value);
    }

    #endregion
}