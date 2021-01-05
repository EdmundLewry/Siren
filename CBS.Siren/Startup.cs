using AutoMapper;
using CBS.Siren.Application;
using CBS.Siren.Data;
using CBS.Siren.DataLayer;
using CBS.Siren.Device;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CBS.Siren
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddAutoMapper(typeof(Startup));
            
            services.AddTransient<SirenApplication>();
            services.AddTransient<IDataLayerInitializer, DataLayerInitializer>();
            services.AddSingleton<IDataLayer, CollectionDataLayer>();
            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<ITransmissionListServiceStore, TransmissionListServiceStore>();
            services.AddSingleton<IDeviceListEventStore, DeviceListEventStore>();
            services.AddTransient<ITransmissionListService, TransmissionListService>();
            services.AddTransient<ITransmissionListHandler, TransmissionListHandler>();
            services.AddTransient<IScheduler, SimpleScheduler>();
            services.AddTransient<IDeviceFactory, DeviceFactory>();
            services.AddTransient<IDeviceListEventWatcher, DeviceListEventWatcher>();
            services.AddTransient<IChannelHandler, ChannelHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            IDeviceManager deviceManager = app.ApplicationServices.GetService<IDeviceManager>();
            deviceManager.AddDevice("DemoDevice", new DeviceProperties() { Preroll = TimeSpan.FromSeconds(5) });

            IDataLayerInitializer dataLayerInitializer = app.ApplicationServices.GetService<IDataLayerInitializer>();
            dataLayerInitializer?.Seed();
        }
    }
}