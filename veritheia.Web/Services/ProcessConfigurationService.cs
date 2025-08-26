using Veritheia.Data.ViewModels;

namespace veritheia.Web.Services;

/// <summary>
/// Service for managing process configuration
/// </summary>
public class ProcessConfigurationService
{
    /// <summary>
    /// Get available process options
    /// </summary>
    public List<ProcessOptionView> GetAvailableProcesses()
    {
        return new List<ProcessOptionView>
        {
            new ProcessOptionView
            {
                Type = "systematic-screening",
                DisplayName = "Systematic Literature Screening",
                Icon = "📊",
                Description = "LLAssist methodology for systematic literature review with dual assessment (relevance + contribution)",
                Requirements = "Research questions and CSV file with academic papers"
            },
            new ProcessOptionView
            {
                Type = "basic-constrained-composition",
                DisplayName = "Constrained Composition",
                Icon = "✍️",
                Description = "Pedagogical formation with structured writing frameworks and assessment rubrics",
                Requirements = "Learning objectives, constraints, and document templates"
            }
        };
    }

    /// <summary>
    /// Get process option by type
    /// </summary>
    public ProcessOptionView? GetProcessOption(string processType)
    {
        return GetAvailableProcesses().FirstOrDefault(p => p.Type == processType);
    }
}
