using System.Security.Cryptography;

namespace Veritheia.Data.Services;

/// <summary>
/// Service for applying orthogonal transformations to vectors for user isolation
/// Creates mathematically distinct parallel universes for each user's vector space
/// </summary>
public class OrthogonalTransformationService
{
    /// <summary>
    /// Applies orthogonal transformation to a vector for user isolation
    /// </summary>
    /// <param name="userId">User identifier for deterministic transformation</param>
    /// <param name="vector">Original vector to transform</param>
    /// <returns>Orthogonally transformed vector</returns>
    public float[] TransformVectorForUser(Guid userId, float[] vector)
    {
        if (vector == null || vector.Length == 0)
            throw new ArgumentException("Vector cannot be null or empty", nameof(vector));

        // Generate deterministic permutation and sign flips
        var permutation = GeneratePermutation(userId, vector.Length);
        var signs = GenerateSignFlips(userId, vector.Length);

        // Apply transformation: permute then flip signs
        var result = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            result[i] = vector[permutation[i]] * signs[i];
        }

        return result;
    }

    /// <summary>
    /// Generates a deterministic permutation from user ID
    /// </summary>
    private int[] GeneratePermutation(Guid userId, int length)
    {
        // Initialize permutation as identity
        var permutation = new int[length];
        for (int i = 0; i < length; i++)
            permutation[i] = i;

        // Use SHA512 for key expansion
        var hash = SHA512.HashData(userId.ToByteArray());
        
        // Fisher-Yates shuffle with deterministic randomness
        using (var hmac = new HMACSHA256(hash[..32])) // Use first 256 bits as key
        {
            var counter = new byte[8];
            
            for (int i = length - 1; i > 0; i--)
            {
                // Generate deterministic random value for this position
                var hashBytes = hmac.ComputeHash(counter);
                var randomValue = BitConverter.ToUInt32(hashBytes, 0);
                
                // Get index in range [0, i]
                int j = (int)(randomValue % (uint)(i + 1));
                
                // Swap
                (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
                
                // Increment counter for next iteration
                IncrementCounter(counter);
            }
        }

        return permutation;
    }

    /// <summary>
    /// Generates deterministic sign flips from user ID
    /// </summary>
    private float[] GenerateSignFlips(Guid userId, int length)
    {
        // Use second half of SHA512 for sign flips
        var hash = SHA512.HashData(userId.ToByteArray());
        var signs = new float[length];
        
        // Generate enough bits for all dimensions
        var bitsNeeded = length;
        var bytesNeeded = (bitsNeeded + 7) / 8;
        var signBytes = new byte[bytesNeeded];
        
        // Expand hash if needed using HMAC
        if (bytesNeeded <= 32)
        {
            Array.Copy(hash, 32, signBytes, 0, bytesNeeded); // Use second half of hash
        }
        else
        {
            // Need more bytes - use HMAC for expansion
            using (var hmac = new HMACSHA256(hash[32..])) // Use second half as key
            {
                var counter = new byte[8];
                var offset = 0;
                
                while (offset < bytesNeeded)
                {
                    var expansion = hmac.ComputeHash(counter);
                    var copyLength = Math.Min(expansion.Length, bytesNeeded - offset);
                    Array.Copy(expansion, 0, signBytes, offset, copyLength);
                    offset += copyLength;
                    IncrementCounter(counter);
                }
            }
        }
        
        // Convert bits to signs
        for (int i = 0; i < length; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;
            bool isNegative = (signBytes[byteIndex] & (1 << bitIndex)) != 0;
            signs[i] = isNegative ? -1.0f : 1.0f;
        }
        
        return signs;
    }

    /// <summary>
    /// Increments a counter byte array for HMAC expansion
    /// </summary>
    private void IncrementCounter(byte[] counter)
    {
        for (int i = 0; i < counter.Length; i++)
        {
            if (++counter[i] != 0)
                break;
        }
    }

    /// <summary>
    /// Verifies that a transformation is orthogonal (preserves distances)
    /// Used for testing
    /// </summary>
    public bool VerifyOrthogonalTransformation(Guid userId, float[] vector1, float[] vector2)
    {
        var transformed1 = TransformVectorForUser(userId, vector1);
        var transformed2 = TransformVectorForUser(userId, vector2);
        
        // Calculate original distance
        var originalDistance = CalculateEuclideanDistance(vector1, vector2);
        
        // Calculate transformed distance
        var transformedDistance = CalculateEuclideanDistance(transformed1, transformed2);
        
        // Check if distances are preserved (within floating point tolerance)
        const float tolerance = 1e-5f;
        return Math.Abs(originalDistance - transformedDistance) < tolerance;
    }

    private float CalculateEuclideanDistance(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have same dimension");
            
        float sum = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var diff = a[i] - b[i];
            sum += diff * diff;
        }
        return (float)Math.Sqrt(sum);
    }
}