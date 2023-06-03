using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Blazor.Analytics;


namespace ReYzuClassViewer
{
    public class Program
    {
        public static async Task Main( string[] args )
        {
            var builder = WebAssemblyHostBuilder.CreateDefault( args );
            builder.RootComponents.Add<App>( "#app" );
            builder.RootComponents.Add<HeadOutlet>( "head::after" );

            builder.Services.AddScoped( sp => new HttpClient 
            {
                //BaseAddress = new Uri( builder.HostEnvironment.BaseAddress ),
               


            } );

            //
            builder.Services.AddAntDesign();
            builder.Services.AddGoogleAnalytics( "G-0KDQ4FHSYZ" );



            //

            await builder.Build().RunAsync();
        }
    }
}