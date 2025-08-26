namespace Veritheia.Core.Exceptions;

/// <summary>
/// Base exception for cognitive service failures that affect formation processes.
/// These exceptions indicate critical failures where neural processing cannot occur,
/// requiring immediate user notification and system halt to maintain data integrity.
/// </summary>
public abstract class CognitiveServiceException : Exception
{
    /// <summary>
    /// The user ID associated with the formation process that failed
    /// </summary>
    public Guid? UserId { get; }
    
    /// <summary>
    /// The journey ID where the failure occurred, if applicable
    /// </summary>
    public Guid? JourneyId { get; }
    
    /// <summary>
    /// The specific operation that failed
    /// </summary>
    public string Operation { get; }
    
    /// <summary>
    /// Impact on formation process
    /// </summary>
    public string FormationImpact { get; }
    
    protected CognitiveServiceException(
        string message, 
        string operation,
        string formationImpact,
        Guid? userId = null, 
        Guid? journeyId = null, 
        Exception? innerException = null) 
        : base(message, innerException)
    {
        Operation = operation;
        FormationImpact = formationImpact;
        UserId = userId;
        JourneyId = journeyId;
    }
}

/// <summary>
/// Exception thrown when embedding generation fails.
/// This prevents fake embeddings from corrupting the vector space and formation process.
/// </summary>
public class EmbeddingGenerationException : CognitiveServiceException
{
    public EmbeddingGenerationException(
        string text,
        string reason,
        Guid? userId = null,
        Guid? journeyId = null,
        Exception? innerException = null)
        : base(
            message: $"Neural embedding generation failed: {reason}",
            operation: "Embedding Generation",
            formationImpact: "Document processing halted. Vector space integrity maintained by preventing fake embeddings.",
            userId: userId,
            journeyId: journeyId,
            innerException: innerException)
    {
        Data["TextLength"] = text.Length;
        Data["TextPreview"] = text.Length > 100 ? text[..100] + "..." : text;
        Data["ActionRequired"] = "Ensure LLM service is running and accessible on the configured endpoint.";
    }
}

/// <summary>
/// Exception thrown when text generation fails.
/// This prevents error messages from being treated as successful neural responses.
/// </summary>
public class TextGenerationException : CognitiveServiceException
{
    public TextGenerationException(
        string prompt,
        string reason,
        Guid? userId = null,
        Guid? journeyId = null,
        Exception? innerException = null)
        : base(
            message: $"Neural text generation failed: {reason}",
            operation: "Text Generation",
            formationImpact: "Assessment or analysis halted. Formation integrity maintained by preventing fake neural responses.",
            userId: userId,
            journeyId: journeyId,
            innerException: innerException)
    {
        Data["PromptLength"] = prompt.Length;
        Data["PromptPreview"] = prompt.Length > 100 ? prompt[..100] + "..." : prompt;
        Data["ActionRequired"] = "Verify LLM service is running and accessible on the configured endpoint.";
    }
}