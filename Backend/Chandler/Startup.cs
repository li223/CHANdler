﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Chandler.Data;

namespace Chandler
{
    public class Startup
    {
        private Database _db;
        private ServerMeta _meta;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _db = new Database(DatabaseProvider.InMemory, null);
            _meta = new ServerMeta();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<Database>(_db);
            services.AddSingleton<ServerMeta>(_meta);
            services.AddCors(o => o.AddPolicy("publicpolicy", builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));

            var ctx = _db.GetContext();
            ctx.Database.EnsureCreated();

            // insert debug thread data to database
            ctx.Boards.Add(new Data.Entities.Board()
            {
                Name = "CHANdler",
                Tag = "c",
                Description = "CHANdler test board",
                ImageUrl = "https://i.kym-cdn.com/photos/images/newsfeed/000/779/388/d33.jpg"
            });

            ctx.Boards.Add(new Data.Entities.Board()
            {
                Name = "Random",
                Tag = "r",
                Description = "Random shit",
            });

            ctx.Boards.Add(new Data.Entities.Board()
            {
                Name = "Memes",
                Tag = "m",
                ImageUrl = "https://img.thedailybeast.com/image/upload/c_crop,d_placeholder_euli9k,h_1440,w_2560,x_0,y_0/dpr_1.5/c_limit,w_1044/fl_lossy,q_auto/v1531451526/180712-Weill--The-Creator-of-Pepe-hero_uionjj",
                Description = "haha cool and good dank memes",
            });

            var salt = Passworder.GenerateSalt();
            var pass = Passworder.GenerateHash("admin", salt);

            ctx.Passwords.Add(new Data.Entities.Password()
            {
                Id = -1,
                Salt = salt,
                Cycles = pass.cycles,
                Hash = pass.hash
            });

            ctx.SaveChanges();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("publicpolicy");
            app.UseMvc();
        }
    }
}