using System;
using System.Threading;
using System.Threading.Tasks;
using Veritheia.Core.ValueObjects;
using Veritheia.Data.Entities;

namespace Veritheia.Core.Interfaces;

/// <summary>
/// Interface for all analytical processes
/// Processes operate within journey projection spaces
/// </summary>
public interface IAnalyticalProcess
{
    /// <summary>
    /// Unique identifier for this process type
    /// </summary>
    string ProcessId { get; }
    
    /// <summary>
    /// Display name for the process
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Description of what this process does
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Category for organization
    /// </summary>
    string Category { get; }
    
    /// <summary>
    /// Define input requirements for this process
    /// </summary>
    InputDefinition GetInputDefinition();
    
    /// <summary>
    /// Validate that required inputs are present and valid
    /// </summary>
    bool ValidateInputs(ProcessContext context);
    
    /// <summary>
    /// Execute the process within a journey's projection space
    /// </summary>
    Task<ProcessResult> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get process capabilities for registration
    /// </summary>
    ProcessCapability GetCapabilities();
}