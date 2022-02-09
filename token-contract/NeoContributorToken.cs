using System;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using AndroidTechnologies;

#nullable enable

namespace NgdEnterprise.Samples
{
    [DisplayName("NgdEnterprise.Samples.NeoContributorToken")]
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "onNEP11Payment")]
    // public class NeoContributorToken : SmartContract
    public class NeoContributorToken : Nep11Token<LunaMintsTokenState>
    {
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

        const byte Prefix_ContractOwner = 0xFF;

        [Safe]
        public override string Symbol() => "LUNAMINTS";

        [Safe]
        public override Map<string, object> Properties(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            LunaMintsTokenState token = (LunaMintsTokenState)StdLib.Deserialize(tokenMap[tokenId]);
            Map<string, object> map = new();

            // Even though the "owner" and "name" fields reside in the 
            //  Neo.SmartContract.Framework.Nep11TokenState class inherited
            //  by our TokenState object, we are responsible for adding those
            // fields to the map of properties we return.
            map["owner"] = token.Owner;
            map["name"] = token.Name;
            map["description"] = token.Description;
            map["image"] = token.Image;
            return map;
        }

        public static UInt256 Mint(string name, string description, string image)
        {
            if (!ValidateContractOwner()) 
                throw new Exception("Only the contract owner can mint tokens");

            // generate new token ID
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_TokenId };
            var id = (BigInteger)Storage.Get(context, key);
            Storage.Put(context, key, id + 1);

            var tokenIdString = nameof(NeoContributorToken) + id;
            var tokenId = (UInt256)CryptoLib.Sha256(tokenIdString);

            var tokenState = new LunaMintsTokenState
            {
                Owner = UInt160.Zero,
                Name = name,
                Description = description,
                Image = image,
            };

            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            tokenMap[tokenId] = StdLib.Serialize(tokenState);
            UpdateBalance(tokenState.Owner, tokenId, +1);
            UpdateTotalSupply(+1);
            PostTransfer(null, tokenState.Owner, tokenId, null);

            return tokenId;
        }

        public bool Withdraw(UInt160 to)
        {
            if (!ValidateContractOwner()) 
                throw new Exception("Only the contract owner can withdraw NEO");
            if (to == UInt160.Zero || !to.IsValid) 
                throw new Exception("Invalid withrdrawal address");

            var balance = NEO.BalanceOf(Runtime.ExecutingScriptHash);
            if (balance <= 0) 
                return false;

            return NEO.Transfer(Runtime.ExecutingScriptHash, to, balance);
        }

        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            const string errPrefix = "OnNEP17Payment";

            if (data != null)
            {
                var tokenId = (ByteString)data;

                // We only accept NEO gas payments.
                if (Runtime.CallingScriptHash != GAS.Hash) 
                    reportErrorAndThrow($"{errPrefix}: Invalid script hash");

                // TODO: Need to handle variable amounts.
                if (amount < 10) throw 
                    new Exception("Insufficient payment price");

                StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
                var tokenData = tokenMap[tokenId];
                if (tokenData == null) 
                    throw new Exception("Invalid token id"); 
                var token = (LunaMintsTokenState)StdLib.Deserialize(tokenData);
                if (token.Owner != UInt160.Zero) 
                    throw new Exception("Specified token already owned");

                if (!Transfer(from, tokenId, null)) 
                    throw new Exception("Transfer Failed");
            }
        }

        [DisplayName("_deploy")]
        public static void Deploy(object data, bool update)
        {
            if (update) return;

            var tx = (Transaction)Runtime.ScriptContainer;
            var key = new byte[] { Prefix_ContractOwner };
            Storage.Put(Storage.CurrentContext, key, tx.Sender);
        }

        public static void Update(ByteString nefFile, string manifest)
        {
            if (!ValidateContractOwner()) 
                throw new Exception("Only the contract owner can update the contract");

            ContractManagement.Update(nefFile, manifest, null);
        }

        static bool ValidateContractOwner()
        {
            var key = new byte[] { Prefix_ContractOwner };
            var contractOwner = (UInt160)Storage.Get(Storage.CurrentContext, key);
            var tx = (Transaction)Runtime.ScriptContainer;
            return contractOwner.Equals(tx.Sender) && Runtime.CheckWitness(contractOwner);
        }
    }
}
