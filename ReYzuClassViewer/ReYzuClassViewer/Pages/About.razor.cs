using System.Text.Json.Nodes;
using System.Text.Json;
using System;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;
using Microsoft.JSInterop;

namespace ReYzuClassViewer.Pages
{
    public partial class About
    {
        private readonly HttpClient http;

        public About()
        {
            this.http = new HttpClient();
        }



        #region version
        string clientVersionCurrent = "0.3.1";
        string clientVersion = "Loading";
        string courseVersion = "Loading";
        string scheduleVersion = "Loading";


        class VersionHandler
        {
            private string _clientVersion = "";
            private string _courseVersion = "";
            private string _scheduleVersion = "";


            public (string clientVersion, string courseVersion, string scheduleVersion) getVersion( string jsonString ) 
            { 
                getData( jsonString );
                
                return (clientVersion: _clientVersion, courseVersion: _courseVersion, scheduleVersion: _scheduleVersion);
            }

            private void getData( string jsonString )
            {

                // parse json
                JsonNode versionNode = JsonNode.Parse( jsonString )!;

                _clientVersion = versionNode!["ClassViewer"]!.GetValue<string>();
                _courseVersion = versionNode!["CourseData"]!.GetValue<string>();
                _scheduleVersion = versionNode!["CourseSchedule"]!.GetValue<string>();

            }

        }

        #endregion


        #region Load
        private async Task<string> getStringFromApi( string uri )
        {
            var response = await http.GetAsync( uri );

            return await response.Content.ReadAsStringAsync();
        }

        private async Task loadVersion()
        {
            string jsonString = await getStringFromApi(
                "https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/class-viewer/version.json" );


            var versionHandler = new VersionHandler();
            (clientVersion, courseVersion, scheduleVersion) = versionHandler.getVersion( jsonString );


            StateHasChanged();
        }

        #endregion


        public async Task reloadPage()
        {
            await JS.InvokeVoidAsync( "reload_page" );

        }


        protected override async Task OnInitializedAsync()
        {

            // load data
            await loadVersion();



		}






	}
}
