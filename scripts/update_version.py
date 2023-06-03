"""
update_version
~~~~~~~~~~~~
write the update time to json

"""


import pathlib
import json

import datetime
import zoneinfo



#region get current datetime

time_tz=zoneinfo.ZoneInfo("Asia/Taipei")
time_now = datetime.datetime.now(time_tz)

#endregion


#region read file

file_folder = pathlib.Path().resolve() / "data/class-viewer/"

file_path = file_folder / "version.json"
with open( file_path , mode='r' ) as file:
    
   version_json = json.load(file)

#endregion


#region update data

version_json["CourseData"] = f"{time_now:%Y-%m-%d %H:%M:%S}"
version_json["CourseSchedule"] = f"{time_now:%Y-%m-%d %H:%M:%S}"

#endregion


#region to json

result_json = json.dumps( version_json , indent=4 )


with open( file_path , mode='w' ) as file:
    
    print(result_json)
    file.write( result_json )

#endregion


