// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;





var hostBuilder = Host.CreateDefaultBuilder(args)
                        .ConfigureHostConfiguration(conf => conf.AddEnvironmentVariables())
                        .ConfigureServices( x =>
                        {
                            x.AddSingleton<TwitchService>();
                            x.AddSingleton<TelegramService>();

                        }
                        
                         );

                        
var host = hostBuilder.Build();

await host.Services.GetRequiredService<TwitchService>().ExecuteAsync();
await host.Services.GetRequiredService<TelegramService>().ExecuteAsync();

host.WaitForShutdown();