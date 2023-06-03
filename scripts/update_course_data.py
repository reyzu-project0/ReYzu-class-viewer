"""
update_course_data
~~~~~~~~~~~~
get the course data from school api, and write to json

"""


import time

import requests
import json

import pandas as pd

import pathlib



file_folder = pathlib.Path().resolve() / "data/course/"

#region get year_semester_list

year_semester_list_path = file_folder / "year_semester_list.json"
with open( year_semester_list_path , mode='r' ) as file:
    
   year_semester_list = json.load(file)

#endregion


#region get year_semester_current

year_semester_current_path = file_folder / "year_semester_current.json"
with open( year_semester_current_path , mode='r' ) as file:
    
   year_semester_current_dict = json.load(file)

#endregion


#region get department list

department_list_path = file_folder / "department_list.json"

with open( department_list_path , mode='r' ) as file:
    
   department_list = json.load(file)

#endregion


#region update course

def get_course_data( year: str , semester: str , department_number: str  ) -> pd.DataFrame | None :
    """get course data from api"""

    df_course_data = pd.DataFrame()

    # 0 = all , not contain Degree
    for degree in range(1,5):
        
        url = "https://portalfun.yzu.edu.tw/AcademicWebAPI/api/Cos/Select_Cos_Smtrcos_dept"
        payload = {
        "year": year,
        "smtr": semester,
        "DepNo": department_number,
        "Degree": degree,
        "ShowLang": "zh"
        }
        
        # try to send request , limit 5 times
        for attempt in range(5):
            try:
                # API Rate Limiting
                time.sleep(0.1)

                response = requests.post(url, data=payload)
                response.raise_for_status()

            except requests.HTTPError as error:
                
                print( f"bad request , retry - {attempt+1}" , error )
                continue

            else:
                # http status success
                break
        
        else:

            print( "request failed" )
            raise RuntimeError()


        # response to json
        course_data_list = json.loads(response.text)

        # check if degree no course data
        if not course_data_list:
            continue

        # add to DataFrame
        df = pd.DataFrame(course_data_list)
        df["Degree"] = degree

        # concat all degree
        df_course_data = pd.concat( [ df_course_data , df ] ) 

    # check if department no course data
    if df_course_data.empty:

        return None
    
    df_course_data.reset_index(drop=True, inplace=True)

    return df_course_data

def create_course_data_department( year , semester , department_number ) :
    """create course data for single department"""
    
    # create folder - department
    department_path = file_folder / year / semester / department_number
    pathlib.Path.mkdir( department_path , exist_ok=True )
    
    # get course data
    df_course_data = get_course_data( year , semester , department_number  )

    # if no course data , set "CourseData=null" in course_data.json
    if df_course_data is None:

        print( "no course data  -" , year , semester , department_number )
        result_json = json.dumps( { "CourseData": None } , indent=4 )
        
    else:

        print( "course data -" , year , semester , department_number )
        result_json = df_course_data.to_json( orient="records" , force_ascii=False , indent=4 )

    # write to file
    course_data_path = file_folder / year / semester / department_number / "course_data.json"
    with open( course_data_path , mode='w' ) as file:
        
        file.write( result_json )


for year_semester in year_semester_list:

    _year = year_semester["Year"]
    _semester = year_semester["Semester"]


    # for test
    # if not ( _year == "112" and _semester == "1"):
    #     print("skip" ,_year , _semester )
    #     continue

    # create folder - year
    year_path = file_folder / _year
    pathlib.Path.mkdir( year_path , exist_ok=True )

    # create folder - semester
    semester_path = file_folder / _year / _semester
    pathlib.Path.mkdir( semester_path , exist_ok=True )

    
    for department in department_list:

        print( "year_semester -" , _year , _semester , "department -" , department["ID"] , department["Name"] )

        create_course_data_department( _year , _semester , department["ID"] )

    
    # stop when reach the current year_semester
    if _year == year_semester_current_dict["Year"] and _semester == year_semester_current_dict["Semester"] :
        print( "Finished - reach the current year_semester" )
        break


    # API Rate Limiting
    print( "Wait API Rate Limiting - year_semester" )
    time.sleep(10)


#endregion
