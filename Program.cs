using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GodotFest.VisaLetter;
using GodotFest.VisaLetter.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<BrowserFiles>();
builder.Services.AddScoped<LetterOutput>();

await builder.Build().RunAsync();
