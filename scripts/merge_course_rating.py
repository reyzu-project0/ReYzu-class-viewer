"""
merge_course_rating
~~~~~~~~~~~~
merge the course data and rating data , then write to json

"""


from typing import Any

import json

import pandas as pd
import re

import pathlib



file_folder = pathlib.Path().resolve() / "data"

#region get year_semester_list

year_semester_list_path = file_folder / "course/year_semester_list.json"
with open( year_semester_list_path , mode='r' ) as file:
    
   year_semester_list = json.load(file)

#endregion


#region get department list

department_list_path = file_folder / "course/department_list.json"

with open( department_list_path , mode='r' ) as file:
    
   department_list = json.load(file)

#endregion


#region get rating data

rating_data_path = file_folder / "rating/rating_data.json"

with open( rating_data_path , mode='r' ) as file:
    
   rating_data_json = json.load(file)

df_rating_data = pd.DataFrame(rating_data_json)

#endregion


#region merge course and rating

def merge_rating( df_course_data: pd.DataFrame , df_rating_data: pd.DataFrame ) -> Any:
    """merge course_data and rating_data"""
    
    # rename index
    df_course_data.rename(columns={"CosID": "CourseID"} , inplace=True)
    df_course_data.rename(columns={"CosClass": "CourseClass"} , inplace=True)
    df_course_data.rename(columns={"TypeNa": "TypeName"} , inplace=True)
    df_course_data.rename(columns={"CosName": "CourseName"} , inplace=True)


    def split_TeacherName(cell): 

        # 沈家傑(Chia-Chieh Shen)
        if cell.find("(") != -1 :

            # 沈家傑
            return cell[  : cell.find("(")  ]
        
        else:
            
            # no english name
            return cell
        
    def split_TeacherName_en(cell):

        pattern = re.compile(R"\((?P<name_en>.*)\)")
        match = pattern.search( cell )

        # 沈家傑(Chia-Chieh Shen)
        if match :

            # Chia-Chieh Shen
            return match.group("name_en")
        
        else:

            # no english name
            return None


    # if already merge
    if "CourseRatingCount" in df_course_data.columns:

        # drop old
        df_course_data.drop( ["CourseRatingCount","CourseRating","CourseRatingPercentages"] ,axis="columns" , inplace=True )

    else:
        
        # split Teacher Name to TeacherName , TeacherNameEn
        df_course_data["TeacherNameEn"] = df_course_data.loc[ : , "TeacherNa"].apply( split_TeacherName_en )
        df_course_data["TeacherName"] = df_course_data.loc[ : , "TeacherNa"].apply( split_TeacherName )

        df_course_data.drop( "TeacherNa" ,axis="columns" , inplace=True )
 
    # prevent merge() convert int to float because unmatched records in the join (NaN)
    # https://stackoverflow.com/questions/38444480/how-to-prevent-pandas-from-converting-my-integers-to-floats-when-i-merge-two-dat
    # convert type to Int64 that can contain nan
    df_rating_data['CourseRatingCount'] = df_rating_data['CourseRatingCount'].astype('Int64')

    # merge course_data and rating_data
    # left join
    df_merge = df_course_data.merge( df_rating_data , how="left" )

    # reorder
    columns_order = ['CourseID','CourseClass','Degree',  'CourseName','TeacherName','TeacherNameEn','ClassTime',  'TypeName',  'CourseRatingCount','CourseRating','CourseRatingPercentages']
    df_merge = df_merge[ columns_order ]

    # to json
    result_json = df_merge.to_json( orient="records" , force_ascii=False , indent=4 )


    return result_json


for year_semester in year_semester_list:

    _year = year_semester["Year"]
    _semester = year_semester["Semester"]

    
    for department in department_list:

        course_data_path = file_folder / "course" / _year / _semester / department["ID"] / "course_data.json"

        # check whether course_data.json exists
        if course_data_path.exists():

            with open( course_data_path , mode='r' ) as file:
                
                course_data_json = json.load(file)

            # check if no course data
            if type(course_data_json) is dict:
                continue

            # merge
            df_course_data = pd.DataFrame( course_data_json )
            result_json = merge_rating( df_course_data , df_rating_data )
            print( "merge finished -" , _year , _semester , department["ID"] )


            with open( course_data_path , mode='w' ) as file:
                
                file.write( result_json )

        else:
            
            print( "Error - course data not exists" , _year , _semester , department["ID"] )
            raise RuntimeError("Error - course data not exists")


#endregion
