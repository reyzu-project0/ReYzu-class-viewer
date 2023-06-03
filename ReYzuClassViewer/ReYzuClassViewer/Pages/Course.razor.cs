using AntDesign;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.JSInterop;
using ReYzuClassViewer.Shared;
using static ReYzuClassViewer.Shared.MainLayout;
using static System.Net.WebRequestMethods;

namespace ReYzuClassViewer.Pages
{
    public class YearSemester
    {
        public string Year { get; set; }
        public string Semester { get; set; }
        public string YearAndSemester { get; set; }
    }

    public class Department
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class Degree
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    class CourseData
    {
        public string CourseID { get; set; }
        public string CourseClass { get; set; }
        public int Degree { get; set; }
        public string CourseName { get; set; }
        public string TeacherName { get; set; }
        public string TeacherNameEn { get; set; }
        public string ClassTime { get; set; }
        public string TypeName { get; set; }

        public int? CourseRatingCount { get; set; }
        public float? CourseRating { get; set; }
        public List<string>? CourseRatingPercentages { get; set; }

    }

    public partial class Course
    {
        private readonly HttpClient http;

        public Course()
        {
            this.http = new HttpClient();
        }


        #region Semester

        List<YearSemester> semesters = new();
        YearSemester? selectedSemester;
        string labelSelectedSemester = "";

        
        class SemesterHandler
        {
            List<YearSemester> _semesters = new();


            public List<YearSemester> getSemesters( string jsonString )
            {
                getData( jsonString );

                return _semesters;
            }


            private void getData( string jsonString )
            {

                // parse json
                JsonNode semesterNode = JsonNode.Parse( jsonString )!;
                JsonArray semesterArray = semesterNode.AsArray();

                _semesters.Clear();
                foreach (var semesterItem in semesterArray)
                {
                    var semester = new YearSemester()
                    {
                        Year = semesterItem!["Year"]!.GetValue<string>(),
                        Semester = semesterItem!["Semester"]!.GetValue<string>(),
                        YearAndSemester = semesterItem!["Year"]!.GetValue<string>() + " " + semesterItem!["Semester"]!.GetValue<string>(),
                    };

                    _semesters.Add( semester );
                }

            }

        }


        private void OnSelectedSemesterChanged()
        {

            if (selectedSemester is null)
            {
				Console.WriteLine( "C# - selectedSemester is null" );
			}
            else
            {
				labelSelectedSemester = selectedSemester.YearAndSemester;

				loadCourseData();
			}

        }


        #endregion


        #region Department

        List<Department> departments = new();
        Department? selectedDepartment;
        string labelSelectedDepartment;


        class DepartmentHandler
        {
            List<Department> _departments = new();


            public List<Department> getDepartments( string jsonString )
            {
                getData( jsonString );

                return _departments;
            }


            private void getData( string jsonString )
            {

                // parse json
                JsonNode departmentNode = JsonNode.Parse( jsonString )!;
                JsonArray departmentArray = departmentNode.AsArray();

                _departments.Clear();
                foreach (var department in departmentArray)
                {
                    _departments.Add( JsonSerializer.Deserialize<Department>( department )! );
                }

            }

        }


        private void OnSelectedDepartmentChanged()
        {

			if (selectedDepartment is null)
			{
				Console.WriteLine( "C# - selectedDepartment is null" );
			}
            else
            {
				labelSelectedDepartment = selectedDepartment.Name;

				loadCourseData();
			}
			
        }


        #endregion


        #region Degree

        Degree? selectedDegree;
        string labelSelectedDegree = "";


        List<Degree> degrees = new List<Degree>
        {
            new Degree { ID = "1", Name = "1 年級" },
            new Degree { ID = "2", Name = "2 年級" },
            new Degree { ID = "3", Name = "3 年級" },
            new Degree { ID = "4", Name = "4 年級" },
            new Degree { ID = "0", Name = "全部" },
        };

        private void OnSelectedDegreeChanged()
        {
            labelSelectedDegree = selectedDegree.Name;
        }


        #endregion


        #region Course 

        List<CourseData> courseData = new List<CourseData>();


        class CourseDataHandler
        {
            private List<CourseData> _courseData = new();

            public List<CourseData> getCourseData( string jsonString )
            {
                getData( jsonString );

                return _courseData;
            }

            private void getData( string jsonString )
            {
                // parse json
                JsonNode CoursesNode = JsonNode.Parse( jsonString )!;

                // check if no Course Data
                if (CoursesNode is JsonObject)
                {
                    return;
                }

                JsonArray CoursesArray = CoursesNode.AsArray();

                _courseData.Clear();
                foreach (var course in CoursesArray)
                {
                    _courseData.Add( JsonSerializer.Deserialize<CourseData>( course )! );
                }

            }

        }



        private string getCourseUrl()
        {
            // https://portalfun.yzu.edu.tw/cosSelect/Cos_Plan.aspx?y=111&s=1&id=IM119&c=A

            string courseUrl = $"https://portalfun.yzu.edu.tw/cosSelect/Cos_Plan.aspx?" +
                $"y={selectedSemester?.Year}&s={selectedSemester?.Semester}&id={selectedCourseData?.CourseID}&c={selectedCourseData?.CourseClass}";

            return courseUrl;
        }

