"""
update_department_list
~~~~~~~~~~~~
get the department list from school api, and write to json

"""


import requests
import json

import pathlib



#region get from school api 

url = "https://portalfun.yzu.edu.tw/AcademicWebAPI/api/Cos/SelTechDep20"
payload = { "ShowLang": "zh" }

response = requests.post(url, data=payload)

#endregion


#region convert

# convert to ID,Name
result_list = []

for ID_Name in json.loads(response.text):

    # "DepName": "300 工學院"
    # split str
    id_str = ID_Name["DepName"][ :ID_Name["DepName"].find(" ")]
    name_str = ID_Name["DepName"][ ID_Name["DepName"].find(" ") + 1 : ]

    result_list.append( { "ID": id_str , "Name": name_str } )

#endregion


#region to json

result_json = json.dumps(result_list , indent=4 , ensure_ascii=False )


# create folder for file
file_folder = pathlib.Path().resolve() / "data/course/"
file_folder.mkdir( parents=True , exist_ok=True )

# write to file
file_path = file_folder / "department_list.json"
with open( file_path , mode='w' ) as file:
    
    print(result_json)
    file.write( result_json )


#endregion