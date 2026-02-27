using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace qckdev.Net.Http.Test.Service
{
    public class Startup
    {
        private static readonly object UsersLock = new object();
        private static int _lastUserId = 1000;
        private static readonly Dictionary<int, GoUser> Users = new Dictionary<int, GoUser>();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await WriteJson(context, 200, "Hello world");
                });

                endpoints.MapGet("/api/v2/pokemon/ditto", async context =>
                {
                    await WriteJson(context, 200, new
                    {
                        id = 132,
                        name = "ditto",
                        order = 214,
                        species = new
                        {
                            name = "ditto",
                            url = "https://pokeapi.co/api/v2/pokemon-species/132/"
                        }
                    });
                });

                endpoints.MapGet("/api/v2/pokemon/meloinvento", async context =>
                {
                    await WriteJson(context, 404, new { detail = "Not Found" });
                });

                endpoints.MapGet("/rest/api/latest/issue/JRA-meloinvento", async context =>
                {
                    await WriteJson(context, 404, new
                    {
                        errorMessages = new[] { "Issue does not exist or you do not have permission to see it." },
                        errors = new { }
                    });
                });

                endpoints.MapPost("/public/v1/users", async context =>
                {
                    var request = await JsonSerializer.DeserializeAsync<GoUser>(context.Request.Body, JsonOptions)
                        ?? new GoUser();

                    if (string.IsNullOrWhiteSpace(request.Name))
                    {
                        await WriteJson(context, 422, new
                        {
                            meta = (object)null,
                            data = new[]
                            {
                                new
                                {
                                    field = "name",
                                    message = "can't be blank"
                                }
                            }
                        });
                        return;
                    }

                    GoUser createdUser;
                    lock (UsersLock)
                    {
                        _lastUserId++;
                        createdUser = new GoUser
                        {
                            Id = _lastUserId,
                            Name = request.Name,
                            Gender = request.Gender,
                            Email = request.Email,
                            Status = request.Status
                        };
                        Users[_lastUserId] = createdUser;
                    }

                    await WriteJson(context, 201, new
                    {
                        meta = (object)null,
                        data = new
                        {
                            id = createdUser.Id,
                            name = createdUser.Name,
                            email = createdUser.Email,
                            gender = createdUser.Gender,
                            status = createdUser.Status
                        }
                    });
                });

                endpoints.MapDelete("/public/v1/users/{id:int}", async context =>
                {
                    int id = int.Parse((string)context.Request.RouteValues["id"]);

                    lock (UsersLock)
                    {
                        if (Users.Remove(id))
                        {
                            goto deleted;
                        }
                    }

                    await WriteJson(context, 404, new
                    {
                        meta = (object)null,
                        data = new
                        {
                            message = "Resource not found"
                        }
                    });
                    return;

                deleted:
                    await WriteJson(context, 200, new
                    {
                        meta = (object)null,
                        data = new
                        {
                            message = "Resource deleted"
                        }
                    });
                });

                endpoints.MapGet("/bin/df9f78ca-6298-4a32-93ee-c9130807d116", async context =>
                {
                    await WriteJson(context, 200, "Hello world");
                });
            });
        }

        private static Task WriteJson(HttpContext context, int statusCode, object payload)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }

        private sealed class GoUser
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
        }
    }
}
