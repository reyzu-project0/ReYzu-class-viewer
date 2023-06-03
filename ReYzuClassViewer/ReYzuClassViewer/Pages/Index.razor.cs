using System.Text.Json;
using System.Text.Json.Nodes;


namespace ReYzuClassViewer.Pages
{
    public class Schedule
    {
        public string EventName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class Announcement
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public string Url { get; set; }
    }

    public class Comment
    {
        // nick
        public string Name { get; set; }

        // url
        public string Url { get; set; }

        // orig
        public string TextContent { get; set; }

        // time
        public DateTime Time { get; set; }
    }


    public partial class Index
	{
        private readonly HttpClient http;

        public Index()
        {
            this.http = new HttpClient();
        }



        #region Course Schedule
        string scheduleState = "Loading";
        string scheduleName = "Loading";
        DateTimeOffset scheduleTime = DateTimeOffset.Now;

        class ScheduleHandler
        {
            private List<Schedule> _schedules = new List<Schedule>();

            public (string scheduleState, string scheduleName, DateTimeOffset scheduleTime) getSchedules( string jsonString )
            {

                getData(jsonString);

                var nowTime = DateTimeOffset.Now;


                foreach (var schedule in _schedules)
                {

                    // check if calendar api not publish
                    if (schedule.StartTime == "尚未公布")
                    {
                        return (scheduleState: "", scheduleName: "尚未公布", scheduleTime: nowTime);
                    }


                    var startTime = DateTimeOffset.Parse(schedule.StartTime);
                    var endTime = DateTimeOffset.Parse(schedule.EndTime);

                    // schedule not finished
                    if (nowTime < endTime)
                    {

                        // schedule not started
                        if (nowTime < startTime)
                        {
                            return (scheduleState: "下一個", scheduleName: schedule.EventName, scheduleTime: startTime);
                        }

                        // schedule is in current
                        if (nowTime > startTime)
                        {
                            return (scheduleState: "正在進行的", scheduleName: schedule.EventName, scheduleTime: endTime);
                        }

                    }

                }

                return (scheduleState: "下一個", scheduleName: "日程已結束", scheduleTime: nowTime);

            }

            private void getData( string jsonString )
            {
                
                // parse json
                JsonNode scheduleNode = JsonNode.Parse(jsonString)!;
                JsonArray scheduleArray = scheduleNode.AsArray();

                foreach (var schedule in scheduleArray)
                {
                    _schedules.Add(JsonSerializer.Deserialize<Schedule>(schedule)!);
                }

            }

        }

        #endregion


        #region Announcement
        List<Announcement> Announcements = new List<Announcement>();

        class AnnouncementHandler
        {
            private List<Announcement> _announcements = new List<Announcement>();

            public List<Announcement> getAnnouncements( string jsonString )
            {
                getData(jsonString);

                // get recent 3 announcements
                var announcementsRecent = _announcements
                    .Take(3)
                    .ToList();

                return announcementsRecent;
            }

            private void getData( string jsonString )
            {
                
                // parse json
                JsonNode announcementNode = JsonNode.Parse(jsonString)!;
                JsonArray announcementArray = announcementNode.AsArray();

                foreach (var announcement in announcementArray)
                {
                    _announcements.Add(JsonSerializer.Deserialize<Announcement>(announcement)!);
                }

            }

        }

        #endregion


        #region Comment
        int commentsCount = 0;
        List<Comment> Comments = new List<Comment>();

        class CommentHandler
        {
            private List<Comment> _comments = new List<Comment>();
            private int _commentsCount = 0;

            public List<Comment> getComments( string jsonString )
            {
                getData(jsonString);

                // get recent 2 comments
                var commentsRecent = _comments
                    .Take( 2 )
                    .ToList();

                return commentsRecent;
            }

            public int getCommentsCount( string jsonString )
            {
                getCountData(jsonString);

                return _commentsCount;
            }

            private void getData( string jsonString )
            {
                
                // parse json
                JsonNode commentNode = JsonNode.Parse(jsonString)!;
                JsonArray commentArray = commentNode["data"]!.AsArray();

                foreach (var commentItem in commentArray)
                {

                    var comment = new Comment()
                    {
                        Name = commentItem!["nick"]!.GetValue<string>(),
                        Url = commentItem!["url"]!.GetValue<string>(),
                        TextContent = commentItem!["orig"]!.GetValue<string>(),

                        // UTC milliseconds to local time
                        Time = DateTimeOffset.FromUnixTimeMilliseconds( commentItem!["time"]!.GetValue<long>() ).LocalDateTime,
                    };


                    _comments.Add(comment);

                }

            }

            private void getCountData( string jsonString )
            {
                
                // parse json
                JsonNode CountNode = JsonNode.Parse( jsonString )!;

                _commentsCount = CountNode!["data"]!.GetValue<int>();

            }

        }

		#endregion


		#region Loading

		bool scheduleLoading = true;
		bool commentLoading = true;
		bool announcementLoading = true;

		#endregion


		#region Load
		private async Task<string> getStringFromApi( string uri )
        {
            var response = await http.GetAsync( uri );
    
            return await response.Content.ReadAsStringAsync();
        }

        private async Task loadSchedule()
        {
            string jsonString = await getStringFromApi(
                "https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/class-viewer/course_schedule.json" );


            var scheduleHandler = new ScheduleHandler();
            (scheduleState, scheduleName, scheduleTime) = scheduleHandler.getSchedules( jsonString );


			scheduleLoading = false;
			StateHasChanged();
        }

        private async Task loadAnnouncement()
        {
            string jsonString = await getStringFromApi(
                "https://raw.githubusercontent.com/reyzu-project0/ReYzu-class-viewer/data/class-viewer/announcement.json" );


            var announcementHandler = new AnnouncementHandler();
            Announcements = announcementHandler.getAnnouncements( jsonString );


			announcementLoading = false;
			StateHasChanged();
        }

        private async Task loadComment()
        {

            // only get 5 comment
            string commentJsonString = await getStringFromApi(
                "https://re-yzu-waline.vercel.app/api/comment?type=recent&count=5" );

            string countJsonString = await getStringFromApi(
                "https://re-yzu-waline.vercel.app/api/comment?type=count" );


            var commentHandler = new CommentHandler();
            Comments = commentHandler.getComments( commentJsonString );
            commentsCount = commentHandler.getCommentsCount( countJsonString );


            commentLoading = false;
            StateHasChanged();
        }

        #endregion



        protected override void OnInitialized()
        {
            // load data
            // use skeleton when data loading
            loadSchedule();
            loadAnnouncement();
            loadComment();

        }




    }
}
