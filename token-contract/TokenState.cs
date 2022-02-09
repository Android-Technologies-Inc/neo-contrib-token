using Neo;

#nullable enable

/// <summary>
/// This class contains the properties for a single NFT token.
/// </summary>
public class TokenState
{
    /// <summary>
    /// The N3 address of the token owner.
    /// </summary>
    public UInt160 Owner = UInt160.Zero;
    /// <summary>
    /// The name assigned to the token.
    /// </summary>
    public string Name = string.Empty;
    /// <summary>
    /// An informative description of the token.
    /// </summary>
    public string Description = string.Empty;
    /// <summary>
    /// The image associated with the token.
    /// </summary>
    public string Image = string.Empty;
}