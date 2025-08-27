using Veritheia.Data.Services;
using Xunit;

namespace Veritheia.Tests.Unit.Services;

/// <summary>
/// Test suite for OrthogonalTransformationService
/// Verifies mathematical properties of orthogonal transformations
/// </summary>
public class OrthogonalTransformationServiceTests
{
    private readonly OrthogonalTransformationService _service = new();
    
    [Fact]
    public void TransformVectorForUser_PreservesVectorNorm()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vector = GenerateRandomVector(1536);
        var originalNorm = CalculateNorm(vector);
        
        // Act
        var transformed = _service.TransformVectorForUser(userId, vector);
        var transformedNorm = CalculateNorm(transformed);
        
        // Assert - Orthogonal transformation preserves norm
        Assert.Equal(originalNorm, transformedNorm, precision: 5);
    }
    
    [Fact]
    public void TransformVectorForUser_PreservesDistanceBetweenVectors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vector1 = GenerateRandomVector(768);
        var vector2 = GenerateRandomVector(768);
        
        // Act & Assert
        var preservesDistance = _service.VerifyOrthogonalTransformation(userId, vector1, vector2);
        Assert.True(preservesDistance);
    }
    
    [Fact]
    public void TransformVectorForUser_PreservesAngles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vector1 = GenerateRandomVector(384);
        var vector2 = GenerateRandomVector(384);
        
        var originalCosine = CalculateCosineDistance(vector1, vector2);
        
        // Act
        var transformed1 = _service.TransformVectorForUser(userId, vector1);
        var transformed2 = _service.TransformVectorForUser(userId, vector2);
        var transformedCosine = CalculateCosineDistance(transformed1, transformed2);
        
        // Assert - Cosine similarity preserved
        Assert.Equal(originalCosine, transformedCosine, precision: 5);
    }
    
    [Fact]
    public void TransformVectorForUser_DeterministicForSameUser()
    {
        // Arrange
        var userId = Guid.Parse("12345678-1234-5678-1234-567812345678");
        var vector = GenerateRandomVector(1536);
        
        // Act - Transform same vector multiple times
        var result1 = _service.TransformVectorForUser(userId, vector);
        var result2 = _service.TransformVectorForUser(userId, vector);
        var result3 = _service.TransformVectorForUser(userId, vector);
        
        // Assert - All results identical
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }
    
    [Fact]
    public void TransformVectorForUser_DifferentForDifferentUsers()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var vector = GenerateRandomVector(768);
        
        // Act
        var result1 = _service.TransformVectorForUser(user1, vector);
        var result2 = _service.TransformVectorForUser(user2, vector);
        
        // Assert - Results are different
        Assert.NotEqual(result1, result2);
        
        // But both preserve the norm
        var originalNorm = CalculateNorm(vector);
        Assert.Equal(originalNorm, CalculateNorm(result1), precision: 5);
        Assert.Equal(originalNorm, CalculateNorm(result2), precision: 5);
    }
    
    [Theory]
    [InlineData(384)]   // Compact models
    [InlineData(768)]   // Medium models (E5, BGE)
    [InlineData(1536)]  // Large models (OpenAI)
    public void TransformVectorForUser_WorksWithStandardDimensions(int dimension)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vector = GenerateRandomVector(dimension);
        
        // Act
        var transformed = _service.TransformVectorForUser(userId, vector);
        
        // Assert
        Assert.Equal(dimension, transformed.Length);
        Assert.NotEqual(vector, transformed); // Transformed is different
        Assert.Equal(CalculateNorm(vector), CalculateNorm(transformed), precision: 5);
    }
    
    [Fact]
    public void TransformVectorForUser_CreatesIncommensurableSpaces()
    {
        // Arrange - Two users with identical content vectors
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var identicalVector = GenerateRandomVector(1536);
        
        // Act
        var user1Transformed = _service.TransformVectorForUser(user1, identicalVector);
        var user2Transformed = _service.TransformVectorForUser(user2, identicalVector);
        
        // Calculate "similarity" between transformed vectors
        var crossUserSimilarity = CalculateCosineDistance(user1Transformed, user2Transformed);
        
        // Assert - Transformed vectors should have very low similarity
        // In high dimensions, random orthogonal transformations create nearly orthogonal results
        Assert.True(Math.Abs(crossUserSimilarity) < 0.1, 
            $"Cross-user similarity {crossUserSimilarity} too high - spaces not sufficiently isolated");
    }
    
    [Fact]
    public void TransformVectorForUser_HandlesEdgeCases()
    {
        // Test with zero vector
        var userId = Guid.NewGuid();
        var zeroVector = new float[768];
        var transformed = _service.TransformVectorForUser(userId, zeroVector);
        Assert.All(transformed, v => Assert.Equal(0, v));
        
        // Test with unit vector
        var unitVector = new float[768];
        unitVector[0] = 1.0f;
        var transformedUnit = _service.TransformVectorForUser(userId, unitVector);
        Assert.Equal(1.0f, CalculateNorm(transformedUnit), precision: 5);
    }
    
    [Fact]
    public void TransformVectorForUser_ThrowsOnInvalidInput()
    {
        var userId = Guid.NewGuid();
        
        // Null vector
        Assert.Throws<ArgumentException>(() => 
            _service.TransformVectorForUser(userId, null!));
        
        // Empty vector
        Assert.Throws<ArgumentException>(() => 
            _service.TransformVectorForUser(userId, Array.Empty<float>()));
    }
    
    [Fact]
    public void TransformVectorForUser_PermutationIsValid()
    {
        // This test verifies the permutation contains each index exactly once
        var userId = Guid.NewGuid();
        var vector = Enumerable.Range(0, 100).Select(i => (float)i).ToArray();
        
        var transformed = _service.TransformVectorForUser(userId, vector);
        
        // Check all values are present (possibly negated)
        var absoluteValues = transformed.Select(Math.Abs).OrderBy(v => v).ToArray();
        var expectedValues = vector.OrderBy(v => v).ToArray();
        
        Assert.Equal(expectedValues, absoluteValues);
    }
    
    // Helper methods
    
    private static float[] GenerateRandomVector(int dimension)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var vector = new float[dimension];
        for (int i = 0; i < dimension; i++)
        {
            vector[i] = (float)(random.NextDouble() - 0.5) * 2; // Range [-1, 1]
        }
        return vector;
    }
    
    private static float CalculateNorm(float[] vector)
    {
        return (float)Math.Sqrt(vector.Sum(v => v * v));
    }
    
    private static float CalculateCosineDistance(float[] a, float[] b)
    {
        var dotProduct = a.Zip(b, (x, y) => x * y).Sum();
        var normA = CalculateNorm(a);
        var normB = CalculateNorm(b);
        return dotProduct / (normA * normB);
    }
}