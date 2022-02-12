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

namespace AndroidTechnologies
{
    [DisplayName("AndroidTechnologies.LunaToken")]
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "onNEP11Payment")]
    // public class LunaMintsTokenState : SmartContract
    public class LunaToken : Nep11Token<LunaMintsTokenState>
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

        public static ByteString mintLunaToken(string name, string description, string image)
        {
            if (!ValidateContractOwner())
                throw new Exception("Only the contract owner can mint tokens");

            // Generate new token ID.
            var tokenId = NewTokenId();

            var tokenState = new LunaMintsTokenState
            {
                Owner = UInt160.Zero,
                Name = name,
                Description = description,
                Image = image,
            };

            // Pass the call on to the NEP11Token.Mint() method.
            Mint(tokenId, tokenState);

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
