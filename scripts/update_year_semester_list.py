"""
update_year_semester_list
~~~~~~~~~~~~
get the semester list from school api, merge old file and write to json

"""


import requests
import json

import pathlib



#region get from school api 

url = "https://portalfun.yzu.edu.tw/AcademicWebAPI/api/Cos/SelYM_CosCurrent"

response = requests.post(url)

#endregion


#region convert

# convert to Year,Semester
latest_list = []

for year_semester in json.loads(response.text):

    # "YM": "1112"
    # split str
    latest_list.append( { "Year": year_semester["YM"][:-1] , "Semester": year_semester["YM"][-1:] } )

#endregion


#region merge
"""merge old file because api only get the recent"""

# get old 
# path from root
old_file_path = pathlib.Path().resolve() / "data/course/year_semester_list.json"

with open( old_file_path ) as file:
    
    old_json = json.load(file)


# merge latest to old 
result_list = []

for year_semester in latest_list:

    # stop when reach the latest in old json
    if year_semester["Year"] == old_json[0]["Year"] and year_semester["Semester"] == old_json[0]["Semester"] :
        break
    else:
        result_list.append(year_semester)
        
result_list += old_json

#endregion


#region to json

result_json = json.dumps(result_list , indent=4)

# create folder for file
file_folder = pathlib.Path().resolve() / "data/course/"
file_folder.mkdir( parents=True , exist_ok=True )

# write to file
file_path = file_folder / "year_semester_list.json"
with open( file_path , mode='w' ) as file:
    
    print(result_json)
    file.write( result_json )


#endregion