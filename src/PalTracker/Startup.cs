using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;

namespace PalTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var message = Configuration.GetValue<string>("WELCOME_MESSAGE");
            if (string.IsNullOrEmpty(message)){
                throw new ApplicationException("WELCOME_MESSAGE not configured");
            }

            services.AddSingleton(sp=> new WelcomeMessage(message));
            
            var port = Configuration.GetValue<string>("PORT");
            var memory_limit = Configuration.GetValue<string>("MEMORY_LIMIT");
            var cf_instance_idx = Configuration.GetValue<string>("CF_INSTANCE_INDEX");
            var cf_instance_addr= Configuration.GetValue<string>("CF_INSTANCE_ADDR");

            // if (string.IsNullOrEmpty(port) ||
            //     string.IsNullOrEmpty(memory_limit) ||
            //     string.IsNullOrEmpty(cf_instance_idx) ||
            //     string.IsNullOrEmpty(cf_instance_addr))
            // {
            //     throw new ApplicationException("CloudFoundryInfo not properly configured");
            // }

            services.AddSingleton(o=> new CloudFoundryInfo(port, memory_limit,cf_instance_idx,cf_instance_addr));

            //services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
            services.AddDbContext<TimeEntryContext>(options => options.UseMySql(Configuration));
            services.AddScoped<ITimeEntryRepository, MySqlTimeEntryRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
