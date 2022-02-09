#nullable enable

// This module contains the classes that carries the property fields
//  for the NFTs we create.

namespace AndroidTechnologies
{
    /// <summary>
    /// This class contains the properties for a single NFT token.
    /// </summary>
    public class LunaMintsTokenState : Neo.SmartContract.Framework.Nep11TokenState
    {
        // NOTE: The "Name" and the "Owner" fields are in the inherited class.

        /// <summary>
        /// An informative description of the token.
        /// </summary>
        public string Description = string.Empty;
        /// <summary>
        /// The image associated with the token.
        /// </summary>
        public string Image = string.Empty;
    }
}