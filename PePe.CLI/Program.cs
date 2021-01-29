using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PePe.Base;
using PePe.DAO;
using PePe.Service;

namespace PePe.CLI {
    class Program {
        static void Main(string[] args) {
            using IHost host = CreateHostBuilder(args).Build();

            // IWebScraper webScraper = host.Services.GetRequiredService<IWebScraper>();
            // Console.WriteLine(webScraper.ScrapeForMenu().Date);

            string connectionString = "mongodb://127.0.0.1:27017";
            string dbName = "pepedb";
            string collectionName = "menu";
            MongoMenuDao dao = new MongoMenuDao(connectionString, dbName, collectionName);

            Menu m = new Menu {
                Date = new DateTime(2021, 1, 22),
                Meals = new List<Meal> {
                    new Meal {
                        MealType = MealType.Main,
                        Name = "Shit",
                        Price = 666,
                        Info = "Pure shit"
                    }
                }
            };

            //m = dao.Save(m);

            var menus = dao.GetAll();
            var tmp = menus.Select(m => MenuToStr(m));
            Console.WriteLine(string.Join("\n", tmp));

            var menu = dao.GetMenuByDate(new DateTime(2021, 1, 22));
            Console.WriteLine("GetMenuByDate");
            Console.WriteLine(MenuToStr(menu));
        }

        static private string MenuToStr(Menu m) => m == null ? "null" : $"Menu ({m.Date}):\n" + string.Join("\n\t", m.Meals.Select(meal => $"{meal.Name}: {meal.Price}"));


        static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) => {
                    logging.ClearProviders();
                    logging.AddConsole(options => options.IncludeScopes = true);
                    logging.AddDebug();
                })
                .ConfigureServices(services => {
                    services.AddLogging();

                    new ClassMap().RegisterMaps();

                    services.AddScoped<IMonthConvertor, MonthConvertor>();
                    services.AddScoped<ILoadedHtmlDocumentProvider, WebLoadedHtmlDocumentProvider>();
                    services.AddScoped<IBasicInfoProvider, StaticBasicInfoProvider>();

                    services.AddScoped<IWebScraper, WebScraper>();
                });
        }
    }
}
