"""
update_rating_data
~~~~~~~~~~~~
get the rating data from MySQL, and write to json

"""

import os

import pymysql
import pymysql.cursors

import pandas as pd

import pathlib



#region get data from database

# Connect to the database
connection = pymysql.connect(host=os.environ["database_host"],
                             user=os.environ["database_user"],
                             password=os.environ["database_password"],
                             database=os.environ["database_database"],
                             charset='utf8mb4',
                             ssl_ca=R'/etc/ssl/certs/ca-certificates.crt',
                             cursorclass=pymysql.cursors.DictCursor)


# query database
with connection:
    
    with connection.cursor() as cursor:
        
        sql_query = """
        select 
            reaction0 , reaction1 , reaction2 , reaction3 , reaction4 , url 
        from 
            wl_Counter
        """

        cursor.execute(sql_query)
        result_query = cursor.fetchall()
        # [{'reaction0': 5, 'reaction1': None, 'reaction2': 0, 'reaction3': 7, 'reaction4': 1, 'url': '工程材料-沈家傑'},

#endregion


#region create dataframe

df = pd.DataFrame( data=result_query )

# preprocess
df.fillna(0, inplace=True)
df_converted = df.loc[ : , "reaction0":"reaction4" ].astype(int)

# url to CourseName,TeacherName
# 工程材料-沈家傑
def url_CourseName(cell):
    # 工程材料
    return cell[ :cell.find("-")  ]

def url_TeacherName(cell):
    # 沈家傑
    return cell[ cell.find("-") + 1 :  ]


df_converted["CourseName"] = df.loc[ : , "url"].apply( url_CourseName )
df_converted["TeacherName"] = df.loc[ : , "url"].apply( url_TeacherName )

#endregion


#region compute rating

# count
df_converted["CourseRatingCount"] = df_converted.loc[ : , "reaction0":"reaction4" ].sum(axis=1)
df_converted.loc[ : , "CourseRatingCount"].replace( 0 , None , inplace=True )


# average
def rating_average(row):
    
    weighted_rating = 0
    votes_num = row["CourseRatingCount"]

    if votes_num is None:

        return None

    # rating , 1 to 5
    for index in range(0,5):
        
        weighted_rating += (index+1) * row[ "reaction" + str(index) ]


    return round( weighted_rating / votes_num , 1 )

df_converted["CourseRating"] = df_converted.apply( rating_average , axis=1 )


# percentages 
def rating_percentages(row):
    
    votes_num = row["CourseRatingCount"]
    rating_percentages = []

    if votes_num is None:

        return None

    # rating , 1 to 5
    for index in range(0,5):

        percent = ( row[ "reaction" + str(index) ] / votes_num ) * 100

        percent_str = str( round( percent , 1 ) ) + "%"

        rating_percentages.append( percent_str )


    return rating_percentages


df_converted["CourseRatingPercentages"] = df_converted.apply( rating_percentages , axis=1 )

#endregion


#region to json

df_result = df_converted.loc[ : , "CourseName":"CourseRatingPercentages" ]
print(df_converted.dtypes)

result_json = df_result.to_json(orient="records" , force_ascii=False , indent=4 )


# create folder for file
file_folder = pathlib.Path().resolve() / "data/rating/"
file_folder.mkdir( parents=True , exist_ok=True )

# write to file
file_path = file_folder / "rating_data.json"
with open( file_path , mode='w' ) as file:
    
    print(result_json)
    file.write( result_json )


#endregion