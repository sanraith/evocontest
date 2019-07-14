using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace evorace.WebApp.Core
{
    public class ConfigurationValidator : IConfigurationValidator
    {
        public ConfigurationValidator(IConfiguration configuration)
        {
            myConfiguration = configuration;
        }

        public void ValidateSecrets()
        {
            var emptySecrets = new ConfigurationBuilder()
                .AddJsonFile("secrets-empty.json", false, true)
                .Build();

            foreach(var kvp in emptySecrets.AsEnumerable())
            {
                if (myConfiguration.GetValue<string>(kvp.Key) == null)
                {
                    throw new ArgumentNullException(nameof(myConfiguration), @"User secrets are not configured!");
                }
            }
        }

        private readonly IConfiguration myConfiguration;
    }
}