        #endregion


        #region course filters

        private bool courseFiltersVisible = false;


        private void onCourseFiltersVisibleChange( bool visible )
        {
            courseFiltersVisible = visible;
        }

        private void switchCourseFilters()
        {
            courseFiltersVisible = true ? false : true;

            StateHasChanged();
        }

        #endregion


        #region course sidebar

        bool sidebarCourseVisible = false;
        CourseData? selectedCourseData;


        private async Task openSidebarCourse( CourseData courseData )
        {
            sidebarCourseVisible = true;

            selectedCourseData = courseData;
            await updateWalinePath( courseData.CourseName + "-" + courseData.TeacherName );
        }

        private void onSidebarCourseClose()
        {
            sidebarCourseVisible = false;
        }

        #endregion


        #region ColorScheme

        [CascadingParameter( Name = "CascadeColorScheme" )]
        CascadeColorScheme cascadeColorScheme { get; set; } 

        #endregion


        #region Waline

        private IJSObjectReference? walineModule;
        private bool isWalineLoaded = false;


        private async Task loadWaline()
        {

            walineModule = await JS.InvokeAsync<IJSObjectReference>( "import", "./js/waline.js" );
            await walineModule.InvokeVoidAsync( "load" );

            isWalineLoaded = true;
            Console.WriteLine( "C# - waline loaded" );

        }

        private async void updateWalineTheme( string colorScheme )
        {
            if (isWalineLoaded)
            {
                await walineModule.InvokeVoidAsync( "updateWalineTheme", colorScheme );
                Console.WriteLine( "C# - waline theme updated" );
            }
            else
            {
                Console.WriteLine( "C# - waline not loaded" );
            }

        }

        private async Task updateWalinePath( string path )
        {
            if (isWalineLoaded)
            {
                await walineModule.InvokeVoidAsync( "updateWalinePath", path );
                Console.WriteLine( "C# - waline path updated" );
            }
            else
            {
                Console.WriteLine( "C# - waline not loaded" );
            }
            
        }

        private async Task disposeWaline()
        {
            if (isWalineLoaded)
            {
                await walineModule.InvokeVoidAsync( "destroyWaline" );
                Console.WriteLine( "C# - waline disposed" );
            }
            else
            {
                Console.WriteLine( "C# - waline not loaded" );
            }

        }

        #endregion


        #region Loading

        bool selectSemesterLoading = true;
        bool selectDepartmentLoading = true;

        bool tableLoading = false;


        #endregion


        #region Load

        private async Task<string> getStringFromApi( string uri )
        {
            var response = await http.GetAsync( uri );

            return await response.Content.ReadAsStringAsync();
        }

        private async Task loadCourseData()
        {
            tableLoading = true;


            string jsonString = "";
            if (selectedSemester is null)
            {
                // if not select
                jsonString = await getStringFromApi(
                    $"https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/course/" +
                    $"{semesters[0].Year}/{semesters[0].Semester}/{departments[0].ID}/course_data.json" );
            }
            else
            {
                jsonString = await getStringFromApi(
                    $"https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/course/" +
                    $"{selectedSemester.Year}/{selectedSemester.Semester}/{selectedDepartment.ID}/course_data.json" );
            }


            var courseDataHandler = new CourseDataHandler();
            courseData = courseDataHandler.getCourseData( jsonString );


            tableLoading = false;
            StateHasChanged();
        }

        private async Task loadSemester()
        {
            string jsonString = await getStringFromApi(
                "https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/course/year_semester_list.json" );


            var semesterHandler = new SemesterHandler();
            semesters = semesterHandler.getSemesters( jsonString );

            selectSemesterLoading = false;

            StateHasChanged();
        }

        private async Task loadDepartment()
        {
            string jsonString = await getStringFromApi(
                "https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/course/department_list.json" );


            var departmentHandler = new DepartmentHandler();
            departments = departmentHandler.getDepartments( jsonString );

            selectDepartmentLoading = false;

            StateHasChanged();
        }

        #endregion


        #region Lifecycle events

        protected override void OnInitialized()
        {
            // set function when property value changes
            cascadeColorScheme.SetUpdateThemeFunction( updateWalineTheme );

        }

        protected override async Task OnInitializedAsync()
        {

            await loadSemester();
            await loadDepartment();


            // set default to first value
            labelSelectedSemester = semesters[0].YearAndSemester;
            labelSelectedDepartment = departments[0].Name;
            labelSelectedDegree = degrees[0].Name;


            await loadCourseData();

        }

        protected override async Task OnAfterRenderAsync( bool firstRender )
        {
            if (firstRender)
            {
                // imports the JavaScript module
                await loadWaline();
            }

        }

        public async ValueTask DisposeAsync()
        {
            // remove function when property value changes
            cascadeColorScheme.SetUpdateThemeFunction( null );

            await disposeWaline();

            Console.WriteLine( "C# - page disposed" );
        }

        #endregion







    }
}
