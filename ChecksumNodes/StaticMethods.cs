namespace ChecksumNodes;

/// <summary>
/// Special class that exposes static methods to the script executor
/// </summary>
public class StaticMethods
{
    /// <summary>
    /// Compute the MD5 hash of a file
    /// </summary>
    /// <param name="filePath">the name of the file to hash</param>
    /// <returns>MD5 Hash value</returns>
    public static string ComputeMD5Hash(string filePath)
    {
        return MD5.ComputeHash(filePath);
    }

    /// <summary>
    /// Compute the SHA1 hash of a file
    /// </summary>
    /// <param name="filePath">the name of the file to hash</param>
    /// <returns>SHA1 Hash value</returns>
    public static string ComputeSHA1Hash(string filePath)
    {
        return SHA1.ComputeHash(filePath);
    }

    /// <summary>
    /// Compute the SHA256 hash of a file
    /// </summary>
    /// <param name="filePath">the name of the file to hash</param>
    /// <returns>SHA256 Hash value</returns>
    public static string ComputeSHA256Hash(string filePath)
    {
        return SHA256.ComputeHash(filePath);
    }

    /// <summary>
    /// Compute the SHA512 hash of a file
    /// </summary>
    /// <param name="filePath">the name of the file to hash</param>
    /// <returns>SHA512 Hash value</returns>
    public static string ComputeSHA512Hash(string filePath)
    {
        return SHA512.ComputeHash(filePath);
    }
}