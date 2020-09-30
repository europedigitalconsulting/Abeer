using IdentityServer4.Models;

using System.Collections.Generic;

namespace Cryptocoin.Quorum
{
    public static class Config
    {
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("Cryptocoin.Quorum", "Quorum")
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "Cryptocoin.BlockChainService.ConsoleRunner",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("b14ca5898a4e4133bbce2ea2315a1916".Sha256())
                    },
                    Claims = 
                    {
                        new ClientClaim("audience", "Cryptocoin.BlockChainService")
                    },
                    // scopes that client has access to
                    AllowedScopes = { "Cryptocoin.Quorum" }
                },
                new Client
                {
                    ClientId = "Cryptocoin.ServerApi",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("b14ca5898a4e4133bbce2ea2315a1916".Sha256())
                    },
                    Claims =
                    {
                        new ClientClaim("audience", "Cryptocoin.ServerApi")
                    },
                    // scopes that client has access to
                    AllowedScopes = { "Cryptocoin.Quorum" }
                }
            };
    }
}