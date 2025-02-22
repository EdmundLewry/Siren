using AutoMapper;
using CBS.Siren.Application;
using CBS.Siren.Data;
using CBS.Siren.Device;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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
            services.AddSingleton<IDataLayer, CollectionDataLayer>();
            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<ITransmissionListServiceStore, TransmissionListServiceStore>();
            services.AddSingleton<IDeviceListEventStore, DeviceListEventStore>();
            services.AddTransient<ITransmissionListService, TransmissionListService>();
            services.AddTransient<ITransmissionListHandler, TransmissionListHandler>();
            services.AddTransient<IScheduler, SimpleScheduler>();
            services.AddTransient<IDeviceFactory, DeviceFactory>();
            services.AddTransient<IDeviceListEventWatcher, DeviceListEventWatcher>();
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

            IDataLayer dataLayer = app.ApplicationServices.GetService<IDataLayer>();
            dataLayer.AddUpdateMediaInstances(new MediaInstance("TestInstance", new TimeSpan(0,0,30)));

            /* For this early stage we're just going to create a single transmission list to work on.
            This is because sat this stage of the application, it's not possible to add transmission lists
            to channels */
            InitializeTransmissionList(dataLayer);
        }

        private void InitializeTransmissionList(IDataLayer dataLayer)
        {
            TransmissionList transmissionList = new TransmissionList(new List<TransmissionListEvent>(), null);
            
            dataLayer.AddUpdateTransmissionLists(transmissionList);
        }
    }
}