using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PePe.Base;
using PePe.DAO;
using PePe.Manager;
using PePe.Service;

namespace PePe.CLI {
    class Program {
        static void Main(string[] args) {
            using IHost host = CreateHostBuilder(args).Build();

            // IWebScraper webScraper = host.Services.GetRequiredService<IWebScraper>();
            // Console.WriteLine(webScraper.ScrapeForMenu().Date);

            string connectionString = "mongodb://127.0.0.1:27017/";
            string dbName = "pepedb";
            string collectionName = "menu";
            MongoMenuDao dao = new MongoMenuDao(connectionString, dbName, collectionName);

            var manager = new MenuManager(dao, new PragueDateProvider(), host.Services.GetRequiredService<IWebScraper>(), host.Services.GetRequiredService<ILogger<MenuManager>>());

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

            //SaveOldHtmlMenus(dao, @"C:\Users\martu\OneDrive\Plocha\pepe");

            /*var menus = dao.GetAll();
            var tmp = menus.Select(m => MenuToStr(m));
            Console.WriteLine(string.Join("\n", tmp));*/

            //var todaysMenu = manager.GetTodaysMenu();
            //Console.WriteLine(MenuToStr(todaysMenu));

            var menus = manager.Find(new MenuSearch { FromDate = new DateTime(2021,1,24,15,19,49), ToDate = new DateTime(2021, 1, 27, 15, 19, 49) });
            Console.WriteLine(string.Join("\n", menus.Select(m => MenuToStr(m))));
        }

        class StaticFileLoadedHtmlDocProvider : ILoadedHtmlDocumentProvider {
            public void SetFile(string filename) {
                this.filename = filename;
            }
            public HtmlDocument GetLoadedHtmlDocument() {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(filename);
                return doc;
            }

            private string filename = "";
        }

        static void SaveOldHtmlMenus(IMenuDao dao, string directoryName) {
            var files = Directory.GetFiles(directoryName);
            var htmlProvider = new StaticFileLoadedHtmlDocProvider();
            var webScraper = new WebScraper(htmlProvider, new MonthConvertor(), new PragueDateProvider(), new VoidLogger<WebScraper>());

            foreach (var file in files) {
                if (Path.GetExtension(file) != ".html")
                    continue;
                Console.WriteLine(file);
                htmlProvider.SetFile(file);
                var menu = webScraper.ScrapeForMenu();
                dao.Save(menu);
            }
        }

        static private string MenuToStr(Menu m) => m == null ? "null" : $"Menu ({m.Date}):\n\t" + string.Join("\n\t", m.Meals.Select(meal => $"{meal.Name}: {meal.Price}"));


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
