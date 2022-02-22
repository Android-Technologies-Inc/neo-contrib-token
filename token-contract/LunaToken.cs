// This is the smart contract for the LunaMint NFT.

// IMPORTANT: The current logic of the LunaMint contract currently 
//  does not permit resale of tokens because any attempt to buy a 
//  token that already has an owner (i.e. - a non-zero Owner field), 
//  will trigger an exception.  See the OnNEP17Payment() function.

using System;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

#nullable enable

namespace AndroidTechnologies
{
    [DisplayName("AndroidTechnologies.LunaToken")]
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "onNEP11Payment")]
    // public class LunaMintsTokenState : SmartContract
    // public class LunaToken : Nep11Token<LunaMintsTokenState>
    public class LunaToken : Nep11Token<LunaMintsTokenState>
    {
        // ----------- BEGIN: EVENTS ----------

        // These are the events emitted by this smart contract.

        // >>>>> EVENT: NEW TOKEN MINTED - A new token has been minted.

        // Define a delegate for the event.
        public delegate void OnNewLunamintTokenDelegate(ByteString idOfToken, string nameOfToken);

        // Declare the event name and event.
        [DisplayName("NewTokenCreated")]
        public static event OnNewLunamintTokenDelegate OnNewLunamintToken = default!;

        // ----------- END  : EVENTS ----------

        // >>>>> EVENT: PAYMENT MADE - A payment had been made to a
        //  a player or a band after a recently finished game.

        /// <summary>
        /// Simple function to both log and then throw and Exception
        ///     using the given error message.
        /// </summary>
        /// <param name="errMsg">An error message.</param>
        private static void reportErrorAndThrow(string errMsg)
        {
            Runtime.Log(errMsg);
            throw new Exception(errMsg);
        }

        /*
        private static string uint160ToString(UInt160 u160)
        {
            var retStr = "";

            foreach (var b in u160)
            {
                string strByte = b.ToString();
                retStr += strByte;
            }

            return retStr;
        }
        */

        /// <summary>
        /// Debug function to display two UInt160 values side by
        ///  side as a pair of columns containing the byte values
        ///  for each UInt160 value.  Useful for inspecting two
        ///  UInt160 values to look for differences between
        ///  them.  The results are printed to the debug console.
        /// </summary>
        /// <param name="hash_1">The first UInt160 value to use in
        ///  the comparison.</param>
        /// <param name="hash_2">The second UInt160 value to use in
        ///  the comparison.</param>
        private static void logSideBySideHashDisplay(UInt160 hash_1, UInt160 hash_2)
        {
            var loopCount = Math.Max(hash_1.Length, hash_2.Length);

            for (var ndx = 0; ndx < loopCount; ndx++)
            {
                var b1 = ndx < hash_1.Length ? hash_1[ndx] : -1;
                var b2 = ndx < hash_2.Length ? hash_2[ndx] : -1;

                string str = $"[{b1}], [{ b2}]";
                Runtime.Log(str);
            }
        }

        // The prefix to use for the contract owner field when
        //  stored in a StorageMap object.
        const byte Prefix_ContractOwner = 0xFF;

        /// <summary>
        /// Return a string that represents the tokens this
        ///  contract mints/maintains.
        /// </summary>
        /// <returns></returns>
        [Safe]
        public override string Symbol() => "LUNAMINTS";

        /// <summary>
        /// Get a token's property values.
        /// </summary>
        /// <param name="tokenId">The ID of the desired token.</param>
        /// <returns>Returns a map object containing the desired token's
        ///  metadata (field values).</returns>
        [Safe]
        public override Map<string, object> Properties(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            LunaMintsTokenState token = (LunaMintsTokenState)StdLib.Deserialize(tokenMap[tokenId]);
            Map<string, object> map = new();

            // Even though the "owner" and "name" fields reside in the 
            //  Neo.SmartContract.Framework.Nep11TokenState class inherited
            //  by our LunaMintsTokenState object, we are responsible for adding those
            // fields to the map of properties we return.
            map["owner"] = token.Owner;
            map["name"] = token.Name;
            map["description"] = token.Description;
            map["image_url"] = token.ImageUrl;

            return map;
        }

        /// <summary>
        /// Create a token ID for a new NFT.
        /// </summary>
        /// <returns>Returns a new, unique ID for use
        ///  with a new NFT.</returns>
        private static ByteString MyNewTokenId() {
            // If we are executing on a private instance of the NEO
            //  blockchain (NEO Express), use a simple token ID
            //  that is not based on the hash of the currently
            //  executing smart contract (this one).  That way
            //  we don't break any invoke files that have 
            //  token IDs in their parameter values.  If we are 
            //  not running on a private instance, then use the
            //  method found in the Nep11Token class we
            //  inherit from, that does use the script hash
            //  to build the token ID.
            if (MyUtilities.isNeoExpress()) {
                StorageContext context = Storage.CurrentContext;
                byte[] key = new byte[] { Prefix_TokenId };
                ByteString id = Storage.Get(context, key);
                Storage.Put(context, key, (BigInteger)id + 1);
                // Do not use the executing script hash to build
                //  the token ID or we may break any invoke files
                //  that have token IDs in their parameter values
                //  if we modify this smart cotnract.
                // ByteString data = Runtime.ExecutingScriptHash;
                ByteString data = (ByteString)"0";
                if (id is not null)
                    data += id;

                // We don't hash the value since we are running on 
                //  a private blockchain.
                // return CryptoLib.Sha256(data);                

                return data;
            }
            else
            {
                // We are not running on a private NEO blockchain.
                //  Pass the call on to the Nep11Token smart contract
                //  we inherit from.
                return NewTokenId();
            }
        }

        /// <summary>
        /// Mint a new token.
        /// </summary>
        /// <param name="name">A descriptive name for the token.</param>
        /// <param name="description">A short description of the token.</param>
        /// <param name="imageUrl">The image URL, if applicable.</param>
        /// <returns></returns>
        public static ByteString mintLunaToken(string name, string description, string imageUrl)
        {
            var errPrefix = "(mintLunaToken) ";

            if (!ValidateContractOwner())
                throw new Exception("Only the contract owner can mint tokens");

            // Generate new token ID.
            var tokenId = MyNewTokenId();

            // Get a reference to the current transaction.
            var tx = (Transaction)Runtime.ScriptContainer;

            // Fill in the token's meta-data fields.
            var tokenState = new LunaMintsTokenState
            {
                // The minter of the NFT is the initial owner.
                //  A typical scenario would be an NFT artist
                //  minting a token for later sale.
                Owner = tx.Sender,
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
            };

            // Pass the call on to the NEP11Token.Mint() method.
            Mint(tokenId, tokenState);

            Runtime.Log($"{errPrefix}Minted new token('{name}') with ID: {tokenId}.");

            // Emit an event regarding the new token.
            OnNewLunamintToken(tokenId, name);

            return tokenId;
        }

        /// <summary>
        /// Withdraw the balance of GAS held by this contract and
        ///   transfer it to the given destination.
        /// </summary>
        /// <param name="to">The N3 address to deliver the
        ///  GAS to.</param>
        /// <returns>Returns TRUE if a transfer was executed.  FALSE
        ///   if not, because there is nothing to transfer.</returns>
        public bool Withdraw(UInt160 to)
        {
            if (!ValidateContractOwner()) 
                throw new Exception("Only the contract owner can withdraw GAS");
            if (to == UInt160.Zero || !to.IsValid) 
                throw new Exception("Invalid withrdrawal address");

            var balance = GAS.BalanceOf(Runtime.ExecutingScriptHash);
            if (balance <= 0) 
                return false;

            return GAS.Transfer(Runtime.ExecutingScriptHash, to, balance);
        }

        /// <summary>
        /// This method is called by the GasToken contract after 
        ///  a GAS payment has been made successfully (i.e. - a 
        ///  payment has been made like when buying an NFT, etc.)
        /// </summary>
        /// <param name="from">The sender of the transaction, which
        ///  is the user making the payment.</param>
        /// <param name="amount">The amount of GAS paid.</param>
        /// <param name="data">Any custom data that was included
        ///  with the transfer/payment.</param>
        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            const string errPrefix = "(OnNEP17Payment) ";

            // TODO: REMOVE THIS!  Testing CheckWitness() from within 
            if (Runtime.CheckWitness(from))
                Runtime.Log($"{errPrefix} CheckWitness SUCCEEDED at the top of this call.");
            else
                Runtime.Log($"{errPrefix} CheckWitness FAILED at the top of this call.");

            if (data != null)
            {
                var tokenId = (ByteString)data;

                if (Runtime.CallingScriptHash == GAS.Hash)
                    Runtime.Log($"{errPrefix} The GAS contract is calling us.");
                else if (Runtime.CallingScriptHash == NEO.Hash)
                    Runtime.Log($"{errPrefix} The NEO contract is calling us.");
                else if (Runtime.CallingScriptHash == Runtime.CallingScriptHash)
                    Runtime.Log($"{errPrefix} The LunaToken contract is calling itself.");
                else
                    Runtime.Log($"{errPrefix} Unknown N3 address is calling us");

                // We only accept NEO gas payments.
                if (Runtime.CallingScriptHash != GAS.Hash)
                {
                    // TODO: Mention the (addr) cast for viewing script hashes
                    //  as string instead of an array of hex bytes.
                    logSideBySideHashDisplay(Runtime.CallingScriptHash, GAS.Hash);
                    reportErrorAndThrow($"{errPrefix}: Invalid script hash");
                }

                if (amount < 1)  
                    reportErrorAndThrow($"{errPrefix}: Insufficient payment amount");
                if (amount > 2) 
                    reportErrorAndThrow($"{errPrefix}: Payment amount is too large");

                StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
                var tokenData = tokenMap[tokenId];
                if (tokenData == null)
                {
                    reportErrorAndThrow($"{errPrefix}: Invalid token id. Token ID: {tokenId}");
                }

                var token = (LunaMintsTokenState)StdLib.Deserialize(tokenData);

                if (token.Owner != UInt160.Zero) 
                    reportErrorAndThrow($"{errPrefix}: The specified token already has an owner. Token ID: {tokenId}");

                // Make sure the sender of this transaction (i.e. - the
                //  "from" parameter) is not trying to buy a token they
                //  already own.
                if (token.Owner == from)
                    reportErrorAndThrow($"{errPrefix}: The sender already owns the specified token. Token ID: {tokenId}");

                // We use the "from" N3 address as the "to" parameter value
                //  when calling the inherited Transfer() method becaause
                //  we want to transfer the token from the current owner,
                //  whoever that is, to the N3 address this transaction
                //  is being executed for.  (i.e. - the buyer)

                // If we are running on a private blockchain, we use our own
                //  transfer method that does not execute 
                if (!Transfer(from, tokenId, null)) 
                    reportErrorAndThrow($"{errPrefix}: Transfer Failed.  Check permissions scopes.");
            }
        }

        /// <summary>
        /// Deploy the smart contract.
        /// </summary>
        /// <param name="data">The contract contents (code and data).</param>
        /// <param name="update">If TRUE, then this is an update of an 
        ///  existing instance of the smart contract.  If FALSE then this
        ///  is the first time the contract has been deployed.</param>
        [DisplayName("_deploy")]
        public static void Deploy(object data, bool update)
        {
            if (update) return;

            var tx = (Transaction)Runtime.ScriptContainer;
            var key = new byte[] { Prefix_ContractOwner };
            Storage.Put(Storage.CurrentContext, key, tx.Sender);
        }

        /// <summary>
        /// Update the smart contract with a new version.
        /// </summary>
        /// <param name="nefFile">The smart contract NEF file as a string.</param>
        /// <param name="manifest">The smart contract manifest.</param>
        public static void Update(ByteString nefFile, string manifest)
        {
            if (!ValidateContractOwner()) 
                throw new Exception("Only the contract owner can update the contract");

            ContractManagement.Update(nefFile, manifest, null);
        }

        /// <summary>
        /// Make sure this smart contract has a valid owner.
        /// </summary>
        /// <returns>Returns TRUE if the contract has a a
        ///   valid owner, FALSE if not.</returns>
        static bool ValidateContractOwner()
        {
            var key = new byte[] { Prefix_ContractOwner };
            var contractOwner = (UInt160)Storage.Get(Storage.CurrentContext, key);
            var tx = (Transaction)Runtime.ScriptContainer;

            return contractOwner.Equals(tx.Sender) && Runtime.CheckWitness(contractOwner);
        }
    }
}
