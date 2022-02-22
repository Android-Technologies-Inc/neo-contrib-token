// This module contains the classes that carries the property fields
//  for the NFTs we create.

using System.Numerics;
using Neo;

#nullable enable

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
        /// A URL to the image associated with the token.
        /// </summary>
        public string ImageUrl = string.Empty;

        /// <summary>
        /// If TRUE, then the token is for sale.  If FALSE,
        ///  then it is not for sale.
        /// </summary>
        public bool isForSale = false;

        /// <summary>
        /// If this field has a non-empty value, then only
        ///  the N3 address found in the field can buy
        ///  the token if it is for sale. Otherwise
        ///  anyone call buy the token if it is for sale.
        ///  This field is useful when you want to sell
        ///  transfer an NFT to a specific buyer, whether
        ///  that buyer is a user or another smart
        ///  contract.
        /// </summary>
        public UInt160 allowedBuyer = UInt160.Zero;

        /// <summary>
        /// If a token has been listed for sale, then
        ///  this field tells us how it should be listed
        ///  for sale (e.g. - auction, fixed price, etc.)
        /// </summary>
        public BigInteger saleType = 0;

        /// <summary>
        /// If a token has been listed for sale, this 
        ///  field should contain the price that the token
        ///  should be sold for (fixed price sale) or the
        ///  minimum price it should be sold for if it is
        ///  being auctioned off (i.e. - reserve price).
        /// </summary>
        public BigInteger saleOrMinimumPrice = 0;
    }
}