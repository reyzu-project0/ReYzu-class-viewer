using AntDesign;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;


namespace ReYzuClassViewer.Shared
{

	public class CascadeColorScheme
    {
		private Action<string>? _updateTheme { get; set; }
        private bool _isColorSchemeAuto;
        private bool _isColorSchemeDark;

		public void SetUpdateThemeFunction( Action<string>? updateTheme )
		{
			_updateTheme = updateTheme;
		}

        public bool IsColorSchemeAuto 
		{
            get => _isColorSchemeAuto;
            set
            {
				_isColorSchemeAuto = value;

                if ( value && _updateTheme is not null )
                {
					_updateTheme("auto");
                }

            }

        }

        public bool IsColorSchemeDark 
		{
            get => _isColorSchemeDark;
            set
            {
                _isColorSchemeDark = value;

                if ( !_isColorSchemeAuto && value && _updateTheme is not null)
                {
                    _updateTheme( "dark" );
					return;
                }

                if (!_isColorSchemeAuto && !value && _updateTheme is not null)
                {
                    _updateTheme( "light" );
                    return;
                }

            }

        }

    }


    public partial class MainLayout
    {

        #region Responsive
        // Handle Breakpoint
        AntDesign.BreakpointType BreakpointCurrent;
        AntDesign.BreakpointType[] BreakpointTypes = new[]
        {
            AntDesign.BreakpointType.Xxl, AntDesign.BreakpointType.Xl, AntDesign.BreakpointType.Lg,
            AntDesign.BreakpointType.Md, AntDesign.BreakpointType.Sm, AntDesign.BreakpointType.Xs
        };

        void BreakpointHandler( AntDesign.BreakpointType breakpoint )
        {
            BreakpointCurrent = breakpoint;
        }
		#endregion


		#region sidebar
		bool sidebarVisible = false;


		void SidebarOpen()
		{
			sidebarVisible = true;
		}

		void SidebarClose()
		{
			sidebarVisible = false;
		}

		#endregion


		#region ColorScheme
		bool isColorSchemeAuto = true;
		bool isColorSchemeDark = false;
        string selectedColorScheme = "";
		string selectedColorSchemeIcon = "bi bi-circle-half";

		CascadeColorScheme cascadeColorScheme = new CascadeColorScheme()
		{
            IsColorSchemeAuto = true,
			IsColorSchemeDark = false,
		};


		public class ColorScheme
		{
			public string Value { get; set; }
			public string Name { get; set; }
			public string Icon { get; set; }
		}

		List<ColorScheme> colorSchemes = new List<ColorScheme>
		{
			new ColorScheme { Value = "os_default", Name = "OS Default", Icon = "bi bi-circle-half"},
			new ColorScheme { Value = "light", Name = "Light", Icon = "bi bi-sun-fill"},
			new ColorScheme { Value = "dark", Name = "Dark", Icon = "bi bi-moon-stars-fill" },
		};


		private void OnSelectedColorScheme()
		{

            switch (selectedColorScheme)
            {
                case "os_default":
					isColorSchemeAuto = true;
					selectedColorSchemeIcon = "bi bi-circle-half";

                    cascadeColorScheme.IsColorSchemeAuto = isColorSchemeAuto;

                    break;

				case "light":
					isColorSchemeAuto = false;
					isColorSchemeDark = false;
					selectedColorSchemeIcon = "bi bi-sun-fill";

                    cascadeColorScheme.IsColorSchemeAuto = isColorSchemeAuto;
                    cascadeColorScheme.IsColorSchemeDark = isColorSchemeDark;

					break;

				case "dark":
					isColorSchemeAuto = false;
					isColorSchemeDark = true;
					selectedColorSchemeIcon = "bi bi-moon-stars-fill";

                    cascadeColorScheme.IsColorSchemeAuto = isColorSchemeAuto;
                    cascadeColorScheme.IsColorSchemeDark = isColorSchemeDark;

                    break;

				default:
					Console.WriteLine($"error - selected: {selectedColorScheme}");

					break;
            }

		}


        #endregion


        #region update

        private bool updateAvailable = false;


        private async Task RegisterUpdateAvailableNotification()
        {

            // registers the name of the C# method that needs to be called once an update is detected.
            await JS.InvokeAsync<object>(
                identifier: "registerForUpdateAvailableNotification",
                DotNetObjectReference.Create( this ),
                nameof( OnUpdateAvailable ) );

		}


        [JSInvokable( nameof( OnUpdateAvailable ) )]
        public Task OnUpdateAvailable()
        {
            updateAvailable = true;


			Console.WriteLine( "C# - update available" );
			StateHasChanged();

            return Task.CompletedTask;
		}

		public async Task reloadPage()
		{
			await JS.InvokeVoidAsync( "reload_page" );

		}


		#endregion ErrorBoundary

		private ErrorBoundary? errorBoundary;

		private void ShowErrorNotification( Exception ex )
		{
			// display error notification
			_notice.Error( new()
			{
				Message = ex.Message,
				Description = ex.StackTrace
			});

			// reset to a non-error state
			errorBoundary?.Recover();
		}


		#region




		#endregion




		protected override void OnInitialized()
		{

			RegisterUpdateAvailableNotification();

		}




	}
}
