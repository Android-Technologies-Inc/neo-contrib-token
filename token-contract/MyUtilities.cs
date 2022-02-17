// This module contains some helpful utility code used
//  in NEO N3 smart contract work.
using Neo.SmartContract.Framework.Services;

#nullable enable

namespace AndroidTechnologies
{

    public static class MyUtilities
    {
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
        public static bool isNeoExpress()
        {
            bool bIsNeoExpress = false;

            var networkId = Runtime.GetNetwork();
            var networkName = "NEO Express";

            if (networkId == 7630401)
            {
                /* Neo 2 MainNet */
                networkName = "Neo 2 MainNet";
            }
            else if (networkId == 1953787457)
            {
                /* Neo 2 TestNet */
                networkName = "Neo 2 TestNet";
            }
            else if (networkId == 860833102)
            {
                /* Neo 3 MainNet */
                networkName = "Neo 3 MainNet";
            }
            else if (networkId == 877933390)
            {
                /* Neo 3 TestNet */
                networkName = "Neo 3 TestNet";
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
            else if (networkId == 7630401)
            {
                /* We assume it is a NEO Express instance. */
                bIsNeoExpress = true;
            }

            Runtime.Log($"CURRENT NEO NETWORK: {networkName} ");
            return bIsNeoExpress;
        }
    } // class MyUtilities
}