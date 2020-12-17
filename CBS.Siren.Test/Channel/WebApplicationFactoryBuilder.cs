using CBS.Siren.DataLayer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace CBS.Siren.Test
{
    public class WebApplicationFactoryBuilder<TStartup> where TStartup : class
    {
        public WebApplicationFactoryBuilder()
        {
        }

        public WebApplicationFactory<TStartup> CreateWebApplicationFactory(Type dataLayerInitializerType)
        {
            var factory = new WebApplicationFactory<TStartup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => {
                    //Remove any added services which we don't need
                });

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IDataLayerInitializer));
                    services.AddTransient(typeof(IDataLayerInitializer), dataLayerInitializerType);
                });
            });

            return factory;
        }
    }
}
