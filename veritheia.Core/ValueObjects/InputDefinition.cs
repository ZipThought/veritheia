namespace Veritheia.Core.ValueObjects;

/// <summary>
/// Defines input requirements for a process
/// </summary>
public class InputDefinition
{
    /// <summary>
    /// Collection of input fields
    /// </summary>
    public List<InputField> Fields { get; set; } = new();
    
    /// <summary>
    /// Add a text area input
    /// </summary>
    public InputDefinition AddTextArea(string name, string description, bool required = true)
    {
        Fields.Add(new InputField
        {
            Name = name,
            Description = description,
            Type = InputFieldType.TextArea,
            Required = required
        });
        return this;
    }
    
    /// <summary>
    /// Add a single-line text input
    /// </summary>
    public InputDefinition AddTextInput(string name, string description, bool required = true)
    {
        Fields.Add(new InputField
        {
            Name = name,
            Description = description,
            Type = InputFieldType.TextInput,
            Required = required
        });
        return this;
    }
    
    /// <summary>
    /// Add a dropdown selection
    /// </summary>
    public InputDefinition AddDropdown(string name, string description, string[] options, bool required = true)
    {
        Fields.Add(new InputField
        {
            Name = name,
            Description = description,
            Type = InputFieldType.Dropdown,
            Options = options.ToList(),
            Required = required
        });
        return this;
    }
    
    /// <summary>
    /// Add a knowledge scope selector
    /// </summary>
    public InputDefinition AddScopeSelector(string name, string description, bool required = false)
    {
        Fields.Add(new InputField
        {
            Name = name,
            Description = description,
            Type = InputFieldType.ScopeSelector,
            Required = required
        });
        return this;
    }
    
    /// <summary>
    /// Add a document selector
    /// </summary>
    public InputDefinition AddDocumentSelector(string name, string description, bool required = true)
    {
        Fields.Add(new InputField
        {
            Name = name,
            Description = description,
            Type = InputFieldType.DocumentSelector,
            Required = required
        });
        return this;
    }
    
    /// <summary>
    /// Add a multi-select field
    /// </summary>
    public InputDefinition AddMultiSelect(string name, string description, string[] options, bool required = true)
    {
        Fields.Add(new InputField
        {
            Name = name,
            Description = description,
            Type = InputFieldType.MultiSelect,
            Options = options.ToList(),
            Required = required
        });
        return this;
    }
}

/// <summary>
/// Individual input field definition
/// </summary>
public class InputField
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InputFieldType Type { get; set; }
    public bool Required { get; set; }
    public List<string> Options { get; set; } = new();
    public object? DefaultValue { get; set; }
}

/// <summary>
/// Types of input fields
/// </summary>
public enum InputFieldType
{
    TextInput,
    TextArea,
    Dropdown,
    MultiSelect,
    ScopeSelector,
    DocumentSelector,
    NumberInput,
    DatePicker
}