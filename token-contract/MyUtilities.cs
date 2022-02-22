// This module contains some helpful utility code used
//  in NEO N3 smart contract work.
using System;
using Neo;
using Neo.SmartContract.Framework.Services;

#nullable enable

namespace AndroidTechnologies
{

    public static class MyUtilities
    {
        /// <summary>
        /// Simple function to both log and then throw and Exception
        ///     using the given error message.
        /// </summary>
        /// <param name="errMsg">An error message.</param>
        public static void ReportErrorAndThrow(string errMsg)
        {
            Runtime.Log(errMsg);
            throw new Exception(errMsg);
        }

        /// <summary>
        /// This function checks to see if we are currently
        ///  executing on a NEO Express network instance.
        ///  
        /// WARNING: If this function is not kept updated as 
        ///  new networks are added with the network IDs
        ///  of those networks, it may return an incorrect
        ///  value!
        /// </summary>
        /// <returns>Returns TRUE if the current network we 
        ///  are executing is (allegedly) a NEO Express
        ///  instance, FALSE if not.</returns>
        public static bool IsNeoExpress()
        {
            bool bIsNeoExpress = false;

            var networkId = Runtime.GetNetwork();
            var networkName = "NEO Express";

            // Do the main net check first so we minimize
            //  GAS usage when it counts the most.
            if (networkId == 860833102)
            {
                /* Neo 3 MainNet */
                networkName = "Neo 3 MainNet";
            }
            else if (networkId == 877933390)
            {
                /* Neo 3 TestNet */
                networkName = "Neo 3 TestNet";
            }
            else if (networkId == 7630401)
            {
                /* Neo 2 MainNet */
                networkName = "Neo 2 MainNet";
            }
            else if (networkId == 1953787457)
            {
                /* Neo 2 TestNet */
                networkName = "Neo 2 TestNet";
            }
            else if (networkId == 844378958)
            {
                /* Neo 3 RC3 TestNet */
                networkName = "Neo 3 RC3 TestNet";
            }
            else if (networkId == 827601742)
            {
                /* Neo 3 RC1 TestNet */
                networkName = "Neo 3 RC1 TestNet";
            }
            else if (networkId == 894448462)
            {
                /* Neo 3 Preview 5 TestNet */
                networkName = "Neo 3 Preview 5 TestNet";
            }
            else
            {
                /* We assume it is a NEO Express instance. */
                bIsNeoExpress = true;
            }

            Runtime.Log($"CURRENT NEO NETWORK: {networkName} ");
            return bIsNeoExpress;
        }

        /// <summary>
        /// Helper function that returns TRUE if a UInt160 value
        ///  contains anything but the default empty value.
        ///  FALSE otherwise.
        /// </summary>
        /// <param name="uiValue">The value to inspect.</param>
        /// <returns>Returns TRUE if the UInt160 value is
        ///  empty, FALSE if not.</returns>
        public static bool IsEmptyUInt160(UInt160 uiValue) {
            return uiValue == UInt160.Zero;
        }

        /// <summary>
        /// Validates the transaction sender and the makes sure
        ///  that it is equal to the given token owner value.
        /// </summary>
        /// <param name="tokenOwner">The current owner of 
        ///  a token.</param>
        /// <returns>Returns TRUE if the transaction sender
        ///  passes a CheckWitness() check and is equal
        ///  to the value given in the tokenOwner parameter.</returns>
        public static bool IsSenderTokenOwner(UInt160 tokenOwner)
        {
            if (IsEmptyUInt160(tokenOwner))
                ReportErrorAndThrow($"({nameof(IsSenderTokenOwner)}) The token owner parameter is empty.");

            // Get a reference to the current transaction.
            var tx = (Transaction)Runtime.ScriptContainer;

            return (tx.Sender == tokenOwner && Runtime.CheckWitness(tx.Sender));
        }
    } // class MyUtilities
}