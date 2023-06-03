"""
update_year_semester_current
~~~~~~~~~~~~
get the current year and semester from school api, and write to json

"""


import requests
import json

import pathlib



#region get from school api 

url = "https://portalfun.yzu.edu.tw/AcademicWebAPI/api/Cos/Select_Current_Smtr"

response = requests.post(url)

#endregion


#region to json

# modify name
json_school = json.loads(response.text)[0]
modified = { "Year": json_school["year"] , "Semester": json_school["smtr"] }

result_json = json.dumps( modified , indent=4 )


# create folder for file
file_folder = pathlib.Path().resolve() / "data/course/"
file_folder.mkdir( parents=True , exist_ok=True )

# write to file
file_path = file_folder / "year_semester_current.json"
with open( file_path , mode='w' ) as file:
    
    print(result_json)
    file.write( result_json )
   

#endregion



